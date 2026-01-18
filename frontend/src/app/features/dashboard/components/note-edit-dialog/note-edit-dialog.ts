import { Component, Input, Output, EventEmitter, signal, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NoteService } from '../../../../core/services/note.service';
import { Note } from '../../../../core/models/note.model';
import { NOTE_COLORS } from '../note-card/note-card';

@Component({
  selector: 'app-note-edit-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './note-edit-dialog.html',
  styleUrls: ['./note-edit-dialog.scss']
})
export class NoteEditDialogComponent implements OnInit {
  private noteService = inject(NoteService);

  @Input() note!: Note;
  @Output() close = new EventEmitter<void>();

  editTitle = '';
  editContent = '';
  selectedColor = signal('#ffffff');
  showColorPicker = signal(false);
  showMoreMenu = signal(false);
  showLabelPicker = signal(false);
  isSaving = signal(false);

  colors = NOTE_COLORS;

  ngOnInit(): void {
    if (this.note) {
      this.editTitle = this.note.title || '';
      this.editContent = this.note.content || '';
      this.selectedColor.set(this.note.color || '#ffffff');
    }
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
  }

  closeMenus(): void {
    this.showColorPicker.set(false);
    this.showMoreMenu.set(false);
    this.showLabelPicker.set(false);
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
