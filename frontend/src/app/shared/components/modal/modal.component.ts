import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../button/button.component';

@Component({
  selector: 'app-modal',
  standalone: true,
  imports: [CommonModule, ButtonComponent],
  template: `
    @if (visible) {
      <div class="modal-overlay" (click)="onOverlayClick($event)" role="dialog" aria-modal="true" [attr.aria-label]="title">
        <div class="modal">
          <header class="modal__header">
            <h2 class="modal__title">{{ title }}</h2>
            <button class="modal__close" (click)="cancelled.emit()" aria-label="Cerrar">×</button>
          </header>
          <div class="modal__body">
            <ng-content />
          </div>
          <footer class="modal__footer">
            <app-button variant="secondary" (click)="cancelled.emit()">Cancelar</app-button>
            <app-button [variant]="confirmVariant" [loading]="confirmLoading" (click)="confirmed.emit()">
              {{ confirmLabel }}
            </app-button>
          </footer>
        </div>
      </div>
    }
  `,
  styleUrl: './modal.component.css'
})
export class ModalComponent {
  @Input() visible = false;
  @Input() title = 'Confirmar acción';
  @Input() confirmLabel = 'Confirmar';
  @Input() confirmVariant: 'primary' | 'danger' = 'primary';
  @Input() confirmLoading = false;

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  onOverlayClick(event: MouseEvent): void {
    if ((event.target as HTMLElement).classList.contains('modal-overlay')) {
      this.cancelled.emit();
    }
  }
}
