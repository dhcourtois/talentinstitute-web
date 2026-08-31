import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { StaffService } from '../../core/services/staff.service';
import { ToastService } from '../../core/services/toast.service';
import { Rol, Staff } from '../../models';
import { BadgeComponent } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BadgeComponent, ButtonComponent, ModalComponent, SpinnerComponent],
  template: `
    <section class="page-shell">
      <header class="topbar">
        <div>
          <p class="eyebrow">Solo Principal</p>
          <h1>Staff</h1>
        </div>
        <app-button variant="ghost" (click)="load()">Actualizar</app-button>
      </header>

      <form [formGroup]="form" (ngSubmit)="create()" class="panel inline-form">
        <label>
          <span>Correo</span>
          <input type="email" formControlName="email" placeholder="monitora@talentinstitute.com" />
        </label>
        <label>
          <span>Contraseña</span>
          <input type="password" formControlName="password" placeholder="Mínimo 6 caracteres" />
        </label>
        <label>
          <span>Rol</span>
          <select formControlName="rol">
            <option *ngFor="let rol of roles" [value]="rol">{{ rol }}</option>
          </select>
        </label>
        <app-button [loading]="creating" [disabled]="form.invalid">Registrar</app-button>
      </form>

      <div class="state" *ngIf="loading">
        <app-spinner variant="overlay" />
      </div>

      <div class="state error" *ngIf="!loading && errorMessage">
        <p>{{ errorMessage }}</p>
        <app-button (click)="load()">Reintentar</app-button>
      </div>

      <div class="panel" *ngIf="!loading && !errorMessage">
        <div class="empty" *ngIf="staff.length === 0">Aún no hay personal registrado.</div>

        <table *ngIf="staff.length > 0">
          <thead>
            <tr>
              <th scope="col">Correo</th>
              <th scope="col">Rol</th>
              <th scope="col">Estado</th>
              <th scope="col"><span class="sr-only">Acciones</span></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let miembro of staff">
              <th scope="row">{{ miembro.email }}</th>
              <td>
                <select
                  [value]="miembro.rol"
                  [disabled]="!miembro.activo || updatingId === miembro.id"
                  [attr.aria-label]="'Rol de ' + miembro.email"
                  (change)="changeRol(miembro, $any($event.target).value)"
                >
                  <option *ngFor="let rol of roles" [value]="rol">{{ rol }}</option>
                </select>
              </td>
              <td>
                <app-badge [variant]="miembro.activo ? 'green' : 'gray'">
                  {{ miembro.activo ? 'Activo' : 'Inactivo' }}
                </app-badge>
              </td>
              <td class="row-actions">
                <app-button
                  variant="secondary"
                  [disabled]="!miembro.activo"
                  (click)="askDeactivate(miembro)"
                >
                  Desactivar
                </app-button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>

    <app-modal
      [visible]="!!pendingDeactivation"
      title="Desactivar miembro del staff"
      confirmLabel="Desactivar"
      confirmVariant="danger"
      [confirmLoading]="deactivating"
      (confirmed)="confirmDeactivate()"
      (cancelled)="pendingDeactivation = null"
    >
      <p>
        {{ pendingDeactivation?.email }} dejará de poder iniciar sesión. Su historial de méritos y
        calificaciones se conserva.
      </p>
    </app-modal>
  `,
  styles: [`
    .page-shell {
      display: grid;
      gap: var(--space-5);
      padding: var(--space-6);
    }

    .topbar {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
    }

    .topbar h1 {
      margin: 0;
    }

    .eyebrow {
      margin: 0;
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }

    .panel {
      padding: var(--space-5);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      background: var(--color-bg-surface);
      overflow-x: auto;
    }

    .inline-form {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
      gap: var(--space-4);
      align-items: end;
    }

    label {
      display: grid;
      gap: var(--space-2);
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
    }

    input,
    select {
      width: 100%;
      min-height: var(--tap-target-min, 44px);
      padding: 0 var(--space-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
    }

    table {
      width: 100%;
      border-collapse: collapse;
    }

    th, td {
      padding: var(--space-3);
      text-align: left;
      border-bottom: 1px solid var(--color-border);
    }

    thead th {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }

    .row-actions {
      text-align: right;
    }

    .empty,
    .state {
      display: grid;
      place-items: center;
      min-height: 140px;
      padding: var(--space-6);
      border: 1px dashed var(--color-border);
      border-radius: var(--radius-md);
      color: var(--color-text-secondary);
      background: var(--color-bg-surface);
    }

    .state.error {
      gap: var(--space-4);
      color: var(--color-danger);
      background: var(--color-danger-bg);
    }

    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      overflow: hidden;
      clip: rect(0 0 0 0);
      white-space: nowrap;
    }
  `]
})
export class StaffComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(StaffService);
  private readonly toast = inject(ToastService);

  readonly roles: Rol[] = ['Principal', 'Supervisora', 'Monitora'];

  staff: Staff[] = [];
  loading = true;
  creating = false;
  deactivating = false;
  updatingId: string | null = null;
  errorMessage = '';
  pendingDeactivation: Staff | null = null;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    rol: ['Monitora' as Rol, Validators.required]
  });

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    this.service.getAll().subscribe({
      next: staff => {
        this.staff = staff;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = error?.error?.message ?? 'No se pudo cargar el personal.';
      }
    });
  }

  create(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.creating = true;
    const { email, password, rol } = this.form.getRawValue();

    this.service.create(email, password, rol).subscribe({
      next: response => {
        this.creating = false;
        this.form.reset({ email: '', password: '', rol: 'Monitora' });
        this.toast.success(response?.message ?? 'Miembro registrado.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.creating = false;
        this.toast.error(error?.error?.message ?? 'No se pudo registrar al miembro del staff.');
      }
    });
  }

  changeRol(miembro: Staff, rol: Rol): void {
    if (rol === miembro.rol) return;

    this.updatingId = miembro.id;
    this.service.updateRol(miembro.id, rol).subscribe({
      next: response => {
        this.updatingId = null;
        this.toast.success(response?.message ?? 'Rol actualizado.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.updatingId = null;
        this.toast.error(error?.error?.message ?? 'No se pudo actualizar el rol.');
        this.load();
      }
    });
  }

  askDeactivate(miembro: Staff): void {
    this.pendingDeactivation = miembro;
  }

  confirmDeactivate(): void {
    if (!this.pendingDeactivation) return;

    this.deactivating = true;
    this.service.deactivate(this.pendingDeactivation.id).subscribe({
      next: response => {
        this.deactivating = false;
        this.pendingDeactivation = null;
        this.toast.success(response?.message ?? 'Miembro desactivado.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.deactivating = false;
        this.pendingDeactivation = null;
        this.toast.error(error?.error?.message ?? 'No se pudo desactivar al miembro.');
      }
    });
  }
}
