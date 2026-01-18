import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, map, catchError, throwError } from 'rxjs';
import { Label, CreateLabelDto, UpdateLabelDto, ApiResponse } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LabelService {
  private apiUrl = `${environment.apiUrl}/api/labels`;

  private labelsSubject = new BehaviorSubject<Label[]>([]);
  public labels$ = this.labelsSubject.asObservable();

  constructor(private http: HttpClient) {}

  // Get all labels
  getAllLabels(): Observable<Label[]> {
    return this.http.get<ApiResponse<Label[]>>(this.apiUrl)
      .pipe(
        map(response => response.data),
        tap(labels => this.labelsSubject.next(labels)),
        catchError(this.handleError)
      );
  }

  // Get label by ID
  getLabelById(id: number): Observable<Label> {
    return this.http.get<ApiResponse<Label>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(response => response.data),
        catchError(this.handleError)
      );
  }

  // Create label
  createLabel(dto: CreateLabelDto): Observable<Label> {
    return this.http.post<ApiResponse<Label>>(this.apiUrl, dto)
      .pipe(
        map(response => response.data),
        tap(label => {
          const currentLabels = this.labelsSubject.value;
          this.labelsSubject.next([...currentLabels, label]);
        }),
        catchError(this.handleError)
      );
  }

  // Update label
  updateLabel(id: number, dto: UpdateLabelDto): Observable<Label> {
    return this.http.put<ApiResponse<Label>>(`${this.apiUrl}/${id}`, dto)
      .pipe(
        map(response => response.data),
        tap(updatedLabel => {
          const currentLabels = this.labelsSubject.value.map(l =>
            l.id === id ? updatedLabel : l
          );
          this.labelsSubject.next(currentLabels);
        }),
        catchError(this.handleError)
      );
  }

  // Delete label
  deleteLabel(id: number): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
      .pipe(
        map(() => void 0),
        tap(() => {
          const currentLabels = this.labelsSubject.value.filter(l => l.id !== id);
          this.labelsSubject.next(currentLabels);
        }),
        catchError(this.handleError)
      );
  }

  // Refresh labels from backend
  refreshLabels(): void {
    this.getAllLabels().subscribe();
  }

  // Get labels synchronously
  getLabels(): Label[] {
    return this.labelsSubject.value;
  }

  private handleError(error: any) {
    console.error('LabelService Error:', error);
    return throwError(() => error);
  }
}
