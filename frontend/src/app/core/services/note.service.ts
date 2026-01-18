import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, map, catchError, throwError } from 'rxjs';
import { Note, CreateNoteDto, UpdateNoteDto, UpdateNoteColorDto, ApiResponse } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class NoteService {
  private apiUrl = `${environment.apiUrl}/api/notes`;

  private notesSubject = new BehaviorSubject<Note[]>([]);
  public notes$ = this.notesSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Get all notes
  getAllNotes(): Observable<Note[]> {
    return this.http.get<ApiResponse<Note[]>>(this.apiUrl)
      .pipe(
        map(response => response.data),
        tap(notes => this.notesSubject.next(notes)),
        catchError(this.handleError)
      );
  }

  // Get note by ID
  getNoteById(id: number): Observable<Note> {
    return this.http.get<ApiResponse<Note>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Create note
  createNote(dto: CreateNoteDto): Observable<Note> {
    return this.http.post<ApiResponse<Note>>(this.apiUrl, dto)
      .pipe(
        map(response => response.data),
        tap(note => {
          const currentNotes = this.notesSubject.value;
          this.notesSubject.next([note, ...currentNotes]);
        }),
        catchError(this.handleError)
      );
  }

  // Update note
  updateNote(id: number, dto: UpdateNoteDto): Observable<Note> {
    return this.http.put<ApiResponse<Note>>(`${this.apiUrl}/${id}`, dto)
      .pipe(
        map(response => response.data),
        tap(updatedNote => {
          const currentNotes = this.notesSubject.value.map(n =>
            n.id === id ? updatedNote : n
          );
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Delete note
  deleteNote(id: number): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(() => void 0),
        tap(() => {
          const currentNotes = this.notesSubject.value.filter(n => n.id !== id);
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Search notes
  searchNotes(query: string): Observable<Note[]> {
    return this.http.get<ApiResponse<Note[]>>(`${this.apiUrl}/search?query=${encodeURIComponent(query)}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Toggle pin
  togglePin(id: number): Observable<Note> {
    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/pin`, {})
      .pipe(
        map(response => response.data),
        tap(updatedNote => {
          const currentNotes = this.notesSubject.value.map(n =>
            n.id === id ? updatedNote : n
          );
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Toggle archive
  toggleArchive(id: number): Observable<Note> {
    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/archive`, {})
      .pipe(
        map(response => response.data),
        tap(updatedNote => {
          const currentNotes = this.notesSubject.value.map(n =>
            n.id === id ? updatedNote : n
          );
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Update color
  updateColor(id: number, dto: UpdateNoteColorDto): Observable<Note> {
    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/color`, dto)
      .pipe(
        map(response => response.data),
        tap(updatedNote => {
          const currentNotes = this.notesSubject.value.map(n =>
            n.id === id ? updatedNote : n
          );
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Bulk delete
  bulkDelete(noteIds: number[]): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/bulk-delete`, { noteIds })
      .pipe(
        map(() => void 0),
        tap(() => {
          const currentNotes = this.notesSubject.value.filter(n => !noteIds.includes(n.id));
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Toggle trash
  toggleTrash(id: number): Observable<Note> {
    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/trash`, {})
      .pipe(
        map(response => response.data),
        tap(updatedNote => {
          const currentNotes = this.notesSubject.value.map(n =>
            n.id === id ? updatedNote : n
          );
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Get trashed notes
  getTrashedNotes(): Observable<Note[]> {
    return this.http.get<ApiResponse<Note[]>>(`${this.apiUrl}/trash`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Restore from trash
  restoreFromTrash(id: number): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/${id}/restore`, {})
      .pipe(
        map(() => void 0),
        tap(() => {
          const currentNotes = this.notesSubject.value.filter(n => n.id !== id);
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Delete permanently
  deletePermanently(id: number): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}/permanent`)
      .pipe(
        map(() => void 0),
        tap(() => {
          const currentNotes = this.notesSubject.value.filter(n => n.id !== id);
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Empty trash
  emptyTrash(): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/trash/empty`)
      .pipe(
        map(() => void 0),
        tap(() => {
          // Clear all deleted notes from local state
          const currentNotes = this.notesSubject.value.filter(n => !n.isDeleted);
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Add label to note
  addLabelToNote(noteId: number, labelId: number): Observable<Note> {
    return this.http.post<ApiResponse<Note>>(`${this.apiUrl}/${noteId}/labels/${labelId}`, {})
      .pipe(
        map(response => response.data),
        tap(updatedNote => {
          const currentNotes = this.notesSubject.value.map(n =>
            n.id === noteId ? updatedNote : n
          );
          this.notesSubject.next(currentNotes);
        }),
        catchError(this.handleError)
      );
  }

  // Remove label from note
  removeLabelFromNote(noteId: number, labelId: number): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${noteId}/labels/${labelId}`)
      .pipe(
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
        catchError(this.handleError)
      );
  }

  // Copy note
  copyNote(note: Note): Observable<Note> {
    const copyDto = {
      title: note.title ? `${note.title} (copy)` : undefined,
      content: note.content || undefined,
      color: note.color || '#ffffff'
    };
    return this.createNote(copyDto);
  }

  // Get pinned notes
  getPinnedNotes(): Note[] {
    return this.notesSubject.value.filter(n => n.isPinned && !n.isArchived);
  }

  // Get other notes (not pinned, not archived)
  getOtherNotes(): Note[] {
    return this.notesSubject.value.filter(n => !n.isPinned && !n.isArchived);
  }

  // Get archived notes
  getArchivedNotes(): Note[] {
    return this.notesSubject.value.filter(n => n.isArchived);
  }

  // Refresh notes from backend
  refreshNotes(): void {
    this.getAllNotes().subscribe();
  }

  private handleError(error: any) {
    console.error('NoteService Error:', error);
    return throwError(() => error);
  }
}
