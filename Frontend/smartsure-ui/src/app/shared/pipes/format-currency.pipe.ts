import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'formatCurrency', standalone: false })
export class FormatCurrencyPipe implements PipeTransform {
  transform(value: number | string | null | undefined, currency = 'INR'): string {
    const amount = Number(value ?? 0);
    return new Intl.NumberFormat('en-IN', {
      style: 'currency',
      currency,
      maximumFractionDigits: 0
    }).format(Number.isFinite(amount) ? amount : 0);
  }
}

