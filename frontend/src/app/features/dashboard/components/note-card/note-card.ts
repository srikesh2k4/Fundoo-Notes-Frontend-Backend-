import { Component, Input, Output, EventEmitter, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NoteService } from '../../../../core/services/note.service';
import { LabelService } from '../../../../core/services/label.service';
import { Note, LabelDto } from '../../../../core/models/note.model';
import { Label } from '../../../../core/models/label.model';

// Google Keep color palette - must match backend validation
export const NOTE_COLORS = [
  { name: 'Default', value: '#FFFFFF' },
  { name: 'Red', value: '#F28B82' },
  { name: 'Orange', value: '#FBBC04' },
  { name: 'Yellow', value: '#FFF475' },
  { name: 'Green', value: '#CCFF90' },
  { name: 'Teal', value: '#A7FFEB' },
  { name: 'Blue', value: '#CBF0F8' },
  { name: 'Dark Blue', value: '#AECBFA' },
  { name: 'Purple', value: '#D7AEFB' },
  { name: 'Pink', value: '#FDCFE8' },
  { name: 'Brown', value: '#E6C9A8' },
  { name: 'Gray', value: '#E8EAED' }
];

@Component({
  selector: 'app-note-card',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './note-card.html',
  styleUrls: ['./note-card.scss']
})
export class NoteCardComponent implements OnInit {
  private noteService = inject(NoteService);
  private labelService = inject(LabelService);

  @Input() note!: Note;
  @Input() isTrash = false;
  @Input() isArchive = false;
  @Output() noteUpdated = new EventEmitter<void>();
  @Output() noteClicked = new EventEmitter<Note>();

  showColorPicker = signal(false);
  showMoreMenu = signal(false);
  showLabelPicker = signal(false);
  labelSearchQuery = signal('');
  allLabels = signal<Label[]>([]);
  colors = NOTE_COLORS;

  ngOnInit(): void {
    this.labelService.labels$.subscribe(labels => {
      this.allLabels.set(labels);
    });
  }

  onCardClick(): void {
    if (!this.isTrash) {
      this.noteClicked.emit(this.note);
    }
  }

  togglePin(event: Event): void {
    event.stopPropagation();
    this.noteService.togglePin(this.note.id).subscribe({
      next: (updatedNote) => {
        this.note = updatedNote;
        this.noteUpdated.emit();
      },
      error: (err) => console.error('Failed to toggle pin:', err)
    });
  }

  toggleArchive(event: Event): void {
    event.stopPropagation();
    this.noteService.toggleArchive(this.note.id).subscribe({
      next: (updatedNote) => {
        this.note = updatedNote;
        this.noteUpdated.emit();
      },
      error: (err) => console.error('Failed to toggle archive:', err)
    });
  }

  // Move note to trash (soft delete)
  moveToTrash(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    this.noteService.toggleTrash(this.note.id).subscribe({
      next: () => this.noteUpdated.emit(),
      error: (err) => console.error('Failed to move to trash:', err)
    });
  }

  // Restore note from trash
  restoreNote(event: Event): void {
    event.stopPropagation();
    this.noteService.restoreFromTrash(this.note.id).subscribe({
      next: () => this.noteUpdated.emit(),
      error: (err) => console.error('Failed to restore note:', err)
    });
  }

  // Permanently delete note
  deletePermanently(event: Event): void {
    event.stopPropagation();
    if (confirm('Delete note forever?')) {
      this.noteService.deletePermanently(this.note.id).subscribe({
        next: () => this.noteUpdated.emit(),
        error: (err) => console.error('Failed to delete permanently:', err)
      });
    }
  }

  // Legacy deleteNote - now moves to trash
  deleteNote(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    this.noteService.toggleTrash(this.note.id).subscribe({
      next: () => this.noteUpdated.emit(),
      error: (err) => console.error('Failed to delete note:', err)
    });
  }

  // Copy note
  copyNote(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    this.noteService.copyNote(this.note).subscribe({
      next: () => this.noteUpdated.emit(),
      error: (err) => console.error('Failed to copy note:', err)
    });
  }

