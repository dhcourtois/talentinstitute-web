import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container" aria-live="polite" aria-atomic="false">
      @for (toast of toastService.toasts().slice(0, 3); track toast.id) {
        <div class="toast" [class]="'toast--' + toast.type" role="alert">
          <span class="toast__icon">{{ iconMap[toast.type] }}</span>
          <span class="toast__message">{{ toast.message }}</span>
          <button class="toast__close" (click)="toastService.dismiss(toast.id)" aria-label="Cerrar">×</button>
        </div>
      }
    </div>
  `,
  styleUrl: './toast.component.css'
})
export class ToastComponent {
  readonly toastService = inject(ToastService);

  readonly iconMap: Record<string, string> = {
    success: '✓',
    warning: '⚠',
    error: '✕',
    info: 'ℹ'
  };
}
