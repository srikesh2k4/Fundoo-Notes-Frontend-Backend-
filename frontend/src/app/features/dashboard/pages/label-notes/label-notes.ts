import { Component, OnInit, OnDestroy, signal, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { NavbarComponent } from '../../components/navbar/navbar';
import { SidebarComponent } from '../../components/sidebar/sidebar';
import { NoteCardComponent } from '../../components/note-card/note-card';
import { NoteEditDialogComponent } from '../../components/note-edit-dialog/note-edit-dialog';
import { NoteService } from '../../../../core/services/note.service';
import { LabelService } from '../../../../core/services/label.service';
import { Note } from '../../../../core/models/note.model';

@Component({
  selector: 'app-label-notes',
  standalone: true,
  imports: [
    CommonModule,
    NavbarComponent,
    SidebarComponent,
    NoteCardComponent,
    NoteEditDialogComponent
  ],
  templateUrl: './label-notes.html',
  styleUrls: ['./label-notes.scss']
})
export class LabelNotesComponent implements OnInit, OnDestroy {
  private noteService = inject(NoteService);
  private labelService = inject(LabelService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  
  private notesSubscription?: Subscription;
  private routeSubscription?: Subscription;
  private labelsSubscription?: Subscription;

  // State signals
  sidebarExpanded = signal(false);
  labelName = signal('');
  labelId = signal<number | null>(null);
  labelNotes = signal<Note[]>([]);
  isLoading = signal(true);
  labelExists = signal(true);

  selectedNote = signal<Note | null>(null);
  showEditDialog = signal(false);

  isGridView = signal(true);
  searchQuery = signal('');

  // Computed filtered notes
  filteredNotes = computed(() => {
    const query = this.searchQuery().toLowerCase().trim();
    const notes = this.labelNotes();

    if (!query) return notes;

    return notes.filter(note => {
      const titleMatch = note.title?.toLowerCase().includes(query) || false;
      const contentMatch = note.content?.toLowerCase().includes(query) || false;
      const labelMatch = note.labels?.some(l => l.name.toLowerCase().includes(query)) || false;

      return titleMatch || contentMatch || labelMatch;
    });
  });

  ngOnInit(): void {
    // Subscribe to route params to handle label changes
    this.routeSubscription = this.route.params.subscribe(params => {
      const rawLabelName = params['labelName'];
      
      // Decode URI component to handle special characters
      const decodedLabelName = decodeURIComponent(rawLabelName || '');
      
      this.labelName.set(decodedLabelName);
      this.validateLabelExists(decodedLabelName);
      this.setupNotesSubscription();
      this.noteService.refreshNotes();
    });

    // Load labels to validate existence
    this.labelsSubscription = this.labelService.labels$.subscribe(labels => {
      if (labels.length > 0 && this.labelName()) {
        this.validateLabelExists(this.labelName());
      }
    });

    // Initial label fetch
    this.labelService.refreshLabels();
  }

  ngOnDestroy(): void {
    this.notesSubscription?.unsubscribe();
    this.routeSubscription?.unsubscribe();
    this.labelsSubscription?.unsubscribe();
  }

  // Validate if label exists
  private validateLabelExists(labelName: string): void {
    if (!labelName) {
      this.labelExists.set(false);
      return;
    }

    const labels = this.labelService.getLabels();
    const label = labels.find(l => 
      l.name.toLowerCase() === labelName.toLowerCase()
    );

    if (label) {
      this.labelId.set(label.id);
      this.labelExists.set(true);
    } else {
      this.labelExists.set(false);
      // If label doesn't exist and labels are loaded, redirect to home
      if (labels.length > 0) {
        console.warn(`Label "${labelName}" not found, redirecting to home`);
        setTimeout(() => {
          this.router.navigate(['/']);
        }, 2000); // Give user time to see the error message
      }
    }
  }

  // Setup notes subscription with filtering
  private setupNotesSubscription(): void {
    this.isLoading.set(true);
    this.notesSubscription?.unsubscribe();
    
    this.notesSubscription = this.noteService.notes$.subscribe({
      next: (notes) => {
        const currentLabelName = this.labelName().toLowerCase();
        
        // Filter notes that have the current label and are not archived/deleted
        const labelNotes = notes.filter(n => {
          const hasLabel = n.labels?.some(l => 
            l.name.toLowerCase() === currentLabelName
          ) || false;
          
          const isActive = !n.isArchived && !n.isDeleted;
          
          return hasLabel && isActive;
        });

        this.labelNotes.set(labelNotes);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error loading label notes:', err);
        this.isLoading.set(false);
        this.labelNotes.set([]);
      }
    });
  }

  // Toggle sidebar
  toggleSidebar(): void {
    this.sidebarExpanded.update(value => !value);
  }

  // Handle note update
  onNoteUpdated(): void {
    this.noteService.refreshNotes();
    this.labelService.refreshLabels();
  }

  // Handle search change
  onSearchChange(query: string): void {
    this.searchQuery.set(query);
  }

  // Handle view mode change
  onViewModeChange(isGrid: boolean): void {
    this.isGridView.set(isGrid);
  }

  // Handle note click
  onNoteClick(note: Note): void {
    this.selectedNote.set(note);
    this.showEditDialog.set(true);
  }

  // Close edit dialog
  closeEditDialog(): void {
    this.showEditDialog.set(false);
    this.selectedNote.set(null);
    this.noteService.refreshNotes();
    this.labelService.refreshLabels();
  }

  // Get note count
  getNoteCount(): number {
    return this.filteredNotes().length;
  }

  // TrackBy function for performance
  trackByNoteId(index: number, note: Note): number {
    return note.id;
  }

  // Handle label deletion
  onLabelDeleted(): void {
    this.router.navigate(['/']);
  }
}
