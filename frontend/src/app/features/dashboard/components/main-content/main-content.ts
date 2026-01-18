import { Component, OnInit, Input, signal, inject, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TakeNoteComponent } from '../take-note/take-note';
import { NoteCardComponent } from '../note-card/note-card';
import { NoteEditDialogComponent } from '../note-edit-dialog/note-edit-dialog';
import { NoteService } from '../../../../core/services/note.service';
import { Note } from '../../../../core/models/note.model';

@Component({
  selector: 'app-main-content',
  standalone: true,
  imports: [CommonModule, TakeNoteComponent, NoteCardComponent, NoteEditDialogComponent],
  templateUrl: './main-content.html',
  styleUrls: ['./main-content.scss']
})
export class MainContentComponent implements OnInit, OnChanges {
  @Input() sidebarExpanded = false;
  @Input() isGridView = true;
  @Input() searchQuery = '';

  private noteService = inject(NoteService);

  allNotes = signal<Note[]>([]);
  pinnedNotes = signal<Note[]>([]);
  otherNotes = signal<Note[]>([]);
  isLoading = signal(true);

  selectedNote = signal<Note | null>(null);
  showEditDialog = signal(false);

  ngOnInit(): void {
    this.loadNotes();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['searchQuery']) {
      this.filterNotes();
    }
  }

  loadNotes(): void {
    this.isLoading.set(true);
    this.noteService.notes$.subscribe(notes => {
      // Store all active notes
      this.allNotes.set(notes.filter(n => !n.isDeleted && !n.isArchived));
      this.filterNotes();
      this.isLoading.set(false);
    });
    this.noteService.refreshNotes();
  }

  filterNotes(): void {
    let notes = this.allNotes();

    // Apply search filter
    if (this.searchQuery && this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase().trim();
      notes = notes.filter(n =>
        (n.title && n.title.toLowerCase().includes(query)) ||
        (n.content && n.content.toLowerCase().includes(query)) ||
        (n.labels && n.labels.some(l => l.name.toLowerCase().includes(query)))
      );
    }

    // Separate pinned and others
    const pinned = notes.filter(n => n.isPinned);
    const others = notes.filter(n => !n.isPinned);
    this.pinnedNotes.set(pinned);
    this.otherNotes.set(others);
  }

  onNoteCreated(): void {
    // Notes are automatically updated via the service's BehaviorSubject
  }

  onNoteUpdated(): void {
    // Notes are automatically updated via the service's BehaviorSubject
  }

  onNoteClick(note: Note): void {
    this.selectedNote.set(note);
    this.showEditDialog.set(true);
  }

  closeEditDialog(): void {
    this.showEditDialog.set(false);
    this.selectedNote.set(null);
  }
}
