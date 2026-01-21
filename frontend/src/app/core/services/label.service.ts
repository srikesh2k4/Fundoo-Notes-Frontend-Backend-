import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Label, CreateLabelDto, UpdateLabelDto, ApiResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class LabelService {
  private readonly apiUrl = `${environment.apiUrl}/api/labels`;
  
  // BehaviorSubject to share labels across components
  private labelsSubject = new BehaviorSubject<Label[]>([]);
  public labels$ = this.labelsSubject.asObservable();

  constructor(private http: HttpClient) {
    this.loadLabels();
  }

  /**
   * Load all labels from backend
   */
  loadLabels(): void {
    this.http.get<ApiResponse<Label[]>>(`${this.apiUrl}`)
      .pipe(
        tap(response => {
          const labels = response.data || [];
          this.labelsSubject.next(labels);
        })
      )
      .subscribe();
  }

  /**
   * Get all labels
   */
  getLabels(): Observable<ApiResponse<Label[]>> {
    return this.http.get<ApiResponse<Label[]>>(`${this.apiUrl}`)
      .pipe(
        tap(response => {
          const labels = response.data || [];
          this.labelsSubject.next(labels);
        })
      );
  }

  /**
   * Get label by ID
   */
  getLabelById(id: number): Observable<ApiResponse<Label>> {
    return this.http.get<ApiResponse<Label>>(`${this.apiUrl}/${id}`);
  }

  /**
   * Create a new label
   */
  createLabel(dto: CreateLabelDto): Observable<ApiResponse<Label>> {
    return this.http.post<ApiResponse<Label>>(`${this.apiUrl}`, dto)
      .pipe(
        tap(() => this.loadLabels())
      );
  }

  /**
   * Update a label
   */
  updateLabel(id: number, dto: UpdateLabelDto): Observable<ApiResponse<Label>> {
    return this.http.put<ApiResponse<Label>>(`${this.apiUrl}/${id}`, dto)
      .pipe(
        tap(() => this.loadLabels())
      );
  }

  /**
   * Delete a label
   */
  deleteLabel(id: number): Observable<ApiResponse<void>> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
      .pipe(
        tap(() => this.loadLabels())
      );
  }

  /**
   * Get current labels value (synchronous)
   */
  getCurrentLabels(): Label[] {
    return this.labelsSubject.value;
  }
}
