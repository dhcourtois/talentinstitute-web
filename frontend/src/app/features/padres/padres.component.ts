import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin } from 'rxjs';
import { AlumnosService } from '../../core/services/alumnos.service';
import { PadresService } from '../../core/services/padres.service';
import { ToastService } from '../../core/services/toast.service';
import { Alumno, PadreFamilia } from '../../models';
import { BadgeComponent } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

/**
 * Alta y vinculación de cuentas de padres de familia (issue #8).
 *
 * Solo el Principal. Vincular un alumno a una cuenta decide quién puede ver su
 * expediente, así que es una atribución de dirección y no de operación diaria.
 */
@Component({
  selector: 'app-padres',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BadgeComponent, ButtonComponent, SpinnerComponent],
  template: `
    <section class="page">
      <header class="page-header">
        <div>
          <h1>Padres de familia</h1>
          <p class="subtitle">Cuentas de consulta y los alumnos que cada una puede ver.</p>
        </div>
        <app-button variant="ghost" (click)="load()">Actualizar</app-button>
      </header>

      <section class="panel">
        <h2>Crear cuenta</h2>
        <form [formGroup]="form" (ngSubmit)="crear()" class="form-grid">
          <label>
            <span>Nombre</span>
            <input type="text" formControlName="nombre" placeholder="Juan Pérez" />
          </label>
          <label>
            <span>Correo</span>
            <input type="email" formControlName="email" placeholder="papa@ejemplo.com" />
          </label>
          <label>
            <span>Credencial inicial</span>
            <input type="text" formControlName="password" placeholder="Mínimo 8 caracteres" />
            <small>Se entrega al padre para su primer acceso.</small>
          </label>
          <div class="actions">
            <app-button [loading]="creando" [disabled]="form.invalid">Crear cuenta</app-button>
          </div>
        </form>
      </section>

      <div class="state" *ngIf="loading"><app-spinner variant="overlay" /></div>
      <div class="state error" *ngIf="!loading && errorMessage">{{ errorMessage }}</div>
      <div class="empty" *ngIf="!loading && !errorMessage && padres.length === 0">
        No hay cuentas de padres de familia registradas.
      </div>

      <section class="panel" *ngFor="let padre of padres">
        <div class="panel-header">
          <div>
            <strong>{{ padre.nombre }}</strong>
            <small>{{ padre.email }}</small>
          </div>
          <div class="panel-actions">
            <app-badge [variant]="padre.activo ? 'green' : 'gray'">
              {{ padre.activo ? 'Activa' : 'Desactivada' }}
            </app-badge>
            <button type="button" class="link" (click)="cambiarEstado(padre)">
              {{ padre.activo ? 'Desactivar' : 'Reactivar' }}
            </button>
          </div>
        </div>

        <div class="hijos">
          <span class="etiqueta">Alumnos vinculados</span>
          <p class="muted" *ngIf="padre.hijos.length === 0">
            Ninguno. Esta cuenta todavía no puede ver información.
          </p>
          <div class="chips">
            <span class="chip" *ngFor="let hijo of padre.hijos">
              {{ hijo.nombreCompleto }}
              <small>{{ hijo.numeroMatricula }}</small>
              <button type="button" (click)="desvincular(padre, hijo.id)" aria-label="Desvincular">×</button>
            </span>
          </div>
        </div>

        <div class="vincular">
          <label>
            <span>Vincular alumno</span>
            <select #selector>
              <option value="">Selecciona un alumno</option>
              <option *ngFor="let alumno of disponiblesPara(padre)" [value]="alumno.id">
                {{ alumno.nombre }} {{ alumno.apellido }} — {{ alumno.numeroMatricula }}
              </option>
            </select>
          </label>
          <app-button variant="ghost" (click)="vincular(padre, selector.value); selector.value = ''">
            Vincular
          </app-button>
        </div>
      </section>
    </section>
  `,
  styles: [`
    .page { display: grid; gap: var(--space-5); }

    .page-header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--space-4);
    }

    h1 { margin: 0; font-size: 1.5rem; }
    h2 { margin: 0; font-size: 1rem; }

    .subtitle, .muted {
      margin: 0;
      font-size: 0.85rem;
      color: var(--color-text-muted);
    }

    .panel {
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
    }

    .panel-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-3);
      flex-wrap: wrap;
    }

    .panel-header > div:first-child { display: grid; gap: 2px; }
    .panel-header small { font-size: 0.78rem; color: var(--color-text-muted); }
    .panel-actions { display: flex; align-items: center; gap: var(--space-3); }

    .form-grid,
    .vincular {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr)) auto;
      gap: var(--space-3);
      align-items: end;
    }

    .vincular { grid-template-columns: minmax(0, 1fr) auto; }

    label { display: grid; gap: var(--space-2); min-width: 0; }

    label span {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--color-text-muted);
    }

    input, select {
      width: 100%;
      padding: var(--space-2) var(--space-3);
      font: inherit;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
    }

    small { font-size: 0.72rem; color: var(--color-text-muted); }

    .etiqueta {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--color-text-muted);
    }

    .hijos { display: grid; gap: var(--space-2); }
    .chips { display: flex; flex-wrap: wrap; gap: var(--space-2); }

    .chip {
      display: inline-flex;
      align-items: center;
      gap: var(--space-2);
      padding: var(--space-1) var(--space-3);
      background: var(--color-bg-page);
      border: 1px solid var(--color-border);
      border-radius: 999px;
      font-size: 0.85rem;
    }

    .chip button,
    .link {
      padding: 0;
      border: 0;
      background: none;
      font: inherit;
      color: var(--color-danger);
      cursor: pointer;
    }

    .link { text-decoration: underline; font-size: 0.85rem; }

    .state, .empty {
      padding: var(--space-6);
      text-align: center;
      color: var(--color-text-muted);
    }

    .state.error { color: var(--color-danger); background: var(--color-danger-bg); border-radius: var(--radius-md); }

    @media (max-width: 860px) {
      .form-grid { grid-template-columns: minmax(0, 1fr); }
    }
  `]
})
export class PadresComponent implements OnInit {
  private readonly fb = inject(FormBuilder);

