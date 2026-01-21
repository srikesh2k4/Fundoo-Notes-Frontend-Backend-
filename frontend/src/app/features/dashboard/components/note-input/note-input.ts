import { Component, signal, Output, EventEmitter, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-note-input',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './note-input.html',
  styleUrl: './note-input.scss'
})
export class NoteInputComponent {
  // Add this Input
  @Input() defaultLabelId?: number;

  // Signals for reactive state
  isExpanded = signal(false);
  title = signal('');
  content = signal('');
  selectedColor = signal('#ffffff');

  // Output event to emit new notes
  @Output() noteCreated = new EventEmitter<{
    title: string;
    content: string;
    color: string;
  }>();

  // Available colors for notes
  colors = [
    { name: 'Default', value: '#ffffff' },
    { name: 'Red', value: '#f28b82' },
    { name: 'Orange', value: '#fbbc04' },
    { name: 'Yellow', value: '#fff475' },
    { name: 'Green', value: '#ccff90' },
    { name: 'Teal', value: '#a7ffeb' },
    { name: 'Blue', value: '#cbf0f8' },
    { name: 'Dark Blue', value: '#aecbfa' },
    { name: 'Purple', value: '#d7aefb' },
    { name: 'Pink', value: '#fdcfe8' },
    { name: 'Brown', value: '#e6c9a8' },
    { name: 'Gray', value: '#e8eaed' }
  ];

  showColorPicker = signal(false);

  /**
   * Expands the input box to show full form
   */
  expand(): void {
    this.isExpanded.set(true);
  }

  /**
   * Toggles color picker visibility
   */
  toggleColorPicker(event: Event): void {
    event.stopPropagation();
    this.showColorPicker.update(value => !value);
  }

  /**
   * Selects a color for the note
   */
  selectColor(color: string, event: Event): void {
    event.stopPropagation();
    this.selectedColor.set(color);
    this.showColorPicker.set(false);
  }

  /**
   * Closes the expanded form and creates note if content exists
   */
  close(): void {
    if (this.title().trim() || this.content().trim()) {
      this.createNote();
    }
    this.reset();
  }

  /**
   * Creates and emits a new note
   */
  createNote(): void {
    const noteData = {
      title: this.title().trim(),
      content: this.content().trim(),
      color: this.selectedColor()
    };

    if (noteData.title || noteData.content) {
      this.noteCreated.emit(noteData);
      this.reset();
    }
  }

  /**
   * Resets the form to initial state
   */
  reset(): void {
    this.isExpanded.set(false);
    this.title.set('');
    this.content.set('');
    this.selectedColor.set('#ffffff');
    this.showColorPicker.set(false);
  }

  /**
   * Handles clicks outside the component
   */
  onClickOutside(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.note-input-container')) {
      this.close();
    }
  }

  /**
   * Closes menus when clicking inside the container
   */
  closeMenus(): void {
    this.showColorPicker.set(false);
  }
}
