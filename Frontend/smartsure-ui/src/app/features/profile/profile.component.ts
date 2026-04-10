import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ProfileDto, UpdateProfileDto } from '../../models/api-models';
import { AuthService } from '../../core/services/auth.service';
import { AuthStateService } from '../../core/services/auth-state.service';
import { SharedModule } from '../../shared/shared.module';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, SharedModule],
  template: `
    <section class="page-shell">

      <!-- Page Header -->
      <header class="page-header">
        <div class="header-left">
          <div class="page-icon">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 21v-2a4 4 0 00-4-4H8a4 4 0 00-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
          </div>
          <div>
            <p class="eyebrow">Account</p>
            <h1 class="page-title">My Profile</h1>
          </div>
        </div>
        <app-status-badge [value]="profile?.isActive ? 'ACTIVE' : 'INACTIVE'" *ngIf="profile"></app-status-badge>
      </header>

      <!-- Loading state -->
      <div class="content-card loading-card" *ngIf="!profile && !loadError">
        <div class="loading-spinner"></div>
        <p class="hint">Loading your profile...</p>
      </div>

      <!-- Error state -->
      <div class="content-card empty-state" *ngIf="loadError">
        <div class="empty-icon"><svg viewBox="0 0 48 48" fill="none" stroke="#d97706" stroke-width="2"><path d="M24 6L4 42h40L24 6z" fill="rgba(245,158,11,0.08)" stroke-linejoin="round"/><path d="M24 18v10" stroke-linecap="round"/><circle cx="24" cy="34" r="1.5" fill="#d97706"/></svg></div>
        <h3>Unable to load profile</h3>
        <p>{{ loadError }}</p>
        <button type="button" class="primary-button" (click)="loadProfile()">Try Again</button>
      </div>

      <!-- Profile Content -->
      <ng-container *ngIf="profile">

        <!-- Profile Overview Card -->
        <div class="profile-hero">
          <div class="avatar">
            <span>{{ getInitials() }}</span>
          </div>
          <div class="hero-info">
            <h2>{{ profile.fullName }}</h2>
            <p class="hero-email">{{ profile.email }}</p>
            <div class="hero-meta">
              <span class="meta-chip">
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="2" y="2" width="12" height="12" rx="2"/><path d="M2 6h12"/><path d="M6 2v4"/><path d="M10 2v4"/></svg>
                Joined {{ profile.createdAt | date:'mediumDate' }}
              </span>
              <span class="meta-chip" *ngIf="profile.lastLoginAt">
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="8" cy="8" r="6"/><path d="M8 4v4l3 2"/></svg>
                Last login {{ profile.lastLoginAt | date:'medium' }}
              </span>
            </div>
          </div>
        </div>

        <!-- Edit Profile Form -->
        <div class="content-card">
          <div class="card-header">
            <h2 class="card-title">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M11 4H4a2 2 0 00-2 2v10a2 2 0 002 2h10a2 2 0 002-2v-7"/><path d="M17.5 2.5a2.121 2.121 0 013 3L12 14l-4 1 1-4 8.5-8.5z"/></svg>
              Personal Information
            </h2>
            <button type="button" class="ghost-link" *ngIf="!editing" (click)="startEditing()">
              <svg viewBox="0 0 14 14" fill="none" stroke="currentColor" stroke-width="1.5" style="width:13px;height:13px;margin-right:3px;vertical-align:-1px"><path d="M10.5 1.5a1.5 1.5 0 012.12 2.12L5 11.25 2 12l.75-3L10.5 1.5z"/></svg>
              Edit
            </button>
            <button type="button" class="ghost-link" *ngIf="editing" (click)="cancelEditing()">✕ Cancel</button>
          </div>

          <!-- Read-only view -->
          <div class="info-grid" *ngIf="!editing">
            <div class="info-item">
              <span class="info-label">Full Name</span>
              <strong>{{ profile.fullName }}</strong>
            </div>
            <div class="info-item">
              <span class="info-label">Email Address</span>
              <strong>{{ profile.email }}</strong>
              <span class="info-note">Email cannot be changed</span>
            </div>
            <div class="info-item">
              <span class="info-label">Phone Number</span>
              <strong>{{ profile.phoneNumber || '— Not set' }}</strong>
            </div>
            <div class="info-item">
              <span class="info-label">Address</span>
              <strong>{{ profile.address || '— Not set' }}</strong>
            </div>
          </div>

          <!-- Edit form -->
          <form class="edit-form" *ngIf="editing" (ngSubmit)="saveProfile()">
            <div class="form-grid">
              <label class="form-field">
                <span class="field-label">Full Name</span>
                <input name="fullName" [(ngModel)]="editModel.fullName" required minlength="2" placeholder="Your full name" />
              </label>
              <label class="form-field">
                <span class="field-label">Email Address</span>
                <input value="{{ profile.email }}" disabled class="disabled-input" />
                <span class="field-hint">Email cannot be changed for security reasons</span>
              </label>
              <label class="form-field">
                <span class="field-label">Phone Number</span>
                <input name="phoneNumber" [(ngModel)]="editModel.phoneNumber" placeholder="e.g. +91 98765 43210" />
              </label>
              <label class="form-field full-width">
                <span class="field-label">Address</span>
                <textarea name="address" [(ngModel)]="editModel.address" rows="3" placeholder="Your full address"></textarea>
              </label>
            </div>
            <div class="form-actions">
              <button type="button" class="secondary-button" (click)="cancelEditing()">Cancel</button>
              <button type="submit" class="primary-button" [disabled]="saving">
                <svg viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="2" *ngIf="!saving"><path d="M3 8.5l3 3 7-7"/></svg>
                {{ saving ? 'Saving...' : 'Save Changes' }}
              </button>
            </div>
          </form>

          <p class="success-msg" *ngIf="successMsg">
            <svg viewBox="0 0 16 16" fill="none" stroke="#059669" stroke-width="2" style="width:14px;height:14px;vertical-align:-2px;margin-right:4px"><circle cx="8" cy="8" r="7"/><path d="M5 8l2 2 4-4"/></svg>
            {{ successMsg }}
          </p>
          <p class="error-msg" *ngIf="errorMsg">
            <svg viewBox="0 0 16 16" fill="none" stroke="#dc2626" stroke-width="2" style="width:14px;height:14px;vertical-align:-2px;margin-right:4px"><circle cx="8" cy="8" r="7"/><path d="M5 5l6 6"/><path d="M11 5l-6 6"/></svg>
            {{ errorMsg }}
          </p>
        </div>

        <!-- Security Section -->
        <div class="content-card">
          <div class="card-header">
            <h2 class="card-title">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="3" y="9" width="14" height="9" rx="2"/><path d="M7 9V6a3 3 0 016 0v3"/></svg>
              Security
            </h2>
          </div>
          <div class="security-grid">
            <div class="security-item">
              <div class="security-info">
                <strong>Password</strong>
                <span>Change your account password. You'll receive a reset link via email.</span>
              </div>
              <button type="button" class="secondary-button" (click)="requestPasswordChange()" [disabled]="passwordResetSent">
                {{ passwordResetSent ? 'Link Sent ✓' : 'Change Password' }}
              </button>
            </div>
            <div class="security-item">
              <div class="security-info">
                <strong>Account Status</strong>
                <span>Your account is currently <strong [class.active-text]="profile.isActive" [class.inactive-text]="!profile.isActive">{{ profile.isActive ? 'Active' : 'Deactivated' }}</strong>.</span>
              </div>
              <app-status-badge [value]="profile.isActive ? 'ACTIVE' : 'INACTIVE'"></app-status-badge>
            </div>
          </div>
        </div>

        <!-- Account Info Card -->
        <div class="content-card">
          <div class="card-header">
            <h2 class="card-title">
              <svg viewBox="0 0 20 20" fill="none" stroke="currentColor" stroke-width="1.5"><circle cx="10" cy="10" r="8"/><path d="M10 7v3"/><circle cx="10" cy="13.5" r="0.5" fill="currentColor"/></svg>
              Account Details
            </h2>
          </div>
          <div class="info-grid compact">
            <div class="info-item"><span class="info-label">User ID</span><strong class="mono">{{ profile.userId }}</strong></div>
            <div class="info-item"><span class="info-label">Member Since</span><strong>{{ profile.createdAt | date:'longDate' }}</strong></div>
            <div class="info-item"><span class="info-label">Last Login</span><strong>{{ profile.lastLoginAt ? (profile.lastLoginAt | date:'medium') : 'Never' }}</strong></div>
            <div class="info-item"><span class="info-label">Role</span><strong>{{ authState.hasRole('ADMIN') ? 'Admin' : 'Customer' }}</strong></div>
          </div>
        </div>
      </ng-container>
    </section>
  `,
  styles: [`
    :host { display: block; animation: fadeInUp 0.4s ease; }
    .page-shell { display: grid; gap: 1.25rem; }

    /* ── Page Header ── */
    .page-header { display: flex; justify-content: space-between; align-items: center; flex-wrap: wrap; gap: 1rem; }
    .header-left { display: flex; align-items: center; gap: 0.85rem; }
    .page-icon {
      width: 2.6rem; height: 2.6rem; border-radius: var(--radius, 14px);
      background: linear-gradient(135deg, #5465ff 0%, #788bff 100%);
      display: grid; place-items: center; flex-shrink: 0;
    }
    .page-icon svg { width: 1.3rem; height: 1.3rem; color: white; }
    .eyebrow { text-transform: uppercase; letter-spacing: 0.14em; font-size: 0.65rem; font-weight: 700; color: var(--primary-2, #788bff); margin: 0; }
    .page-title { margin: 0; font-family: 'Space Grotesk', sans-serif; font-size: 1.4rem; font-weight: 700; color: var(--ink, #14213d); letter-spacing: -0.02em; line-height: 1.2; }

    /* ── Profile Hero ── */
    .profile-hero {
      display: flex; align-items: center; gap: 1.5rem;
      border-radius: var(--radius-lg, 20px); padding: 2rem;
      background: linear-gradient(135deg, rgba(84, 101, 255, 0.06) 0%, rgba(155, 177, 255, 0.1) 50%, rgba(226, 253, 255, 0.15) 100%);
      border: 1px solid rgba(84, 101, 255, 0.1);
    }
    .avatar {
      width: 5rem; height: 5rem; border-radius: 999px; flex-shrink: 0;
      background: linear-gradient(135deg, #5465ff 0%, #788bff 100%);
      display: grid; place-items: center;
      box-shadow: 0 8px 24px rgba(84, 101, 255, 0.3);
    }
    .avatar span { color: white; font-size: 1.6rem; font-weight: 700; font-family: 'Space Grotesk', sans-serif; letter-spacing: 0.04em; }
    .hero-info h2 { margin: 0; font-family: 'Space Grotesk', sans-serif; font-size: 1.5rem; font-weight: 700; color: var(--ink); }
    .hero-email { color: var(--muted, #6b7a99); font-size: 0.9rem; margin: 0.2rem 0 0.75rem; }
    .hero-meta { display: flex; flex-wrap: wrap; gap: 0.5rem; }
    .meta-chip {
      display: inline-flex; align-items: center; gap: 0.35rem;
      background: rgba(255, 255, 255, 0.7); border: 1px solid rgba(84, 101, 255, 0.08);
      border-radius: var(--radius-full, 999px); padding: 0.35rem 0.75rem;
      font-size: 0.78rem; color: var(--muted); font-weight: 500;
    }
    .meta-chip svg { width: 13px; height: 13px; }

    /* ── Content Card ── */
    .content-card {
      border-radius: var(--radius-lg, 20px); padding: 1.5rem 1.75rem;
      background: rgba(255, 255, 255, 0.9); backdrop-filter: blur(12px);
      border: 1px solid rgba(84, 101, 255, 0.08);
      box-shadow: var(--shadow, 0 2px 8px rgba(0,0,0,0.04));
    }
    .card-header { display: flex; justify-content: space-between; align-items: center; gap: 1rem; margin-bottom: 1.25rem; }
    .card-title { margin: 0; font-size: 1.1rem; font-weight: 700; font-family: 'Space Grotesk', sans-serif; color: var(--ink); display: flex; align-items: center; gap: 0.5rem; }
    .card-title svg { width: 1.1rem; height: 1.1rem; color: var(--primary-2, #788bff); }

    /* ── Info Grid (read-only) ── */
    .info-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 0.75rem; }
    .info-grid.compact .info-item { padding: 0.85rem; }
    .info-item {
      display: grid; gap: 0.3rem; padding: 1rem;
      border-radius: var(--radius-sm, 10px); background: rgba(255, 255, 255, 0.75);
      border: 1px solid rgba(84, 101, 255, 0.06); transition: transform 0.2s ease;
    }
    .info-item:hover { transform: translateY(-1px); }
    .info-label { font-size: 0.75rem; color: var(--muted); font-weight: 600; text-transform: uppercase; letter-spacing: 0.04em; }
    .info-item strong { font-size: 0.95rem; color: var(--ink); word-break: break-word; }
    .info-note { font-size: 0.72rem; color: var(--muted); font-style: italic; }
    .mono { font-family: 'SF Mono', Monaco, Consolas, monospace; font-size: 0.8rem !important; letter-spacing: -0.01em; }

    /* ── Form Grid ── */
    .form-grid { display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem; }
    .form-field { display: grid; gap: 0.3rem; }
    .form-field.full-width { grid-column: 1 / -1; }
    .field-label { font-weight: 600; color: var(--ink); font-size: 0.88rem; }
    .field-hint { font-size: 0.75rem; color: var(--muted); }
    input, textarea {
      border: 1.5px solid rgba(84, 101, 255, 0.14); background: rgba(255, 255, 255, 0.8);
      border-radius: var(--radius-sm, 10px); padding: 0.82rem 1rem; color: var(--ink); font-size: 0.92rem;
      transition: all 0.2s ease; font-family: inherit;
    }
    input:focus, textarea:focus { border-color: var(--primary-2, #788bff); background: white; box-shadow: 0 0 0 4px rgba(84, 101, 255, 0.1); outline: none; }
    textarea { resize: vertical; min-height: 4rem; }
    .disabled-input { background: rgba(84, 101, 255, 0.04) !important; color: var(--muted) !important; cursor: not-allowed; }

    .form-actions { display: flex; justify-content: flex-end; align-items: center; gap: 0.75rem; margin-top: 1.25rem; padding-top: 1.25rem; border-top: 1px solid rgba(84, 101, 255, 0.06); }

    /* ── Security Grid ── */
    .security-grid { display: grid; gap: 0.75rem; }
    .security-item {
      display: flex; justify-content: space-between; align-items: center; gap: 1.5rem;
      padding: 1.15rem; border-radius: var(--radius, 14px);
      border: 1px solid rgba(84, 101, 255, 0.06); background: rgba(255, 255, 255, 0.7);
      transition: all 0.2s ease;
    }
    .security-item:hover { border-color: rgba(84, 101, 255, 0.12); }
    .security-info strong { display: block; font-size: 0.92rem; color: var(--ink); margin-bottom: 0.2rem; }
    .security-info span { font-size: 0.82rem; color: var(--muted); line-height: 1.5; }
    .active-text { color: #059669 !important; }
    .inactive-text { color: #dc2626 !important; }

    /* ── Buttons ── */
    .primary-button, .secondary-button {
      border: 0; border-radius: var(--radius-sm, 10px); padding: 0.82rem 1.5rem;
      font-weight: 700; font-size: 0.88rem; cursor: pointer; transition: all 0.2s ease;
      display: inline-flex; align-items: center; gap: 0.4rem;
    }
    .primary-button { color: white; background: linear-gradient(135deg, #5465ff 0%, #788bff 100%); box-shadow: 0 4px 14px rgba(84, 101, 255, 0.25); }
    .primary-button:hover:not(:disabled) { transform: translateY(-1px); box-shadow: 0 6px 20px rgba(84, 101, 255, 0.35); }
    .primary-button:disabled { opacity: 0.55; cursor: not-allowed; }
    .primary-button svg { width: 16px; height: 16px; }
    .secondary-button { background: rgba(84, 101, 255, 0.07); color: var(--primary, #5465ff); }
    .secondary-button:hover:not(:disabled) { background: rgba(84, 101, 255, 0.12); }
    .secondary-button:disabled { opacity: 0.5; cursor: not-allowed; }
    .ghost-link { background: transparent; border: 0; color: var(--primary, #5465ff); cursor: pointer; font-weight: 700; font-size: 0.82rem; padding: 0.3rem 0; }
    .ghost-link:hover { opacity: 0.8; }

    .hint { color: var(--muted); font-size: 0.85rem; text-align: center; }
    .success-msg { color: #059669; font-size: 0.88rem; font-weight: 600; margin-top: 1rem; padding: 0.75rem 1rem; background: rgba(16, 185, 129, 0.06); border-radius: var(--radius-sm, 10px); border: 1px solid rgba(16, 185, 129, 0.12); }
    .error-msg { color: #dc2626; font-size: 0.88rem; font-weight: 600; margin-top: 1rem; padding: 0.75rem 1rem; background: rgba(239, 68, 68, 0.06); border-radius: var(--radius-sm, 10px); border: 1px solid rgba(239, 68, 68, 0.12); }

    /* ── Loading ── */
    .loading-card { display: flex; flex-direction: column; align-items: center; gap: 1rem; padding: 3rem; }
    .loading-spinner {
      width: 2.5rem; height: 2.5rem; border-radius: 999px;
      border: 3px solid rgba(84, 101, 255, 0.1); border-top-color: #5465ff;
      animation: spin 0.8s linear infinite;
    }
    @keyframes spin { to { transform: rotate(360deg); } }

    /* ── Empty State ── */
    .empty-state { text-align: center; padding: 2.5rem; }
    .empty-icon { width: 3rem; height: 3rem; margin: 0 auto 0.5rem; color: var(--primary-2, #788bff); }
    .empty-icon svg { width: 100%; height: 100%; }
    .empty-state h3 { margin: 0 0 0.3rem; font-family: 'Space Grotesk', sans-serif; font-weight: 700; color: var(--ink); }
    .empty-state p { color: var(--muted); font-size: 0.9rem; margin: 0 0 1rem; }

    @media (max-width: 768px) {
      .profile-hero { flex-direction: column; text-align: center; }
      .hero-meta { justify-content: center; }
      .info-grid, .form-grid { grid-template-columns: 1fr; }
      .form-field.full-width { grid-column: 1; }
      .security-item { flex-direction: column; text-align: center; gap: 0.75rem; }
    }
  `]
})
export class ProfileComponent implements OnInit {
  profile: ProfileDto | null = null;
  editing = false;
  saving = false;
  loadError = '';
  successMsg = '';
  errorMsg = '';
  passwordResetSent = false;

