import { Component, OnInit, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { NavbarComponent } from '../../components/navbar/navbar';
import { SidebarComponent } from '../../components/sidebar/sidebar';
import { NoteCardComponent } from '../../components/note-card/note-card';
import { NoteEditDialogComponent } from '../../components/note-edit-dialog/note-edit-dialog';
import { NoteService } from '../../../../core/services/note.service';
import { Note } from '../../../../core/models/note.model';

@Component({
  selector: 'app-label-notes',
  standalone: true,
  imports: [CommonModule, NavbarComponent, SidebarComponent, NoteCardComponent, NoteEditDialogComponent],
  templateUrl: './label-notes.html',
  styleUrls: ['./label-notes.scss']
})
export class LabelNotesComponent implements OnInit {
  private noteService = inject(NoteService);
  private route = inject(ActivatedRoute);

  sidebarExpanded = signal(false);
  labelName = signal('');
  labelNotes = signal<Note[]>([]);
  isLoading = signal(true);

  selectedNote = signal<Note | null>(null);
  showEditDialog = signal(false);

  isGridView = signal(true);
  searchQuery = signal('');

  filteredNotes = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const notes = this.labelNotes();

    if (!query) return notes;

    return notes.filter(note =>
      note.title?.toLowerCase().includes(query) ||
      note.content?.toLowerCase().includes(query) ||
      note.labels?.some(l => l.name.toLowerCase().includes(query))
    );
  });

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.labelName.set(params['labelName']);
      this.loadLabelNotes();
    });
  }

  toggleSidebar(): void {
    this.sidebarExpanded.update(value => !value);
  }

  loadLabelNotes(): void {
    this.isLoading.set(true);
    this.noteService.notes$.subscribe(notes => {
      const labelNotes = notes.filter(n =>
        n.labels?.some(l => l.name.toLowerCase() === this.labelName().toLowerCase()) &&
        !n.isArchived &&
        !n.isDeleted
      );
      this.labelNotes.set(labelNotes);
      this.isLoading.set(false);
    });
    this.noteService.refreshNotes();
  }

  onNoteUpdated(): void {
    this.loadLabelNotes();
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query);
  }

  onViewModeChange(isGrid: boolean): void {
    this.isGridView.set(isGrid);
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
