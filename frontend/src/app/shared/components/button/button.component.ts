import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SpinnerComponent } from '../spinner/spinner.component';

export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'ghost';

@Component({
  selector: 'app-button',
  standalone: true,
  imports: [CommonModule, SpinnerComponent],
  template: `
    <button
      [class]="'btn btn--' + variant"
      [disabled]="disabled || loading"
      [attr.aria-busy]="loading"
    >
      <app-spinner *ngIf="loading" variant="inline" />
      <ng-content *ngIf="!loading" />
    </button>
  `,
  styleUrl: './button.component.css'
})
export class ButtonComponent {
  @Input() variant: ButtonVariant = 'primary';
  @Input() loading = false;
  @Input() disabled = false;
}
