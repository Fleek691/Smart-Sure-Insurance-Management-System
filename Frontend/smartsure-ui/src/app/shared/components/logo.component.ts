import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-logo',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="logo-wrap" [style.width.px]="size" [style.height.px]="size">
      <svg xmlns="http://www.w3.org/2000/svg" [attr.viewBox]="'0 0 64 64'" fill="none" [attr.width]="size" [attr.height]="size">
        <defs>
          <linearGradient id="lg-shield" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stop-color="#5465FF"/>
            <stop offset="100%" stop-color="#788BFF"/>
          </linearGradient>
          <linearGradient id="lg-glow" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stop-color="#9BB1FF" stop-opacity="0.35"/>
            <stop offset="100%" stop-color="#5465FF" stop-opacity="0.08"/>
          </linearGradient>
        </defs>

        <!-- Outer glow ring -->
        <circle cx="32" cy="32" r="30" fill="url(#lg-glow)"/>

        <!-- Shield body fill -->
        <path d="M32 8 L10 17 L10 34 C10 46 20 55 32 58 C44 55 54 46 54 34 L54 17 Z"
              fill="url(#lg-shield)" opacity="0.15"/>
        <!-- Shield outline -->
        <path d="M32 8 L10 17 L10 34 C10 46 20 55 32 58 C44 55 54 46 54 34 L54 17 Z"
              fill="none" stroke="url(#lg-shield)" stroke-width="2.5" stroke-linejoin="round"/>

        <!-- Inner shield highlight -->
        <path d="M32 13 L15 20.5 L15 34 C15 43.5 22.5 51 32 53.5 C41.5 51 49 43.5 49 34 L49 20.5 Z"
              fill="url(#lg-shield)" opacity="0.08"/>

        <!-- Checkmark -->
        <path d="M21 33 L28 41 L43 24"
              stroke="url(#lg-shield)" stroke-width="4.5"
              stroke-linecap="round" stroke-linejoin="round"/>

        <!-- Spark dot -->
        <circle cx="47" cy="16" r="3.5" fill="#5465FF"/>
        <circle cx="47" cy="16" r="2" fill="#BFD7FF"/>

        <!-- Accent dots -->
        <circle cx="54" cy="24" r="1.8" fill="#9BB1FF" opacity="0.7"/>
        <circle cx="51" cy="12" r="1.2" fill="#788BFF" opacity="0.6"/>
      </svg>
    </div>
  `,
  styles: [`
    .logo-wrap {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }
    svg { display: block; }
  `]
})
export class LogoComponent {
  @Input() size = 40;
}
