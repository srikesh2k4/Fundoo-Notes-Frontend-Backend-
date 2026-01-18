import { Component, signal, Output, EventEmitter, inject, ElementRef, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NoteService } from '../../../../core/services/note.service';
import { NOTE_COLORS } from '../note-card/note-card';

@Component({
  selector: 'app-take-note',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './take-note.html',
  styleUrls: ['./take-note.scss']
})
export class TakeNoteComponent {
  private noteService = inject(NoteService);
  private elementRef = inject(ElementRef);

  isExpanded = signal(false);
  noteTitle = '';
  noteContent = '';
  selectedColor = signal('#ffffff');
  isPinned = signal(false);
  showColorPicker = signal(false);
  isSaving = signal(false);

  @Output() noteCreated = new EventEmitter<void>();

  colors = NOTE_COLORS;

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isExpanded() && !this.elementRef.nativeElement.contains(event.target)) {
      this.saveAndClose();
    }
  }

  expand(): void {
    this.isExpanded.set(true);
  }

  collapse(): void {
    this.saveAndClose();
  }

  saveAndClose(): void {
    if (this.noteTitle.trim() || this.noteContent.trim()) {
      this.saveNote();
    } else {
      this.resetAndClose();
    }
  }

  saveNote(): void {
    if (this.isSaving()) return;

    const title = this.noteTitle.trim();
    const content = this.noteContent.trim();

    if (!title && !content) {
      this.resetAndClose();
      return;
    }

    this.isSaving.set(true);

    this.noteService.createNote({
      title: title || undefined,
      content: content || undefined,
      color: this.selectedColor()
    }).subscribe({
      next: (note) => {
        // If pinned, toggle pin on the created note
        if (this.isPinned()) {
          this.noteService.togglePin(note.id).subscribe();
        }
        this.noteCreated.emit();
        this.resetAndClose();
      },
      error: (err) => {
        console.error('Failed to create note:', err);
        this.isSaving.set(false);
      }
    });
  }

  resetAndClose(): void {
    this.isExpanded.set(false);
    this.noteTitle = '';
    this.noteContent = '';
    this.selectedColor.set('#ffffff');
    this.isPinned.set(false);
    this.showColorPicker.set(false);
    this.isSaving.set(false);
  }

  togglePin(): void {
    this.isPinned.update(v => !v);
  }

  toggleColorPicker(event: Event): void {
    event.stopPropagation();
    this.showColorPicker.update(v => !v);
  }

  selectColor(color: string, event: Event): void {
    event.stopPropagation();
    this.selectedColor.set(color);
    this.showColorPicker.set(false);
  }
}
