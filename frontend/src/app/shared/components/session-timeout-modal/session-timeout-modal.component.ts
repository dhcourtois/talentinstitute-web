import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ButtonComponent } from '../button/button.component';

@Component({
  selector: 'app-session-timeout-modal',
  standalone: true,
  imports: [CommonModule, ButtonComponent],
  template: `
    @if (visible) {
      <div class="modal-overlay" role="dialog" aria-modal="true" aria-labelledby="session-timeout-title">
        <div class="modal">
          <header class="modal__header">
            <h2 class="modal__title" id="session-timeout-title">⏱ Sesión por expirar</h2>
          </header>
          <div class="modal__body">
            <p>Tu sesión actual va a terminar. ¿Deseas continuar?</p>
            @if (countdown > 0) {
              <p class="modal__countdown">La sesión se cerrará automáticamente en <strong>{{ countdown }}</strong> segundo{{ countdown === 1 ? '' : 's' }}.</p>
            }
          </div>
          <footer class="modal__footer">
            <app-button variant="secondary" (click)="loggedOut.emit()">Cerrar sesión</app-button>
            <app-button variant="primary" (click)="continued.emit()">Continuar</app-button>
          </footer>
        </div>
      </div>
    }
  `,
  styleUrl: './session-timeout-modal.component.css'
})
export class SessionTimeoutModalComponent {
  @Input() visible = false;
  /** Remaining seconds shown in the countdown. 0 hides the countdown line. */
  @Input() countdown = 0;

  @Output() continued = new EventEmitter<void>();
  @Output() loggedOut = new EventEmitter<void>();
}
