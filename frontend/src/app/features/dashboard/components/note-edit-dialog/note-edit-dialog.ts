import { Component, Input, Output, EventEmitter, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NoteService } from '../../../../core/services/note.service';
import { LabelService } from '../../../../core/services/label.service';
import { Note, LabelDto } from '../../../../core/models/note.model';
import { Label } from '../../../../core/models/label.model';

// Google Keep color palette
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
  selector: 'app-note-edit-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './note-edit-dialog.html',
  styleUrls: ['./note-edit-dialog.scss']
})
export class NoteEditDialogComponent implements OnInit {
  private noteService = inject(NoteService);
  private labelService = inject(LabelService);

  @Input() note!: Note;
  @Output() close = new EventEmitter<void>();

  editTitle = '';
  editContent = '';
  selectedColor = signal('#FFFFFF');
  labelSearchQuery = signal('');
  showColorPicker = signal(false);
  showMoreMenu = signal(false);
  showLabelPicker = signal(false);
  isSaving = signal(false);
  allLabels = signal<Label[]>([]);

  colors = NOTE_COLORS;

  ngOnInit(): void {
    if (this.note) {
      this.editTitle = this.note.title || '';
      this.editContent = this.note.content || '';
      this.selectedColor.set(this.note.color || '#FFFFFF');
    }
    
    // Load all labels
    this.labelService.labels$.subscribe(labels => {
      this.allLabels.set(labels);
    });
    this.labelService.refreshLabels();
  }

  saveAndClose(): void {
    if (this.isSaving()) return;

    this.isSaving.set(true);

    // Update note
    this.noteService.updateNote(this.note.id, {
      title: this.editTitle.trim() || undefined,
      content: this.editContent.trim() || undefined
    }).subscribe({
      next: () => {
        // If color changed, update it
        if (this.selectedColor() !== this.note.color) {
          this.noteService.updateColor(this.note.id, { color: this.selectedColor() }).subscribe({
            next: () => this.close.emit(),
            error: () => this.close.emit()
          });
        } else {
          this.close.emit();
        }
      },
      error: (err) => {
        console.error('Failed to update note:', err);
        this.isSaving.set(false);
      }
    });
  }

  togglePin(): void {
    this.noteService.togglePin(this.note.id).subscribe({
      next: (updatedNote) => {
        this.note = updatedNote;
      }
    });
  }

  toggleArchive(): void {
    this.noteService.toggleArchive(this.note.id).subscribe({
      next: () => {
        this.close.emit();
      }
    });
  }

  deleteNote(): void {
    this.noteService.toggleTrash(this.note.id).subscribe({
      next: () => {
        this.close.emit();
      }
    });
  }

  toggleColorPicker(event: Event): void {
    event.stopPropagation();
    this.showColorPicker.update(v => !v);
    this.showMoreMenu.set(false);
    this.showLabelPicker.set(false);
  }

  selectColor(color: string, event: Event): void {
    event.stopPropagation();
    this.selectedColor.set(color);
    this.showColorPicker.set(false);
    
    // Save color immediately
    this.noteService.updateColor(this.note.id, { color }).subscribe({
      next: () => {
        this.note.color = color;
      },
      error: (err) => console.error('Failed to update color:', err)
    });
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
        this.toggleLabel(newLabel, event);
        this.labelSearchQuery.set('');
      },
      error: (err) => console.error('Failed to create label:', err)
    });
  }

  closeMenus(): void {
    this.showColorPicker.set(false);
    this.showMoreMenu.set(false);
    this.showLabelPicker.set(false);
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
        },
        error: (err) => console.error('Failed to remove label:', err)
      });
    } else {
      this.noteService.addLabelToNote(this.note.id, label.id).subscribe({
        next: (updatedNote) => {
          this.note.labels = updatedNote.labels;
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
      },
      error: (err) => console.error('Failed to remove label:', err)
    });
  }

  copyNote(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    this.noteService.copyNote(this.note).subscribe({
      next: () => this.close.emit(),
      error: (err) => console.error('Failed to copy note:', err)
    });
  }

  addDrawing(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    console.log('Add drawing feature - to be implemented');
  }

  showCheckboxes(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    console.log('Show checkboxes feature - to be implemented');
  }

  copyToGoogleDocs(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    console.log('Copy to Google Docs feature - to be implemented');
  }

  versionHistory(event: Event): void {
    event.stopPropagation();
    this.closeMenus();
    console.log('Version history feature - to be implemented');
  }

  onOverlayClick(event: Event): void {
    if ((event.target as HTMLElement).classList.contains('dialog-overlay')) {
      this.saveAndClose();
    }
  }
}