  padres: PadreFamilia[] = [];
  alumnos: Alumno[] = [];
  loading = true;
  creando = false;
  errorMessage = '';

  readonly form = this.fb.nonNullable.group({
    nombre: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    // El mismo mínimo que valida el backend, para avisar antes de enviar.
    password: ['', [Validators.required, Validators.minLength(8)]]
  });

  constructor(
    private service: PadresService,
    private alumnosService: AlumnosService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      padres: this.service.getAll(),
      alumnos: this.alumnosService.getAll()
    }).subscribe({
      next: resultado => {
        this.padres = resultado.padres;
        this.alumnos = resultado.alumnos;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = error?.error?.message ?? 'No se pudieron cargar las cuentas de padres.';
      }
    });
  }

  /** Oculta los que ya están vinculados: volver a agregarlos sería un error. */
  disponiblesPara(padre: PadreFamilia): Alumno[] {
    const vinculados = new Set(padre.hijos.map(hijo => hijo.id));
    return this.alumnos.filter(alumno => !vinculados.has(alumno.id));
  }

  crear(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.creando = true;
    this.service.crear(this.form.getRawValue()).subscribe({
      next: response => {
        this.creando = false;
        this.toast.success(response?.message ?? 'Cuenta creada.');
        this.form.reset({ nombre: '', email: '', password: '' });
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.creando = false;
        this.toast.error(error?.error?.message ?? 'No se pudo crear la cuenta.');
      }
    });
  }

  vincular(padre: PadreFamilia, alumnoId: string): void {
    if (!alumnoId) return;

    this.service.vincularHijo(padre.id, alumnoId).subscribe({
      next: response => {
        this.toast.success(response?.message ?? 'Alumno vinculado.');
        this.load();
      },
      error: (error: HttpErrorResponse) =>
        this.toast.error(error?.error?.message ?? 'No se pudo vincular el alumno.')
    });
  }

  desvincular(padre: PadreFamilia, alumnoId: string): void {
    const confirmado = window.confirm(
      `¿Quitar este alumno de la cuenta de ${padre.nombre}?\n\n` +
        'Dejará de ver su información de inmediato.'
    );
    if (!confirmado) return;

    this.service.desvincularHijo(padre.id, alumnoId).subscribe({
      next: response => {
        this.toast.success(response?.message ?? 'Alumno desvinculado.');
        this.load();
      },
      error: (error: HttpErrorResponse) =>
        this.toast.error(error?.error?.message ?? 'No se pudo desvincular el alumno.')
    });
  }

  cambiarEstado(padre: PadreFamilia): void {
    this.service.cambiarEstado(padre.id, !padre.activo).subscribe({
      next: response => {
        this.toast.success(response?.message ?? 'Cuenta actualizada.');
        this.load();
      },
      error: (error: HttpErrorResponse) =>
        this.toast.error(error?.error?.message ?? 'No se pudo actualizar la cuenta.')
    });
  }
}
