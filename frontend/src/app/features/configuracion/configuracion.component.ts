import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ConfiguracionService } from '../../core/services/configuracion.service';
import { ToastService } from '../../core/services/toast.service';
import { ActualizarConfiguracionRequest } from '../../models';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

interface Privilegio {
  nombre: string;
  otorga: 'umbralOficina' | 'umbralComedor' | 'umbralPatio' | 'umbralBiblioteca' | 'umbralActividades';
  revoca:
    | 'umbralOficinaRevocado'
    | 'umbralComedorRevocado'
    | 'umbralPatioRevocado'
    | 'umbralBibliotecaRevocado'
    | 'umbralActividadesRevocado';
}

@Component({
  selector: 'app-configuracion',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent, SpinnerComponent],
  template: `
    <section class="page-shell">
      <header class="topbar">
        <div>
          <p class="eyebrow">Solo Principal</p>
          <h1>Umbrales de privilegios</h1>
        </div>
      </header>

      <p class="intro">
        Definen a partir de qué balance de méritos se otorga cada privilegio y con qué balance se
        revoca. El umbral de revocación siempre debe ser menor que el de otorgamiento.
      </p>

      <div class="state" *ngIf="loading">
        <app-spinner variant="overlay" />
      </div>

      <div class="state error" *ngIf="!loading && errorMessage">
        <p>{{ errorMessage }}</p>
        <app-button (click)="load()">Reintentar</app-button>
      </div>

      <form *ngIf="!loading && !errorMessage" [formGroup]="form" (ngSubmit)="save()" class="panel">
        <table>
          <caption class="sr-only">Umbrales por privilegio</caption>
          <thead>
            <tr>
              <th scope="col">Privilegio</th>
              <th scope="col">Se otorga desde</th>
              <th scope="col">Se revoca en</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let privilegio of privilegios">
              <th scope="row">{{ privilegio.nombre }}</th>
              <td>
                <input
                  type="number"
                  [formControlName]="privilegio.otorga"
                  [attr.aria-label]="privilegio.nombre + ': se otorga desde'"
                />
              </td>
              <td>
                <input
                  type="number"
                  [formControlName]="privilegio.revoca"
                  [attr.aria-label]="privilegio.nombre + ': se revoca en'"
                />
              </td>
            </tr>
          </tbody>
        </table>

        <p class="field-error" *ngIf="invalidos.length">
          El umbral de revocación debe ser menor que el de otorgamiento en: {{ invalidos.join(', ') }}.
        </p>

        <div class="actions">
          <app-button [loading]="saving" [disabled]="form.invalid || invalidos.length > 0">
            Guardar umbrales
          </app-button>
        </div>
      </form>
    </section>
  `,
  styles: [`
    .page-shell {
      display: grid;
      gap: var(--space-5);
      padding: var(--space-6);
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

    .intro {
      max-width: 68ch;
      margin: 0;
      color: var(--color-text-secondary);
    }

    .panel {
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      background: var(--color-bg-surface);
      overflow-x: auto;
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

    input {
      width: 96px;
      min-height: var(--tap-target-min, 44px);
      padding: 0 var(--space-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
    }

    .actions {
      display: flex;
      justify-content: flex-end;
    }

    .field-error {
      margin: 0;
      color: var(--color-danger);
    }

    .state {
      display: grid;
      place-items: center;
      min-height: 160px;
      padding: var(--space-6);
      border: 1px dashed var(--color-border);
      border-radius: var(--radius-md);
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
export class ConfiguracionComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(ConfiguracionService);
  private readonly toast = inject(ToastService);

  loading = true;
  saving = false;
  errorMessage = '';

  readonly privilegios: Privilegio[] = [
    { nombre: 'Oficina', otorga: 'umbralOficina', revoca: 'umbralOficinaRevocado' },
    { nombre: 'Comedor', otorga: 'umbralComedor', revoca: 'umbralComedorRevocado' },
    { nombre: 'Patio', otorga: 'umbralPatio', revoca: 'umbralPatioRevocado' },
    { nombre: 'Biblioteca', otorga: 'umbralBiblioteca', revoca: 'umbralBibliotecaRevocado' },
    { nombre: 'Actividades', otorga: 'umbralActividades', revoca: 'umbralActividadesRevocado' }
  ];

  readonly form = this.fb.nonNullable.group({
    umbralOficina: [0, Validators.required],
    umbralOficinaRevocado: [-2, Validators.required],
    umbralComedor: [0, Validators.required],
    umbralComedorRevocado: [-3, Validators.required],
    umbralPatio: [-1, Validators.required],
    umbralPatioRevocado: [-5, Validators.required],
    umbralBiblioteca: [3, Validators.required],
    umbralBibliotecaRevocado: [0, Validators.required],
    umbralActividades: [5, Validators.required],
    umbralActividadesRevocado: [2, Validators.required]
  });

  ngOnInit(): void {
    this.load();
  }

  /** Misma invariante que valida el dominio, adelantada al formulario. */
  get invalidos(): string[] {
    const valores = this.form.getRawValue();
    return this.privilegios
      .filter(privilegio => Number(valores[privilegio.revoca]) >= Number(valores[privilegio.otorga]))
      .map(privilegio => privilegio.nombre);
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    this.service.getPrivilegios().subscribe({
      next: config => {
        this.form.patchValue(config);
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = error?.error?.message ?? 'No se pudo cargar la configuración de privilegios.';
      }
    });
  }

  save(): void {
    if (this.form.invalid || this.invalidos.length) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.service.updatePrivilegios(this.form.getRawValue() as ActualizarConfiguracionRequest).subscribe({
      next: response => {
        this.saving = false;
        this.toast.success(response?.message ?? 'Umbrales actualizados.');
      },
      error: (error: HttpErrorResponse) => {
        this.saving = false;
        this.toast.error(error?.error?.message ?? 'No se pudieron actualizar los umbrales.');
      }
    });
  }
}
