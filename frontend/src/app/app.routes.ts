import { Routes } from '@angular/router';
import { HomeComponent } from './features/dashboard/pages/home/home';
import { Login } from './features/auth/pages/login/login';
import { Register } from './features/auth/pages/register/register';
import { Pagenotfound } from './features/dashboard/pages/pagenotfound/pagenotfound';
import { ArchiveComponent } from './features/dashboard/pages/archive/archive';
import { TrashComponent } from './features/dashboard/pages/trash/trash';
import { LabelNotesComponent } from './features/dashboard/pages/label-notes/label-notes';
import { authGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    canActivate: [authGuard]
  },
  {
    path: 'login',
    component: Login,
    canActivate: [guestGuard]
  },
  {
    path: 'register',
    component: Register,
    canActivate: [guestGuard]
  },
  {
    path: 'archive',
    component: ArchiveComponent,
    canActivate: [authGuard]
  },
  {
    path: 'trash',
    component: TrashComponent,
    canActivate: [authGuard]
  },
  {
    path: 'label/:labelName',
    component: LabelNotesComponent,
    canActivate: [authGuard]
  },
  {
    path: '**',
    component: Pagenotfound
  }
];
