import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  Note, 
  CreateNoteDto, 
  UpdateNoteDto, 
  UpdateNoteColorDto,
  ApiResponse 
} from '../models';

@Injectable({
  providedIn: 'root'
})
export class NoteService {
  private readonly apiUrl = `${environment.apiUrl}/api/notes`;
  
  private notesSubject = new BehaviorSubject<Note[]>([]);
  public notes$ = this.notesSubject.asObservable();
  
  private notesSignal = signal<Note[]>([]);

  constructor(private http: HttpClient) {
    this.loadNotes();
  }

  loadNotes(): void {
    this.http.get<ApiResponse<Note[]>>(`${this.apiUrl}`)
      .pipe(
        tap(response => {
          const notes = response.data || [];
          this.notesSubject.next(notes);
          this.notesSignal.set(notes);
        }),
        catchError((error: any) => {
          console.error('Error loading notes:', error);
          return throwError(() => error);
        })
      )
      .subscribe();
  }

  getNotes(): Observable<ApiResponse<Note[]>> {
    return this.http.get<ApiResponse<Note[]>>(`${this.apiUrl}`)
      .pipe(
        tap(response => {
          const notes = response.data || [];
          this.notesSubject.next(notes);
          this.notesSignal.set(notes);
        })
      );
  }

  getNoteById(id: number): Observable<ApiResponse<Note>> {
    return this.http.get<ApiResponse<Note>>(`${this.apiUrl}/${id}`);
  }

  createNote(dto: CreateNoteDto): Observable<ApiResponse<Note>> {
    return this.http.post<ApiResponse<Note>>(`${this.apiUrl}`, dto)
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  updateNote(id: number, dto: UpdateNoteDto): Observable<ApiResponse<Note>> {
    return this.http.put<ApiResponse<Note>>(`${this.apiUrl}/${id}`, dto)
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  updateNoteColor(id: number, dto: UpdateNoteColorDto): Observable<ApiResponse<Note>> {
    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/color`, dto)
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  togglePin(id: number): Observable<ApiResponse<Note>> {
    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/pin`, {})
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  toggleArchive(id: number): Observable<ApiResponse<Note>> {
    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/archive`, {})
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  moveToTrash(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  permanentlyDelete(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}/permanent`)
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  restoreNote(id: number): Observable<ApiResponse<Note>> {
    return this.http.patch<ApiResponse<Note>>(`${this.apiUrl}/${id}/restore`, {})
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  addLabel(noteId: number, labelId: number): Observable<ApiResponse<Note>> {
    return this.http.post<ApiResponse<Note>>(
      `${this.apiUrl}/${noteId}/labels/${labelId}`, 
      {}
    ).pipe(
      tap(() => this.loadNotes())
    );
  }

  removeLabel(noteId: number, labelId: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(
      `${this.apiUrl}/${noteId}/labels/${labelId}`
    ).pipe(
      tap(() => this.loadNotes())
    );
  }

  searchNotes(query: string): Observable<ApiResponse<Note[]>> {
    return this.http.get<ApiResponse<Note[]>>(`${this.apiUrl}/search`, {
      params: { query }
    });
  }

  getTrashedNotes(): Observable<ApiResponse<Note[]>> {
    return this.http.get<ApiResponse<Note[]>>(`${this.apiUrl}/trash`);
  }

  emptyTrash(): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/trash/empty`)
      .pipe(
        tap(() => this.loadNotes())
      );
  }

  getCurrentNotes(): Note[] {
    return this.notesSubject.value;
  }
}
