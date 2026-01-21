import { Component, OnInit, signal, computed, inject, DestroyRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NoteService } from '../../../../core/services/note.service';
import { LabelService } from '../../../../core/services/label.service';
import { Note, CreateNoteDto } from '../../../../core/models/note.model';
import { Label } from '../../../../core/models/label.model';
import { NoteCardComponent } from '../../../dashboard/components/note-card/note-card';
import { NoteInputComponent } from '../../components/note-input/note-input';
import { NavbarComponent } from '../../../dashboard/components/navbar/navbar';
import { SidebarComponent } from '../../../dashboard/components/sidebar/sidebar';
import { NoteEditDialogComponent } from '../../../dashboard/components/note-edit-dialog/note-edit-dialog';

@Component({
  selector: 'app-label-notes',
  standalone: true,
  imports: [
    CommonModule,
    NoteCardComponent,
    NoteInputComponent,
    NavbarComponent,
    SidebarComponent,
    NoteEditDialogComponent
  ],
  templateUrl: './label-notes.html',
  styleUrls: ['./label-notes.scss']
})
export class LabelNotesComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private noteService = inject(NoteService);
  private labelService = inject(LabelService);
  private destroyRef = inject(DestroyRef);

  // Signals
  labelName = signal<string>('');
  currentLabel = signal<Label | null>(null);
  allNotes = signal<Note[]>([]);
  isLoading = signal(true);
  sidebarExpanded = signal(false);
  isGridView = signal(true);
  searchQuery = signal('');
  showEditDialog = signal(false);
  selectedNote = signal<Note | null>(null);

  // Computed
  labelExists = computed(() => this.currentLabel() !== null);

  filteredNotes = computed(() => {
    let notes = this.allNotes();
    const query = this.searchQuery().toLowerCase().trim();

    if (query) {
      notes = notes.filter(note =>
        note.title?.toLowerCase().includes(query) ||
        note.content?.toLowerCase().includes(query)
      );
    }

    return notes;
  });

  ngOnInit(): void {
    // Subscribe to route params with automatic cleanup
    this.route.params
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(params => {
        this.labelName.set(params['labelName']);
        this.loadLabelAndNotes();
      });
  }

  private loadLabelAndNotes(): void {
    this.isLoading.set(true);

    // Subscribe to labels with automatic cleanup
    this.labelService.labels$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(labels => {
        const label = labels.find(l =>
          l.name.toLowerCase() === this.labelName().toLowerCase()
        ) || null;

        this.currentLabel.set(label);

        if (label) {
          this.loadNotes();
        } else {
          this.isLoading.set(false);
        }
      });
  }

  private loadNotes(): void {
    const currentLabelId = this.currentLabel()?.id;

    if (!currentLabelId) {
      this.isLoading.set(false);
      return;
    }

    // Subscribe to notes with automatic cleanup
    this.noteService.notes$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(notes => {
        // Filter notes that have this label
        const labelNotes = notes.filter(note =>
          !note.isDeleted &&
          !note.isArchived &&
          note.labels?.some(l => l.id === currentLabelId)
        );
        
        this.allNotes.set(labelNotes);
        this.isLoading.set(false);
      });
  }

  // UI Actions
  toggleSidebar(): void {
    this.sidebarExpanded.update(v => !v);
  }

  onViewModeChange(isGrid: boolean): void {
    this.isGridView.set(isGrid);
  }

  onSearchChange(query: string): void {
    this.searchQuery.set(query);
  }

  onNoteCreated(noteData: { title: string; content: string; color: string }): void {
    const label = this.currentLabel();
    
    if (!label) {
      console.error('No label selected');
      return;
    }

    // ✅ FIX: Create proper CreateNoteDto
    const createDto: CreateNoteDto = {
      title: noteData.title || undefined,
      content: noteData.content || undefined,
      color: noteData.color,
      labelIds: [label.id] // ✅ Pass label IDs instead of full objects
    };

    // Call note service to create the note
    this.noteService.createNote(createDto).subscribe({
      next: () => {
        // Notes will be updated automatically via the observable
        console.log('Note created successfully with label');
      },
      error: (error: any) => {
        console.error('Error creating note:', error);
      }
    });
  }

  onNoteUpdated(): void {
    // Notes are updated automatically via observable subscription
  }

  onNoteClick(note: Note): void {
    this.selectedNote.set(note);
    this.showEditDialog.set(true);
  }

  closeEditDialog(): void {
    this.showEditDialog.set(false);
    this.selectedNote.set(null);
    // No need to manually reload - observable handles updates
  }

  getNoteCount(): number {
    return this.filteredNotes().length;
  }

  trackByNoteId(index: number, note: Note): number {
    return note.id;
  }
}
