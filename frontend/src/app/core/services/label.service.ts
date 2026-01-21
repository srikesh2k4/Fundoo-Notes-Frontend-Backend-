import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Observable, BehaviorSubject, tap, map, catchError, throwError, of } from 'rxjs';
import { Label, CreateLabelDto, UpdateLabelDto, ApiResponse } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LabelService {
  private apiUrl = `${environment.apiUrl}/api/labels`;
  private http = inject(HttpClient);
  
  private labelsSubject = new BehaviorSubject<Label[]>([]);
  public labels$ = this.labelsSubject.asObservable();
  
  // ✅ ADD: Loading state
  private loadingSubject = new BehaviorSubject<boolean>(false);
  public loading$ = this.loadingSubject.asObservable();
  
  // ✅ ADD: Error state
  private errorSubject = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSubject.asObservable();

  // ✅ IMPROVED: Get all labels with loading state
  getAllLabels(): Observable<Label[]> {
    this.loadingSubject.next(true);
    this.errorSubject.next(null);
    
    return this.http.get<ApiResponse<Label[]>>(this.apiUrl).pipe(
      map(response => {
        // ✅ Handle different response structures
        if (response.success && response.data) {
          return Array.isArray(response.data) ? response.data : [];
        }
        console.warn('Unexpected API response structure:', response);
        return [];
      }),
      tap(labels => {
        this.labelsSubject.next(labels);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to load labels');
      })
    );
  }

  // ✅ NEW: Get label by ID
  getLabelById(id: number): Observable<Label | null> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid label ID'));
    }

    return this.http.get<ApiResponse<Label>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.success && response.data ? response.data : null),
      catchError((error) => this.handleError(error, `Failed to load label ${id}`))
    );
  }

  // ✅ IMPROVED: Create label with validation
  createLabel(dto: CreateLabelDto): Observable<Label> {
    // Client-side validation
    if (!dto || !dto.name || dto.name.trim().length === 0) {
      return throwError(() => new Error('Label name is required'));
    }

    if (dto.name.trim().length > 50) {
      return throwError(() => new Error('Label name cannot exceed 50 characters'));
    }

    // ✅ Check for duplicate locally before API call
    const existingLabel = this.labelsSubject.value.find(
      l => l.name.toLowerCase() === dto.name.trim().toLowerCase()
    );
    
    if (existingLabel) {
      return throwError(() => new Error('A label with this name already exists'));
    }

    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.post<ApiResponse<Label>>(this.apiUrl, dto).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(label => {
        const currentLabels = this.labelsSubject.value;
        // ✅ Sort labels alphabetically after adding
        const updatedLabels = [...currentLabels, label].sort((a, b) => 
          a.name.localeCompare(b.name)
        );
        this.labelsSubject.next(updatedLabels);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to create label');
      })
    );
  }

  // ✅ IMPROVED: Update label with validation
  updateLabel(id: number, dto: UpdateLabelDto): Observable<Label> {
    // Validation
    if (id <= 0) {
      return throwError(() => new Error('Invalid label ID'));
    }

    if (!dto || !dto.name || dto.name.trim().length === 0) {
      return throwError(() => new Error('Label name is required'));
    }

    if (dto.name.trim().length > 50) {
      return throwError(() => new Error('Label name cannot exceed 50 characters'));
    }

    // ✅ Check if label exists locally
    const existingLabel = this.labelsSubject.value.find(l => l.id === id);
    if (!existingLabel) {
      return throwError(() => new Error('Label not found'));
    }

    // ✅ Check if name is unchanged
    if (existingLabel.name.toLowerCase() === dto.name.trim().toLowerCase()) {
      return of(existingLabel); // Return existing label without API call
    }

    // ✅ Check for duplicate name (excluding current label)
    const duplicateLabel = this.labelsSubject.value.find(
      l => l.id !== id && l.name.toLowerCase() === dto.name.trim().toLowerCase()
    );
    
    if (duplicateLabel) {
      return throwError(() => new Error('A label with this name already exists'));
    }

    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.put<ApiResponse<Label>>(`${this.apiUrl}/${id}`, dto).pipe(
      map(response => {
        if (!response.success || !response.data) {
          throw new Error('Invalid response from server');
        }
        return response.data;
      }),
      tap(updatedLabel => {
        const currentLabels = this.labelsSubject.value.map(l =>
          l.id === id ? updatedLabel : l
        );
        // ✅ Sort after update
        const sortedLabels = currentLabels.sort((a, b) => 
          a.name.localeCompare(b.name)
        );
        this.labelsSubject.next(sortedLabels);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to update label');
      })
    );
  }

  // ✅ IMPROVED: Delete label with validation
  deleteLabel(id: number): Observable<void> {
    if (id <= 0) {
      return throwError(() => new Error('Invalid label ID'));
    }

    // ✅ Check if label exists locally
    const existingLabel = this.labelsSubject.value.find(l => l.id === id);
    if (!existingLabel) {
      return throwError(() => new Error('Label not found'));
    }

    this.loadingSubject.next(true);
    this.errorSubject.next(null);

    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
      map(() => void 0),
      tap(() => {
        const currentLabels = this.labelsSubject.value.filter(l => l.id !== id);
        this.labelsSubject.next(currentLabels);
        this.loadingSubject.next(false);
      }),
      catchError((error) => {
        this.loadingSubject.next(false);
        return this.handleError(error, 'Failed to delete label');
      })
    );
  }

  // ✅ IMPROVED: Refresh labels with error handling
  refreshLabels(): void {
    this.getAllLabels().subscribe({
      next: () => {
        // Labels successfully refreshed
      },
      error: (error) => {
        console.error('Failed to refresh labels:', error);
      }
    });
  }

  // Get labels synchronously
  getLabels(): Label[] {
    return this.labelsSubject.value;
  }

  // ✅ NEW: Get label by name
  getLabelByName(name: string): Label | undefined {
    return this.labelsSubject.value.find(
      l => l.name.toLowerCase() === name.toLowerCase()
    );
  }

  // ✅ NEW: Check if label exists by name
  labelExists(name: string, excludeId?: number): boolean {
    return this.labelsSubject.value.some(
      l => l.name.toLowerCase() === name.toLowerCase() && 
           (excludeId === undefined || l.id !== excludeId)
    );
  }

  // ✅ NEW: Get label count
  getLabelCount(): number {
    return this.labelsSubject.value.length;
  }

  // ✅ NEW: Clear all labels (useful for logout)
  clearLabels(): void {
    this.labelsSubject.next([]);
    this.loadingSubject.next(false);
    this.errorSubject.next(null);
  }

  // ✅ NEW: Search labels by name
  searchLabels(query: string): Label[] {
    const searchTerm = query.toLowerCase().trim();
    if (!searchTerm) {
      return this.labelsSubject.value;
    }
    
    return this.labelsSubject.value.filter(l => 
      l.name.toLowerCase().includes(searchTerm)
    );
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
        errorMessage = 'Label not found.';
      } else if (error.status === 409) {
        errorMessage = 'A label with this name already exists.';
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
    console.error('LabelService Error:', {
      context,
      error,
      message: fullMessage
    });
    
    this.errorSubject.next(fullMessage);
    return throwError(() => new Error(fullMessage));
  }
}
