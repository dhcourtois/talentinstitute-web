import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { Rol } from '../../models';
import { rutaInicial } from '../../core/auth/permissions';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent],
  template: `
    <main class="login-shell">
      <section class="login-panel" aria-labelledby="login-title">
        <div class="brand">
          <span class="brand-mark" aria-hidden="true">TI</span>
          <div>
            <p class="eyebrow">Sistema ACE</p>
            <h1 id="login-title">Talent Institute</h1>
          </div>
        </div>

        <form [formGroup]="form" (ngSubmit)="submit()" class="login-form" novalidate>
          <label>
            <span>Correo</span>
            <input
              type="email"
              formControlName="email"
              autocomplete="username"
              placeholder="supervisora@talentinstitute.com"
              [class.invalid]="isInvalid('email')"
            />
          </label>
          <p class="field-error" *ngIf="isInvalid('email')">Ingresa un correo válido.</p>

          <label>
            <span>Contraseña</span>
            <div class="password-field">
              <input
                [type]="passwordVisible ? 'text' : 'password'"
                formControlName="password"
                autocomplete="current-password"
                placeholder="Mínimo 6 caracteres"
                [class.invalid]="isInvalid('password')"
              />
              <button
                type="button"
                class="reveal"
                [attr.aria-label]="passwordVisible ? 'Ocultar contraseña' : 'Mostrar contraseña mientras mantienes presionado'"
                [attr.aria-pressed]="passwordVisible"
                (pointerdown)="revealPassword($event)"
                (pointerup)="hidePassword()"
                (pointerleave)="hidePassword()"
                (pointercancel)="hidePassword()"
                (blur)="hidePassword()"
                (keydown)="onRevealKeydown($event)"
                (keyup)="onRevealKeyup($event)"
              >
                <svg viewBox="0 0 24 24" aria-hidden="true" focusable="false">
                  <path
                    d="M1.5 12S5.2 5.5 12 5.5 22.5 12 22.5 12 18.8 18.5 12 18.5 1.5 12 1.5 12Z"
                    fill="none"
                    stroke="currentColor"
                    stroke-width="1.6"
                    stroke-linecap="round"
                    stroke-linejoin="round"
                  />
                  <circle cx="12" cy="12" r="3.1" fill="none" stroke="currentColor" stroke-width="1.6" />
                  <line
                    *ngIf="!passwordVisible"
                    x1="4"
                    y1="20"
                    x2="20"
                    y2="4"
                    stroke="currentColor"
                    stroke-width="1.6"
                    stroke-linecap="round"
                  />
                </svg>
              </button>
            </div>
          </label>
          <p class="field-error" *ngIf="isInvalid('password')">La contraseña debe tener al menos 6 caracteres.</p>

          <fieldset class="role-picker">
            <legend>Vista inicial</legend>
            <button
              type="button"
              *ngFor="let role of roles"
              [class.active]="form.controls.rol.value === role"
              (click)="form.controls.rol.setValue(role)"
            >
              {{ role }}
            </button>
          </fieldset>

          <p class="login-error" *ngIf="errorMessage">{{ errorMessage }}</p>

          <app-button [loading]="loading" [disabled]="form.invalid">
            Ingresar
          </app-button>
        </form>
      </section>
    </main>

    <div class="intro-overlay" *ngIf="showIntro" role="dialog" aria-modal="true" aria-labelledby="intro-title">
      <section class="intro-panel">
        <h2 id="intro-title">Primer acceso {{ introRole }}</h2>
        <ul>
          <li>El dashboard concentra alumnos, alertas y PACEs en revisión.</li>
          <li>Desde cada alumno puedes revisar metas, PACEs y méritos.</li>
          <li>Las acciones restringidas aparecen según el rol de la sesión.</li>
        </ul>
        <app-button (click)="finishIntro()">Continuar</app-button>
      </section>
    </div>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
      background: var(--color-bg-page);
    }

    .login-shell {
      min-height: 100vh;
      display: grid;
      place-items: center;
      padding: var(--space-6);
    }

    .login-panel,
    .intro-panel {
      width: min(100%, 440px);
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-card);
      padding: var(--space-8);
    }

    .brand {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      margin-bottom: var(--space-8);
    }

    .brand-mark {
      display: grid;
      place-items: center;
      width: 52px;
      height: 52px;
      border-radius: var(--radius-md);
      background: var(--color-accent);
      color: var(--color-text-inverse);
      font-weight: var(--font-weight-bold);
    }

    .eyebrow {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
      margin-bottom: var(--space-1);
    }

    h1,
    h2 {
      font-size: var(--font-size-2xl);
      line-height: var(--line-height-tight);
      margin: 0;
    }

    .login-form {
      display: grid;
      gap: var(--space-4);
    }

    label {
      display: grid;
      gap: var(--space-2);
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
    }

    input {
      min-height: var(--tap-target-min);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      padding: 0 var(--space-4);
      color: var(--color-text-primary);
      background: var(--color-bg-surface);
    }

    input.invalid {
      border-color: var(--color-danger);
      background: var(--color-danger-bg);
    }

    .field-error,
    .login-error {
      color: var(--color-danger);
      font-size: var(--font-size-sm);
      margin-top: calc(-1 * var(--space-2));
    }

    .login-error {
      background: var(--color-danger-bg);
      border: 1px solid var(--color-danger);
      border-radius: var(--radius-sm);
      padding: var(--space-3);
      margin-top: 0;
    }

    .password-field {
      position: relative;
      display: flex;
      align-items: center;
    }

    .password-field input {
      width: 100%;
      padding-right: calc(var(--tap-target-min, 44px) + var(--space-2));
    }

    .reveal {
      position: absolute;
      right: 0;
      display: grid;
      place-items: center;
      width: var(--tap-target-min, 44px);
      height: var(--tap-target-min, 44px);
      border: none;
      border-radius: var(--radius-sm);
      background: transparent;
      color: var(--color-text-secondary);
      cursor: pointer;
      touch-action: manipulation;
      -webkit-tap-highlight-color: transparent;
    }

    .reveal:hover {
      color: var(--color-text-primary);
    }

    .reveal:focus-visible {
      outline: 2px solid var(--color-primary);
      outline-offset: -2px;
    }

    .reveal svg {
      width: 20px;
      height: 20px;
    }

    .role-picker {
      border: 0;
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: var(--space-2);
    }

    legend {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
      margin-bottom: var(--space-2);
    }

    .role-picker button {
      min-height: var(--tap-target-min);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      color: var(--color-text-secondary);
      cursor: pointer;
      font-weight: var(--font-weight-semibold);
    }

    .role-picker button.active {
      border-color: var(--color-accent);
      background: var(--color-accent-light);
      color: var(--color-accent);
    }

    .intro-overlay {
      position: fixed;
      inset: 0;
      z-index: var(--z-overlay);
      display: grid;
      place-items: center;
      padding: var(--space-6);
      background: var(--color-bg-overlay);
    }

    .intro-panel {
      display: grid;
      gap: var(--space-5);
    }

    .intro-panel ul {
      display: grid;
      gap: var(--space-3);
      padding-left: var(--space-5);
      color: var(--color-text-secondary);
    }

    @media (max-width: 560px) {
      .login-shell {
        padding: var(--space-4);
      }

      .login-panel,
      .intro-panel {
        padding: var(--space-6);
      }

      .role-picker {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);

  // 'Padre' entra aquí para que la cuenta del portal pueda elegir su propia
  // vista inicial; el rol real sigue viniendo del token (issue #8).
  readonly roles: Rol[] = ['Supervisora', 'Monitora', 'Principal', 'Padre'];
  passwordVisible = false;
  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rol: ['Supervisora' as Rol]
  });

  loading = false;
  errorMessage = '';
  showIntro = false;
  introRole: Rol = 'Supervisora';

  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  // La contraseña solo se revela mientras el control está presionado: soltar,
  // salir del ícono o perder el foco la vuelve a ocultar de inmediato.
  revealPassword(event: PointerEvent): void {
    event.preventDefault();
    this.passwordVisible = true;
  }

  hidePassword(): void {
    this.passwordVisible = false;
  }

  onRevealKeydown(event: KeyboardEvent): void {
    if (event.key === ' ' || event.key === 'Enter') {
      event.preventDefault();
      this.passwordVisible = true;
    }
  }

  onRevealKeyup(event: KeyboardEvent): void {
    if (event.key === ' ' || event.key === 'Enter') {
      event.preventDefault();
      this.passwordVisible = false;
    }
  }

  isInvalid(controlName: 'email' | 'password'): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.dirty || control.touched);
  }

  submit(): void {
    this.errorMessage = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { email, password, rol } = this.form.getRawValue();
    this.loading = true;

    this.auth.login({ email, password, vistaInicial: rol }).subscribe({
      next: () => {
        this.loading = false;
        this.introRole = this.auth.getRole() ?? rol;
        const introKey = `ti_intro_seen_${this.introRole}`;

        if (!localStorage.getItem(introKey)) {
          this.showIntro = true;
          return;
        }

        this.irAlInicio();
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err?.error?.message ?? 'Credenciales incorrectas.';
      }
    });
  }

  finishIntro(): void {
    localStorage.setItem(`ti_intro_seen_${this.introRole}`, 'true');
    this.showIntro = false;
    this.irAlInicio();
  }

  /**
   * El destino depende del rol. Navegar siempre a /dashboard mandaba al padre
   * de familia a una pantalla que el guard le rebota (issue #8).
   */
  private irAlInicio(): void {
    this.router.navigate([rutaInicial(this.auth.getRole())]);
  }
}
