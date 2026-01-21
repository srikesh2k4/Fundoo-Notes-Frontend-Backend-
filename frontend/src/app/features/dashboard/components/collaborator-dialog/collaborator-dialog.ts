import { Component, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-collaborator-dialog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './collaborator-dialog.html',
  styleUrls: ['./collaborator-dialog.scss']
})
export class CollaboratorDialogComponent {
  @Output() close = new EventEmitter<void>();
  
  collaboratorEmail = '';
  collaborators = signal<string[]>([]);

  addCollaborator(): void {
    const email = this.collaboratorEmail.trim();
    if (email && this.isValidEmail(email)) {
      // TODO: Call backend API to add collaborator
      this.collaborators.update(list => [...list, email]);
      this.collaboratorEmail = '';
      console.log('Add collaborator:', email);
    }
  }

  removeCollaborator(email: string): void {
    // TODO: Call backend API to remove collaborator
    this.collaborators.update(list => list.filter(e => e !== email));
    console.log('Remove collaborator:', email);
  }

  isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  onOverlayClick(event: Event): void {
    if ((event.target as HTMLElement).classList.contains('dialog-overlay')) {
      this.close.emit();
    }
  }

  saveAndClose(): void {
    // TODO: Save collaborators to backend
    this.close.emit();
  }
}
