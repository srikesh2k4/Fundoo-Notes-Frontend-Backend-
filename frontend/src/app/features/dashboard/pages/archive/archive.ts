import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavbarComponent } from '../../components/navbar/navbar';
import { SidebarComponent } from '../../components/sidebar/sidebar';
import { NoteCardComponent } from '../../components/note-card/note-card';
import { NoteEditDialogComponent } from '../../components/note-edit-dialog/note-edit-dialog';
import { NoteService } from '../../../../core/services/note.service';
import { Note } from '../../../../core/models/note.model';

@Component({
  selector: 'app-archive',
  standalone: true,
  imports: [CommonModule, NavbarComponent, SidebarComponent, NoteCardComponent, NoteEditDialogComponent],
  templateUrl: './archive.html',
  styleUrls: ['./archive.scss']
})
export class ArchiveComponent implements OnInit {
  private noteService = inject(NoteService);

  sidebarExpanded = signal(false);
  isGridView = signal(true);
  searchQuery = signal('');
  archivedNotes = signal<Note[]>([]);
  filteredNotes = signal<Note[]>([]);
  isLoading = signal(true);

  selectedNote = signal<Note | null>(null);
  showEditDialog = signal(false);

  ngOnInit(): void {
    this.loadArchivedNotes();
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

  loadArchivedNotes(): void {
    this.isLoading.set(true);
    this.noteService.notes$.subscribe(notes => {
      const archived = notes.filter(n => n.isArchived && !n.isDeleted);
      this.archivedNotes.set(archived);
      this.filterNotes();
      this.isLoading.set(false);
    });
    this.noteService.refreshNotes();
  }

  filterNotes(): void {
    let notes = this.archivedNotes();
    const query = this.searchQuery().toLowerCase().trim();

    if (query) {
      notes = notes.filter(n =>
        (n.title && n.title.toLowerCase().includes(query)) ||
        (n.content && n.content.toLowerCase().includes(query)) ||
        (n.labels && n.labels.some(l => l.name.toLowerCase().includes(query)))
      );
    }

    this.filteredNotes.set(notes);
  }

  onNoteUpdated(): void {
    this.loadArchivedNotes();
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
