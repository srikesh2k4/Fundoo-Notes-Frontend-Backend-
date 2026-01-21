// sidebar.ts
import { Component, Input, OnInit, OnDestroy, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LabelService } from '../../../../core/services/label.service';
import { Label } from '../../../../core/models/label.model';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive, FormsModule],
  templateUrl: './sidebar.html',
  styleUrls: ['./sidebar.scss']
})
export class SidebarComponent implements OnInit, OnDestroy {
  @Input() isExpanded = false;
  @Input() activeItem = 'notes';

  private labelService = inject(LabelService);
  private subscriptions = new Subscription();

  labels = signal<Label[]>([]);
  showEditLabelsDialog = signal(false);
  newLabelName = '';
  editingLabelId = signal<number | null>(null);
  editingLabelName = '';
  isCreatingLabel = signal(false);

  ngOnInit(): void {
    this.loadLabels();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadLabels(): void {
    const sub = this.labelService.labels$.subscribe(labels => {
      this.labels.set(labels);
    });
    this.subscriptions.add(sub);
    
    // Initial fetch
    this.labelService.refreshLabels();
  }

  createLabel(): void {
    if (!this.newLabelName.trim()) return;

    this.isCreatingLabel.set(true);
    const sub = this.labelService.createLabel({ name: this.newLabelName.trim() })
      .subscribe({
        next: () => {
          this.newLabelName = '';
          this.isCreatingLabel.set(false);
        },
        error: (err) => {
          console.error('Failed to create label', err);
          this.isCreatingLabel.set(false);
        }
      });
    this.subscriptions.add(sub);
  }

  saveEditLabel(label: Label): void {
    if (!this.editingLabelName.trim()) {
      this.cancelEditLabel();
      return;
    }

    const sub = this.labelService.updateLabel(label.id, { name: this.editingLabelName.trim() })
      .subscribe({
        next: () => {
          this.cancelEditLabel();
        },
        error: (err) => {
          console.error('Failed to update label', err);
        }
      });
    this.subscriptions.add(sub);
  }

  deleteLabel(label: Label): void {
    if (!confirm(`Delete label "${label.name}"?`)) return;

    const sub = this.labelService.deleteLabel(label.id).subscribe({
      error: (err) => {
        console.error('Failed to delete label', err);
      }
    });
    this.subscriptions.add(sub);
  }

  startEditLabel(label: Label): void {
    this.editingLabelId.set(label.id);
    this.editingLabelName = label.name;
  }

  cancelEditLabel(): void {
    this.editingLabelId.set(null);
    this.editingLabelName = '';
  }

  openEditLabels(): void {
    this.showEditLabelsDialog.set(true);
    this.newLabelName = '';
    this.editingLabelId.set(null);
  }

  closeEditLabels(): void {
    this.showEditLabelsDialog.set(false);
    this.newLabelName = '';
    this.editingLabelId.set(null);
  }

  onNewLabelKeydown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && this.newLabelName.trim()) {
      this.createLabel();
    }
  }

  onEditLabelKeydown(event: KeyboardEvent, label: Label): void {
    if (event.key === 'Enter') {
      this.saveEditLabel(label);
    } else if (event.key === 'Escape') {
      this.cancelEditLabel();
    }
  }
}
