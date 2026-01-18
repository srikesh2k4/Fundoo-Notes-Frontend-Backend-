import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Loading } from './features/dashboard/pages/loading/loading';

@Component({
  selector: 'app-root',
  // imports: [RouterOutlet,Loading],
  imports: [RouterOutlet, Loading],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('frontend');
}
