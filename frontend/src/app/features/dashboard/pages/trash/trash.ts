import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
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
export class TrashComponent implements OnInit {
  private noteService = inject(NoteService);

  sidebarExpanded = signal(false);
  isGridView = signal(true);
  searchQuery = signal('');
  trashedNotes = signal<Note[]>([]);
  filteredNotes = signal<Note[]>([]);
  isLoading = signal(true);

  ngOnInit(): void {
    this.loadTrashedNotes();
  }

  toggleSidebar(): void {
    this.sidebarExpanded.update(value => !value);
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query);
    this.filterNotes();
  }

  onViewModeChange(isGrid: boolean): void {
    this.isGridView.set(isGrid);
  }

  loadTrashedNotes(): void {
    this.isLoading.set(true);
    this.noteService.getTrashedNotes().subscribe({
      next: (notes) => {
        this.trashedNotes.set(notes);
        this.filterNotes();
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Failed to load trashed notes:', err);
        this.trashedNotes.set([]);
        this.filteredNotes.set([]);
        this.isLoading.set(false);
      }
    });
  }

  filterNotes(): void {
    let notes = this.trashedNotes();
    const query = this.searchQuery().toLowerCase().trim();

    if (query) {
      notes = notes.filter(n =>
        (n.title && n.title.toLowerCase().includes(query)) ||
        (n.content && n.content.toLowerCase().includes(query))
      );
    }

    this.filteredNotes.set(notes);
  }

  restoreNote(note: Note): void {
    this.noteService.restoreFromTrash(note.id).subscribe({
      next: () => {
        this.trashedNotes.update(notes => notes.filter(n => n.id !== note.id));
        this.filterNotes();
      },
      error: (err) => console.error('Failed to restore note:', err)
    });
  }

  deletePermanently(note: Note): void {
    if (confirm('This will permanently delete the note. Are you sure?')) {
      this.noteService.deletePermanently(note.id).subscribe({
        next: () => {
          this.trashedNotes.update(notes => notes.filter(n => n.id !== note.id));
          this.filterNotes();
        },
        error: (err) => console.error('Failed to delete note:', err)
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
        error: (err) => console.error('Failed to empty trash:', err)
      });
    }
  }

  onNoteUpdated(): void {
    this.loadTrashedNotes();
  }
}
