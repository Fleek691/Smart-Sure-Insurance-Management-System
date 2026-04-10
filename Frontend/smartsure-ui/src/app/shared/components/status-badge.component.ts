import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-status-badge',
  template: '<span class="status-badge" [class]="statusClass">{{ label }}</span>',
  styles: [
    '.status-badge { display: inline-flex; align-items: center; justify-content: center; border-radius: 999px; padding: 0.3rem 0.7rem; font-size: 0.72rem; font-weight: 700; letter-spacing: 0.05em; text-transform: uppercase; white-space: nowrap; border: 1px solid transparent; }',
    '.status-badge.is-draft, .status-badge.is-pending, .status-badge.is-review { background: rgba(84, 101, 255, 0.08); color: #5465ff; border-color: rgba(84, 101, 255, 0.15); }',
    '.status-badge.is-active, .status-badge.is-approved, .status-badge.is-success { background: rgba(16, 185, 129, 0.08); color: #059669; border-color: rgba(16, 185, 129, 0.15); }',
    '.status-badge.is-rejected, .status-badge.is-cancelled, .status-badge.is-error { background: rgba(239, 68, 68, 0.06); color: #dc2626; border-color: rgba(239, 68, 68, 0.12); }',
    '.status-badge.is-unknown { background: rgba(84, 101, 255, 0.05); color: #5f6b82; border-color: rgba(84, 101, 255, 0.1); }'
  ]
})
export class StatusBadgeComponent {
  @Input() value = '';

  get label(): string {
    return this.value.replace(/_/g, ' ');
  }

  get statusClass(): string {
    const normalized = this.value.toLowerCase();

    if (normalized.includes('draft')) {
      return 'status-badge is-draft';
    }

    if (normalized.includes('pending') || normalized.includes('review')) {
      return 'status-badge is-review';
    }

    if (normalized.includes('active') || normalized.includes('approved') || normalized.includes('success')) {
      return 'status-badge is-active';
    }

    if (normalized.includes('rejected') || normalized.includes('cancelled') || normalized.includes('error')) {
      return 'status-badge is-rejected';
    }

    return 'status-badge is-unknown';
  }
}
