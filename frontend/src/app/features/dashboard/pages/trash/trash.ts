import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { NavbarComponent } from '../../components/navbar/navbar';
import { SidebarComponent } from '../../components/sidebar/sidebar';
import { NoteCardComponent } from '../../components/note-card/note-card';
import { NoteService } from '../../../../core/services/note.service';
import { Note } from '../../../../core/models/note.model';

@Component({
  selector: 'app-trash',
  standalone: true,
  imports: [CommonModule, NavbarComponent, SidebarComponent, NoteCardComponent],
  templateUrl: './trash.html',
  styleUrls: ['./trash.scss']
})
export class TrashComponent implements OnInit, OnDestroy {
  private noteService = inject(NoteService);
  private trashSubscription?: Subscription;

  sidebarExpanded = signal(false);
  isGridView = signal(true);
  searchQueryValue = '';
  trashedNotes = signal<Note[]>([]);
  filteredNotes = signal<Note[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadTrashedNotes();
  }

  ngOnDestroy(): void {
    this.trashSubscription?.unsubscribe();
  }

  toggleSidebar(): void {
    this.sidebarExpanded.update(value => !value);
  }

  onSearchChange(query: string): void {
    this.searchQueryValue = query;
    this.filterNotes();
  }

  onViewModeChange(isGrid: boolean): void {
    this.isGridView.set(isGrid);
  }

  loadTrashedNotes(): void {
    this.isLoading.set(true);
    this.noteService.getTrashedNotes().subscribe({
      next: (response) => {
        const notes = response.data || [];
        this.trashedNotes.set(notes);
        this.filterNotes();
        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Failed to load trashed notes:', err);
        this.trashedNotes.set([]);
        this.filteredNotes.set([]);
        this.isLoading.set(false);
      }
    });
  }

  filterNotes(): void {
    let notes = this.trashedNotes();
    const query = this.searchQueryValue.toLowerCase().trim();

    if (query) {
      notes = notes.filter(n =>
        (n.title && n.title.toLowerCase().includes(query)) ||
        (n.content && n.content.toLowerCase().includes(query))
      );
    }

    this.filteredNotes.set(notes);
  }

  restoreNote(note: Note): void {
    this.noteService.restoreNote(note.id).subscribe({
      next: () => {
        this.trashedNotes.update(notes => notes.filter(n => n.id !== note.id));
        this.filterNotes();
      },
      error: (err: any) => console.error('Failed to restore note:', err)
    });
  }

  permanentlyDelete(note: Note): void {
    if (confirm('This will permanently delete the note. Are you sure?')) {
      this.noteService.permanentlyDelete(note.id).subscribe({
        next: () => {
          this.trashedNotes.update(notes => notes.filter(n => n.id !== note.id));
          this.filterNotes();
        },
        error: (err: any) => console.error('Failed to delete note:', err)
      });
    }
  }

  emptyTrash(): void {
    if (this.trashedNotes().length === 0) return;

    if (confirm('This will permanently delete all notes in trash. Are you sure?')) {
      this.noteService.emptyTrash().subscribe({
        next: () => {
          this.trashedNotes.set([]);
          this.filteredNotes.set([]);
        },
        error: (err: any) => console.error('Failed to empty trash:', err)
      });
    }
  }

  onNoteUpdated(): void {
    this.loadTrashedNotes();
  }
}
