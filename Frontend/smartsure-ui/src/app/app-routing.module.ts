import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'auth/login' },
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login.component').then((m) => m.LoginComponent)
  },
  {
    path: 'auth/register',
    loadComponent: () => import('./features/auth/register.component').then((m) => m.RegisterComponent)
  },
  {
    path: 'auth/recovery',
    loadComponent: () => import('./features/auth/recovery.component').then((m) => m.RecoveryComponent)
  },
  {
    path: 'auth/google/callback',
    loadComponent: () => import('./features/auth/google-callback.component').then((m) => m.GoogleCallbackComponent)
  },
  {
    path: 'customer/dashboard',
    loadComponent: () => import('./features/customer/customer-console.component').then((m) => m.CustomerConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['CUSTOMER', 'ADMIN'] }
  },
  {
    path: 'customer/buy-policy',
    loadComponent: () => import('./features/customer/customer-console.component').then((m) => m.CustomerConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['CUSTOMER'] }
  },
  {
    path: 'customer/policy/:id',
    loadComponent: () => import('./features/customer/customer-console.component').then((m) => m.CustomerConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['CUSTOMER'] }
  },
  {
    path: 'claims/my-claims',
    loadComponent: () => import('./features/claims/claims-console.component').then((m) => m.ClaimsConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['CUSTOMER'] }
  },
  {
    path: 'claims/new',
    loadComponent: () => import('./features/claims/claims-console.component').then((m) => m.ClaimsConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['CUSTOMER'] }
  },
  {
    path: 'claims/:id',
    loadComponent: () => import('./features/claims/claims-console.component').then((m) => m.ClaimsConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['CUSTOMER'] }
  },
  {
    path: 'admin/dashboard',
    loadComponent: () => import('./features/admin/admin-console.component').then((m) => m.AdminConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'admin/policies',
    loadComponent: () => import('./features/admin/admin-console.component').then((m) => m.AdminConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'admin/claims',
    loadComponent: () => import('./features/admin/admin-console.component').then((m) => m.AdminConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'admin/users',
    loadComponent: () => import('./features/admin/admin-console.component').then((m) => m.AdminConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'admin/audit-logs',
    loadComponent: () => import('./features/admin/admin-console.component').then((m) => m.AdminConsoleComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'admin/reports',
    loadComponent: () => import('./features/reports/reports-viewer.component').then((m) => m.ReportsViewerComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'admin/reports/:type',
    loadComponent: () => import('./features/reports/reports-viewer.component').then((m) => m.ReportsViewerComponent),
    canActivate: [authGuard, roleGuard],
    data: { roles: ['ADMIN'] }
  },
  {
    path: 'profile',
    loadComponent: () => import('./features/profile/profile.component').then((m) => m.ProfileComponent),
    canActivate: [authGuard],
    data: { roles: ['CUSTOMER', 'ADMIN'] }
  },
  { path: '**', redirectTo: 'auth/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