  editModel: UpdateProfileDto = {
    fullName: '',
    phoneNumber: '',
    address: ''
  };

  constructor(
    private readonly authService: AuthService,
    public readonly authState: AuthStateService,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.loadError = '';
    this.authService.getProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
      },
      error: (err) => {
        this.loadError = 'Could not load your profile. Please check your connection and try again.';
      }
    });
  }

  getInitials(): string {
    if (!this.profile?.fullName) return '?';
    const parts = this.profile.fullName.trim().split(' ');
    if (parts.length >= 2) {
      return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
    }
    return parts[0].substring(0, 2).toUpperCase();
  }

  startEditing(): void {
    if (!this.profile) return;
    this.editing = true;
    this.successMsg = '';
    this.errorMsg = '';
    this.editModel = {
      fullName: this.profile.fullName,
      phoneNumber: this.profile.phoneNumber || '',
      address: this.profile.address || ''
    };
  }

  cancelEditing(): void {
    this.editing = false;
    this.errorMsg = '';
  }

  saveProfile(): void {
    this.saving = true;
    this.errorMsg = '';
    this.successMsg = '';
    this.authService.updateProfile(this.editModel).subscribe({
      next: (updated) => {
        this.profile = updated;
        this.editing = false;
        this.successMsg = 'Profile updated successfully!';
        // Update the displayed name in the header
        this.authState.updateFullName(updated.fullName);
      },
      error: () => {
        this.errorMsg = 'Unable to save changes. Please try again.';
        this.saving = false;
      },
      complete: () => {
        this.saving = false;
      }
    });
  }

  requestPasswordChange(): void {
    if (!this.profile?.email) return;
    this.authService.requestPasswordReset({ email: this.profile.email }).subscribe({
      next: () => {
        this.passwordResetSent = true;
        this.successMsg = 'Password reset link has been sent to your email.';
      },
      error: () => {
        this.errorMsg = 'Unable to send password reset email. Please try again.';
      }
    });
  }
}
