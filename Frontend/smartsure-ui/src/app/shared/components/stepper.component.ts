import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'app-stepper',
  template: `
    <nav class="stepper" role="tablist">
      <ng-container *ngFor="let step of steps; let i = index; let last = last">
        <button
          type="button"
          class="step"
          [class.completed]="i < activeIndex"
          [class.active]="i === activeIndex"
          [class.upcoming]="i > activeIndex"
          [disabled]="i > activeIndex"
          (click)="onStepClick(i)"
          role="tab"
          [attr.aria-selected]="i === activeIndex">
          <span class="dot">
            <svg *ngIf="i < activeIndex" viewBox="0 0 16 16" fill="none" class="check-icon">
              <path d="M3.5 8.5L6.5 11.5L12.5 4.5" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            <span *ngIf="i >= activeIndex">{{ i + 1 }}</span>
          </span>
          <span class="label">{{ step }}</span>
        </button>
        <span class="connector" *ngIf="!last" [class.filled]="i < activeIndex"></span>
      </ng-container>
    </nav>
  `,
  styles: [`
    .stepper {
      display: flex; align-items: center; gap: 0;
      background: rgba(84, 101, 255, 0.03);
      border: 1px solid rgba(84, 101, 255, 0.08);
      border-radius: var(--radius-lg, 20px); padding: 1rem 1.5rem;
      margin-bottom: 0.5rem;
    }
    .step {
      display: flex; align-items: center; gap: 0.6rem;
      border: 0; background: transparent; cursor: pointer;
      padding: 0.3rem 0; color: #a0aec0; font-weight: 600; font-size: 0.88rem;
      transition: all 0.2s ease; white-space: nowrap;
    }
    .step:disabled { cursor: default; }
    .step.active { color: var(--ink, #14213d); }
    .step.completed { color: #059669; }
    .step.completed:hover { opacity: 0.8; }
    .step.upcoming { color: #a0aec0; }

    .dot {
      width: 2rem; height: 2rem; border-radius: 999px;
      display: grid; place-items: center;
      font-weight: 700; font-size: 0.82rem;
      transition: all 0.25s ease; flex-shrink: 0;
    }
    .step.upcoming .dot {
      background: rgba(84, 101, 255, 0.06);
      color: #a0aec0; border: 2px solid rgba(84, 101, 255, 0.12);
    }
    .step.active .dot {
      background: linear-gradient(135deg, #5465ff 0%, #788bff 100%);
      color: #fff; border: 2px solid transparent;
      box-shadow: 0 4px 14px rgba(84, 101, 255, 0.35);
      animation: pulse-dot 2s ease infinite;
    }
    .step.completed .dot {
      background: rgba(16, 185, 129, 0.1);
      color: #059669; border: 2px solid rgba(16, 185, 129, 0.25);
    }
    .check-icon { width: 14px; height: 14px; }

    .label { letter-spacing: -0.01em; }

    .connector {
      flex: 1; height: 2px; min-width: 1.5rem;
      background: rgba(84, 101, 255, 0.1);
      border-radius: 2px; transition: background 0.3s ease;
      margin: 0 0.3rem;
    }
    .connector.filled { background: linear-gradient(90deg, #059669, #10b981); }

    @keyframes pulse-dot {
      0%, 100% { box-shadow: 0 4px 14px rgba(84, 101, 255, 0.35); }
      50% { box-shadow: 0 4px 20px rgba(84, 101, 255, 0.55); }
    }
  `]
})
export class StepperComponent {
  @Input() steps: string[] = [];
  @Input() activeIndex = 0;
  @Output() stepClick = new EventEmitter<number>();

  onStepClick(index: number): void {
    if (index < this.activeIndex) {
      this.stepClick.emit(index);
    }
  }
}
