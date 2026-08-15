import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type SpinnerVariant = 'inline' | 'overlay';

@Component({
  selector: 'app-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div [class]="'spinner spinner--' + variant" role="status" aria-label="Cargando...">
      <div class="spinner__ring"></div>
    </div>
  `,
  styleUrl: './spinner.component.css'
})
export class SpinnerComponent {
  @Input() variant: SpinnerVariant = 'inline';
}
