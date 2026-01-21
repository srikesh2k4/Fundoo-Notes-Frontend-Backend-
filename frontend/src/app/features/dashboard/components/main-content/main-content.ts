import { Component, OnInit, Input, signal, inject, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
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
export class MainContentComponent implements OnInit, OnChanges, OnDestroy {
  @Input() sidebarExpanded = false;
  @Input() isGridView = true;
  @Input() searchQuery = '';

  private noteService = inject(NoteService);
  private notesSubscription?: Subscription;

  allNotes = signal<Note[]>([]);
  pinnedNotes = signal<Note[]>([]);
  otherNotes = signal<Note[]>([]);
  isLoading = signal(true);

  selectedNote = signal<Note | null>(null);
  showEditDialog = signal(false);

  ngOnInit(): void {
    this.setupNotesSubscription();
    this.noteService.refreshNotes();
  }

  ngOnDestroy(): void {
    this.notesSubscription?.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['searchQuery']) {
      this.filterNotes();
    }
  }

  private setupNotesSubscription(): void {
    this.isLoading.set(true);
    this.notesSubscription = this.noteService.notes$.subscribe(notes => {
      this.allNotes.set(notes.filter(n => !n.isDeleted && !n.isArchived));
      this.filterNotes();
      this.isLoading.set(false);
    });
  }

  filterNotes(): void {
    let notes = this.allNotes();

    if (this.searchQuery && this.searchQuery.trim()) {
      const query = this.searchQuery.toLowerCase().trim();
      notes = notes.filter(n =>
        (n.title && n.title.toLowerCase().includes(query)) ||
        (n.content && n.content.toLowerCase().includes(query)) ||
        (n.labels && n.labels.some(l => l.name.toLowerCase().includes(query)))
      );
    }

    const pinned = notes.filter(n => n.isPinned);
    const others = notes.filter(n => !n.isPinned);
    this.pinnedNotes.set(pinned);
    this.otherNotes.set(others);
  }

  onNoteCreated(): void {
    // Notes are automatically updated via the BehaviorSubject subscription
    // Just trigger a refresh to get the latest from server
    this.noteService.refreshNotes();
  }

  onNoteUpdated(): void {
    // Notes are automatically updated via the BehaviorSubject subscription
    // Just trigger a refresh to get the latest from server
    this.noteService.refreshNotes();
  }

  onNoteClick(note: Note): void {
    this.selectedNote.set(note);
    this.showEditDialog.set(true);
  }

  closeEditDialog(): void {
    this.showEditDialog.set(false);
    this.selectedNote.set(null);
    // Refresh notes after closing the edit dialog to reflect any changes
    this.noteService.refreshNotes();
  }
}
