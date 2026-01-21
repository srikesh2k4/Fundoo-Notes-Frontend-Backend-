import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, map, catchError, throwError, of } from 'rxjs';
import { Note, CreateNoteDto, UpdateNoteDto, UpdateNoteColorDto, ApiResponse } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NoteService {
  private apiUrl = `${environment.apiUrl}/api/notes`;
  private http = inject(HttpClient); // ✅ FIX: Fixed typo and use inject

  private notesSubject = new BehaviorSubject<Note[]>([]);
  public notes$ = this.notesSubject.asObservable();

  // ✅ ADD: Loading state
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();

  // ✅ ADD: Error state
  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  // ✅ IMPROVED: Get all notes with loading state
  getAllNotes(): Observable<Note[]> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.get<ApiResponse<Note[]>>(this.apiUrl).pipe(
      map(response => {
        if (response.success && response.data) {
          return Array.isArray(response.data) ? response.data : [];
        }
        console.warn('Unexpected API response structure:', response);
        return [];
      }),
      tap(notes => {
        this.notesSubject.next(notes);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to load notes');
      })
    );
  }

  // ✅ IMPROVED: Get note by ID with validation
  getNoteById(id: number): Observable<Note | null> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    return this.http.get<ApiResponse<Note>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.success && response.data ? response.data : null),
      catchError((error) => this.handleError(error, `Failed to load note ${id}`))
    );
  }

  // ✅ IMPROVED: Create note with validation
  createNote(dto: CreateNoteDto): Observable<Note> {
    // Client-side validation
    if (!dto) {
      return throwError(() => new Error('Note data is required'));
    }

    if (!dto.title?.trim() && !dto.content?.trim()) {
      return throwError(() => new Error('Either title or content is required'));
    }

    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.post<ApiResponse<Note>>(this.apiUrl, dto).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(note => {
        const currentNotes = this.notesSubject.value;
        // ✅ Add to beginning (newest first)
        this.notesSubject.next([note, ...currentNotes]);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to create note');
      })
    );
  }

  // ✅ IMPROVED: Update note with validation
  updateNote(id: number, dto: UpdateNoteDto): Observable<Note> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    if (!dto) {
      return throwError(() => new Error('Note data is required'));
    }

    // ✅ Check if note exists locally
    const existingNote = this.notesSubject.value.find(n => n.id === id);
    if (!existingNote) {
      return throwError(() => new Error('Note not found'));
    }

    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.put<ApiResponse<Note>>(`${this.apiUrl}/${id}`, dto).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(updatedNote => {
        const currentNotes = this.notesSubject.value.map(n =>
          n.id === id ? updatedNote : n
        );
        this.notesSubject.next(currentNotes);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to update note');
      })
    );
  }

  // ✅ IMPROVED: Delete note (soft delete)
  deleteNote(id: number): Observable<void> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
      map(() => void 0),
      tap(() => {
        // ✅ Mark as deleted instead of removing from array
        const currentNotes = this.notesSubject.value.map(n =>
          n.id === id ? { ...n, isDeleted: true, deletedAt: new Date() } : n
        );
        this.notesSubject.next(currentNotes);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to delete note');
      })
    );
  }

  // Search notes
  searchNotes(query: string): Observable<Note[]> {
    if (!query || query.trim().length === 0) {
      return of([]);
    }

    return this.http.get<ApiResponse<Note[]>>(`${this.apiUrl}/search?query=${encodeURIComponent(query)}`).pipe(
      map(response => response.success && response.data ? response.data : []),
      catchError((error) => this.handleError(error, 'Failed to search notes'))
    );
  }

  // ✅ IMPROVED: Toggle pin with validation
  togglePin(id: number): Observable<Note> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/pin`, {}).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(updatedNote => {
        const currentNotes = this.notesSubject.value.map(n =>
          n.id === id ? updatedNote : n
        );
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to toggle pin'))
    );
  }

  // ✅ IMPROVED: Toggle archive with validation
  toggleArchive(id: number): Observable<Note> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/archive`, {}).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(updatedNote => {
        const currentNotes = this.notesSubject.value.map(n =>
          n.id === id ? updatedNote : n
        );
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to toggle archive'))
    );
  }

  // ✅ IMPROVED: Update color with validation
  updateColor(id: number, dto: UpdateNoteColorDto): Observable<Note> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    if (!dto || !dto.color) {
      return throwError(() => new Error('Color is required'));
    }

    // ✅ Validate hex color format
    const hexColorRegex = /^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$/;
    if (!hexColorRegex.test(dto.color)) {
      return throwError(() => new Error('Invalid color format. Use hex format (e.g., #ffffff)'));
    }

    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/color`, dto).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(updatedNote => {
        const currentNotes = this.notesSubject.value.map(n =>
          n.id === id ? updatedNote : n
        );
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to update color'))
    );
  }

  // ✅ IMPROVED: Bulk delete with validation
  bulkDelete(noteIds: number[]): Observable<void> {
    if (!noteIds || noteIds.length === 0) {
      return throwError(() => new Error('No notes selected'));
    }

    if (noteIds.some(id => id <= 0)) {
      return throwError(() => new Error('Invalid note IDs'));
    }

    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/bulk-delete`, { noteIds }).pipe(
      map(() => void 0),
      tap(() => {
        // ✅ Mark as deleted instead of removing
        const currentNotes = this.notesSubject.value.map(n =>
          noteIds.includes(n.id) ? { ...n, isDeleted: true, deletedAt: new Date() } : n
        );
        this.notesSubject.next(currentNotes);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to delete notes');
      })
    );
  }

  // Toggle trash
  toggleTrash(id: number): Observable<Note> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/trash`, {}).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(updatedNote => {
        const currentNotes = this.notesSubject.value.map(n =>
          n.id === id ? updatedNote : n
        );
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to toggle trash'))
    );
  }

  // Get trashed notes
  getTrashedNotes(): Observable<Note[]> {
    return this.http.get<ApiResponse<Note[]>>(`${this.apiUrl}/trash`).pipe(
      map(response => response.success && response.data ? response.data : []),
      catchError((error) => this.handleError(error, 'Failed to load trashed notes'))
    );
  }

  // Restore from trash
  restoreFromTrash(id: number): Observable<void> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/restore`, {}).pipe(
      map(() => void 0),
      tap(() => {
        // ✅ Mark as not deleted instead of removing
        const currentNotes = this.notesSubject.value.map(n =>
          n.id === id ? { ...n, isDeleted: false, deletedAt: undefined } : n
        );
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to restore note'))
    );
  }

  // Delete permanently
  deletePermanently(id: number): Observable<void> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid note ID'));
    }

    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}/permanent`).pipe(
      map(() => void 0),
      tap(() => {
        // ✅ Now actually remove from array
        const currentNotes = this.notesSubject.value.filter(n => n.id !== id);
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to permanently delete note'))
    );
  }

  // Empty trash
  emptyTrash(): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/trash/empty`).pipe(
      map(() => void 0),
      tap(() => {
        // Clear all deleted notes from local state
        const currentNotes = this.notesSubject.value.filter(n => !n.isDeleted);
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to empty trash'))
    );
  }

  // ✅ IMPROVED: Add label to note with validation
  addLabelToNote(noteId: number, labelId: number): Observable<Note> {
    if (noteId <= 0 || labelId <= 0) {
      return throwError(() => new Error('Invalid note or label ID'));
    }

    return this.http.post<ApiResponse<Note>>(`${this.apiUrl}/${noteId}/labels/${labelId}`, {}).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(updatedNote => {
        const currentNotes = this.notesSubject.value.map(n =>
          n.id === noteId ? updatedNote : n
        );
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to add label to note'))
    );
  }

  // ✅ IMPROVED: Remove label from note with validation
  removeLabelFromNote(noteId: number, labelId: number): Observable<void> {
    if (noteId <= 0 || labelId <= 0) {
      return throwError(() => new Error('Invalid note or label ID'));
    }

    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${noteId}/labels/${labelId}`).pipe(
      map(() => void 0),
      tap(() => {
        const currentNotes = this.notesSubject.value.map(n => {
          if (n.id === noteId && n.labels) {
            return { ...n, labels: n.labels.filter(l => l.id !== labelId) };
          }
          return n;
        });
        this.notesSubject.next(currentNotes);
      }),
      catchError((error) => this.handleError(error, 'Failed to remove label from note'))
    );
  }

  // ✅ IMPROVED: Copy note
  copyNote(note: Note): Observable<Note> {
    if (!note) {
      return throwError(() => new Error('Note is required'));
    }

    const copyDto: CreateNoteDto = {
      title: note.title ? `${note.title} (copy)` : undefined,
      content: note.content || undefined,
      color: note.color || '#ffffff'
    };
    return this.createNote(copyDto);
  }

  // ✅ IMPROVED: Get pinned notes (exclude deleted)
  getPinnedNotes(): Note[] {
    return this.notesSubject.value.filter(n => 
      n.isPinned && !n.isArchived && !n.isDeleted
    );
  }

  // ✅ IMPROVED: Get other notes (exclude deleted)
  getOtherNotes(): Note[] {
    return this.notesSubject.value.filter(n => 
      !n.isPinned && !n.isArchived && !n.isDeleted
    );
  }

  // ✅ IMPROVED: Get archived notes (exclude deleted)
  getArchivedNotes(): Note[] {
    return this.notesSubject.value.filter(n => 
      n.isArchived && !n.isDeleted
    );
  }

  // ✅ NEW: Get deleted notes
  getDeletedNotes(): Note[] {
    return this.notesSubject.value.filter(n => n.isDeleted === true);
  }

  // ✅ NEW: Get notes by label
  getNotesByLabel(labelName: string): Note[] {
    return this.notesSubject.value.filter(n =>
      !n.isDeleted &&
      n.labels?.some(l => l.name.toLowerCase() === labelName.toLowerCase())
    );
  }

  // ✅ NEW: Get note count
  getNoteCount(): number {
    return this.notesSubject.value.filter(n => !n.isDeleted).length;
  }

  // ✅ NEW: Clear all notes (useful for logout)
  clearNotes(): void {
    this.notesSubject.next([]);
    this.loadingSubject.next(false);
    this.errorSubject.next(null);
  }

  // ✅ IMPROVED: Refresh notes with error handling
  refreshNotes(): void {
    this.getAllNotes().subscribe({
      next: () => {
        // Notes successfully refreshed
      },
      error: (error) => {
        console.error('Failed to refresh notes:', error);
      }
    });
  }

  // ✅ IMPROVED: Better error handling
  private handleError(error: HttpErrorResponse | Error, context: string): Observable<never> {
    let errorMessage = 'An unexpected error occurred';

    if (error instanceof HttpErrorResponse) {
      // Server-side error
      if (error.error?.message) {
        errorMessage = error.error.message;
      } else if (error.status === 0) {
        errorMessage = 'Unable to connect to server. Please check your internet connection.';
      } else if (error.status === 401) {
        errorMessage = 'Unauthorized. Please log in again.';
      } else if (error.status === 403) {
        errorMessage = 'Access denied.';
      } else if (error.status === 404) {
        errorMessage = 'Note not found.';
      } else if (error.status === 409) {
        errorMessage = 'Conflict: The note may have been modified by another process.';
      } else if (error.status >= 500) {
        errorMessage = 'Server error. Please try again later.';
      } else {
        errorMessage = `Error: ${error.statusText || 'Unknown error'}`;
      }
    } else if (error instanceof Error) {
      // Client-side error
      errorMessage = error.message;
    }

    const fullMessage = `${context}: ${errorMessage}`;
    console.error('NoteService Error:', {
      context,
      error,
      message: fullMessage
    });

    this.errorSubject.next(fullMessage);
    return throwError(() => new Error(fullMessage));
  }
}
