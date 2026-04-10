import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'statusLabel', standalone: false })
export class StatusLabelPipe implements PipeTransform {
  transform(value: string | null | undefined): string {
    return String(value ?? '').replace(/_/g, ' ');
  }
}
