import { CommonModule } from '@angular/common';
import { NgModule } from '@angular/core';
import { FormatCurrencyPipe } from './pipes/format-currency.pipe';
import { StatusLabelPipe } from './pipes/status-label.pipe';
import { SafeHtmlPipe } from './pipes/safe-html.pipe';
import { StatusBadgeComponent } from './components/status-badge.component';
import { StepperComponent } from './components/stepper.component';

@NgModule({
  declarations: [StatusBadgeComponent, StepperComponent, FormatCurrencyPipe, StatusLabelPipe, SafeHtmlPipe],
  imports: [CommonModule],
  exports: [CommonModule, StatusBadgeComponent, StepperComponent, FormatCurrencyPipe, StatusLabelPipe, SafeHtmlPipe]
})
export class SharedModule {}
