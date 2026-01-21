import { Component, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
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
export class ArchiveComponent implements OnInit, OnDestroy {
  private noteService = inject(NoteService);
  private notesSubscription?: Subscription;

  sidebarExpanded = signal(false);
  isGridView = signal(true);
  searchQueryValue = '';
  archivedNotes = signal<Note[]>([]);
  filteredNotes = signal<Note[]>([]);
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

  private setupNotesSubscription(): void {
    this.isLoading.set(true);
    this.notesSubscription = this.noteService.notes$.subscribe(notes => {
      const archived = notes.filter(n => n.isArchived && !n.isDeleted);
      this.archivedNotes.set(archived);
      this.filterNotes();
      this.isLoading.set(false);
    });
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

  filterNotes(): void {
    let notes = this.archivedNotes();
    const query = this.searchQueryValue.toLowerCase().trim();

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
    this.noteService.refreshNotes();
  }

  onNoteClick(note: Note): void {
    this.selectedNote.set(note);
    this.showEditDialog.set(true);
  }

  closeEditDialog(): void {
    this.showEditDialog.set(false);
    this.selectedNote.set(null);
    this.noteService.refreshNotes();
  }
}
