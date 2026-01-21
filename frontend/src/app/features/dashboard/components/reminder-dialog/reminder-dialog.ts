import { Component, Output, EventEmitter, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

interface ReminderOption {
  label: string;
  value: string;
}

@Component({
  selector: 'app-reminder-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reminder-dialog.html',
  styleUrls: ['./reminder-dialog.scss']
})
export class ReminderDialogComponent {
  @Output() close = new EventEmitter<void>();
  
  selectedReminder = signal<string | null>(null);

  reminderOptions: ReminderOption[] = [
    { label: 'Later today', value: 'later_today' },
    { label: 'Tomorrow', value: 'tomorrow' },
    { label: 'Next week', value: 'next_week' },
    { label: 'Pick date & time', value: 'custom' }
  ];

  selectReminder(option: ReminderOption): void {
    this.selectedReminder.set(option.value);
    
    if (option.value === 'custom') {
      // TODO: Open date/time picker
      console.log('Open custom date/time picker');
    } else {
      // TODO: Call backend API to set reminder
      console.log('Set reminder:', option.label);
      this.close.emit();
    }
  }

  onOverlayClick(event: Event): void {
    if ((event.target as HTMLElement).classList.contains('dialog-overlay')) {
      this.close.emit();
    }
  }
}
