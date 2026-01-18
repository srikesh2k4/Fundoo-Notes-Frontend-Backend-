import { Component, signal, Output, EventEmitter, inject, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth.service';
import { NoteService } from '../../../../core/services/note.service';
import { ThemeService } from '../../../../core/services/theme.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './navbar.html',
  styleUrls: ['./navbar.scss'],
})
export class NavbarComponent implements OnInit {
  private authService = inject(AuthService);
  private noteService = inject(NoteService);
  private router = inject(Router);
  themeService = inject(ThemeService);

  searchQuery = signal('');
  isSearchActive = signal(false);
  showProfileMenu = signal(false);
  showSettingsMenu = signal(false);

  @Input() isGridView = true;
  @Output() menuToggle = new EventEmitter<void>();
  @Output() searchChange = new EventEmitter<string>();
  @Output() viewModeChange = new EventEmitter<boolean>();
  @Output() refresh = new EventEmitter<void>();

  userProfile = signal({
    name: 'User',
    email: '',
    avatar: 'https://lh3.googleusercontent.com/a/default-user=s48-p'
  });

  ngOnInit(): void {
    this.loadUserProfile();
  }

  loadUserProfile(): void {
    const user = this.authService.getUser();
    if (user) {
      this.userProfile.set({
        name: user.name || 'User',
        email: user.email || '',
        avatar: this.getInitialsAvatar(user.name || user.email)
      });
    }
  }

  getInitialsAvatar(name: string): string {
    // Generate avatar URL with initials
    const initials = name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2);
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(initials)}&background=1a73e8&color=fff&size=80`;
  }

  onMenuClick(): void {
    this.menuToggle.emit();
  }

  onSearchFocus(): void {
    this.isSearchActive.set(true);
  }

  onSearchBlur(): void {
    if (!this.searchQuery()) {
      this.isSearchActive.set(false);
    }
  }

  onSearchInput(): void {
    this.searchChange.emit(this.searchQuery());
  }

  onSearchSubmit(event: Event): void {
    event.preventDefault();
    this.searchChange.emit(this.searchQuery());
  }

  clearSearch(): void {
    this.searchQuery.set('');
    this.searchChange.emit('');
    this.isSearchActive.set(false);
  }

  onRefreshClick(): void {
    this.noteService.refreshNotes();
    this.refresh.emit();
  }

  onViewToggleClick(): void {
    this.viewModeChange.emit(!this.isGridView);
  }

  toggleSettingsMenu(): void {
    this.showSettingsMenu.update(v => !v);
    this.showProfileMenu.set(false);
  }

  closeSettingsMenu(): void {
    this.showSettingsMenu.set(false);
  }

  onToggleTheme(): void {
    this.themeService.toggleTheme();
    this.closeSettingsMenu();
  }

  onSettingsClick(): void {
    this.toggleSettingsMenu();
  }

  onAppsClick(): void {
    console.log('Apps clicked');
  }

  toggleProfileMenu(): void {
    this.showProfileMenu.update(v => !v);
    this.showSettingsMenu.set(false);
  }

  closeProfileMenu(): void {
    this.showProfileMenu.set(false);
  }

  onLogout(): void {
    this.authService.logout().subscribe({
      next: () => {
        this.router.navigate(['/login']);
      },
      error: () => {
        // Even if logout fails on server, clear local storage and redirect
        this.authService.clearStorage();
        this.router.navigate(['/login']);
      }
    });
  }
}