  updateColor(color: string, event: Event): void {
    event.stopPropagation();
    console.log('Updating color to:', color);
    this.noteService.updateColor(this.note.id, { color }).subscribe({
      next: (updatedNote) => {
        this.note = updatedNote;
        this.showColorPicker.set(false);
        this.noteUpdated.emit();
      },
      error: (err) => {
        console.error('Failed to update color:', err);
        // Fallback: update locally anyway to allow user to see change, 
        // even if backend is being strict (temporary fix for UX)
        this.note.color = color;
        this.showColorPicker.set(false);
      }
    });
  }

  toggleColorPicker(event: Event): void {
    event.stopPropagation();
    this.showColorPicker.update(v => !v);
    this.showMoreMenu.set(false);
    this.showLabelPicker.set(false);
  }

  toggleMoreMenu(event: Event): void {
    event.stopPropagation();
    this.showMoreMenu.update(v => !v);
    this.showColorPicker.set(false);
    this.showLabelPicker.set(false);
  }

  toggleLabelPicker(event: Event): void {
    event.stopPropagation();
    this.showLabelPicker.update(v => !v);
    this.showColorPicker.set(false);
    this.showMoreMenu.set(false);
    this.labelSearchQuery.set('');
  }

  get filteredLabels(): Label[] {
    const query = this.labelSearchQuery().toLowerCase().trim();
    if (!query) return this.allLabels();
    return this.allLabels().filter(l => l.name.toLowerCase().includes(query));
  }

  createLabel(name: string, event: Event): void {
    event.stopPropagation();
    if (!name.trim()) return;

    this.labelService.createLabel({ name: name.trim() }).subscribe({
      next: (newLabel) => {
        // Add to note immediately
        this.toggleLabel(newLabel, event);
        this.labelSearchQuery.set('');
      },
      error: (err) => console.error('Failed to create label:', err)
    });
  }

  isLabelAttached(labelId: number): boolean {
    return this.note.labels?.some(l => l.id === labelId) ?? false;
  }

  toggleLabel(label: Label, event: Event): void {
    event.stopPropagation();
    if (this.isLabelAttached(label.id)) {
      this.noteService.removeLabelFromNote(this.note.id, label.id).subscribe({
        next: () => {
          this.note.labels = this.note.labels?.filter(l => l.id !== label.id);
          this.noteUpdated.emit();
        },
        error: (err) => console.error('Failed to remove label:', err)
      });
    } else {
      this.noteService.addLabelToNote(this.note.id, label.id).subscribe({
        next: (updatedNote) => {
          this.note = updatedNote;
          this.noteUpdated.emit();
        },
        error: (err) => console.error('Failed to add label:', err)
      });
    }
  }

  removeLabel(label: LabelDto, event: Event): void {
    event.stopPropagation();
    this.noteService.removeLabelFromNote(this.note.id, label.id).subscribe({
      next: () => {
        this.note.labels = this.note.labels?.filter(l => l.id !== label.id);
        this.noteUpdated.emit();
      },
      error: (err) => console.error('Failed to remove label:', err)
    });
  }

  // Add drawing (placeholder - not implemented in backend)
  addDrawing(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    console.log('Add drawing feature - to be implemented');
    // TODO: Implement drawing feature
  }

  // Show checkboxes (placeholder - not implemented in backend)
  showCheckboxes(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    console.log('Show checkboxes feature - to be implemented');
    // TODO: Implement checkbox list feature
  }

  // Copy to Google Docs (placeholder - not implemented)
  copyToGoogleDocs(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    console.log('Copy to Google Docs feature - to be implemented');
    // TODO: Implement Google Docs integration
  }

  // Version history (placeholder - not implemented in backend)
  versionHistory(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    console.log('Version history feature - to be implemented');
    // TODO: Implement version history feature
  }

  closeMenus(): void {
    this.showColorPicker.set(false);
    this.showMoreMenu.set(false);
    this.showLabelPicker.set(false);
  }
}
