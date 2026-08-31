import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { EntrevistasService } from '../../core/services/entrevistas.service';
import { ToastService } from '../../core/services/toast.service';
import { EntrevistaPadre, NivelAlerta, nivelAlerta } from '../../models';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-entrevistas',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BadgeComponent, ButtonComponent, SpinnerComponent],
  template: `
    <section class="page-shell">
      <header class="topbar">
        <div>
          <p class="eyebrow">Módulo 1.5</p>
          <h1>Entrevistas a padres de familia</h1>
        </div>
        <app-button variant="ghost" (click)="load()">Actualizar</app-button>
      </header>

      <p class="intro">
        Registro de familias interesadas. Los factores de riesgo en el hogar son información
        sensible: se capturan para la seguridad del personal y solo son visibles para Principal y
        Supervisora.
      </p>

      <section class="panel">
        <h2>Registrar entrevista</h2>
        <form [formGroup]="form" (ngSubmit)="save()" class="form-grid">
          <label class="wide">
            <span>Nombre del padre o tutor</span>
            <input type="text" formControlName="nombrePadre" placeholder="Nombre completo" />
          </label>

          <label>
            <span>Número de hijos</span>
            <input type="number" formControlName="numeroHijos" min="0" />
          </label>

          <fieldset class="flags">
            <legend>Factores de riesgo en el hogar</legend>
            <label class="check">
              <input type="checkbox" formControlName="riesgoViolencia" />
              <span>Violencia familiar</span>
            </label>
            <label class="check">
              <input type="checkbox" formControlName="riesgoDivorcio" />
              <span>Divorcio o separación</span>
            </label>
            <label class="check">
              <input type="checkbox" formControlName="conoceADios" />
              <span>Conoce a Dios</span>
            </label>
          </fieldset>

          <label class="wide">
            <span>Comentarios críticos</span>
            <textarea formControlName="comentarios" rows="3" placeholder="Observaciones relevantes de la entrevista"></textarea>
          </label>

          <label class="check">
            <input type="checkbox" formControlName="aceptado" />
            <span>Familia aceptada</span>
          </label>

          <div class="actions">
            <app-button [loading]="saving" [disabled]="form.invalid">Registrar entrevista</app-button>
          </div>
        </form>
      </section>

      <div class="state" *ngIf="loading">
        <app-spinner variant="overlay" />
      </div>

      <div class="state error" *ngIf="!loading && errorMessage">
        <p>{{ errorMessage }}</p>
        <app-button (click)="load()">Reintentar</app-button>
      </div>

      <section class="panel" *ngIf="!loading && !errorMessage">
        <div class="section-header">
          <h2>Entrevistas registradas</h2>
          <app-badge variant="gray">{{ entrevistas.length }}</app-badge>
        </div>

        <div class="empty" *ngIf="entrevistas.length === 0">Aún no hay entrevistas registradas.</div>

        <article class="entrevista" *ngFor="let entrevista of entrevistas">
          <header>
            <div>
              <strong>{{ entrevista.nombrePadre }}</strong>
              <span class="meta">
                {{ entrevista.fechaEntrevista | date:'dd/MM/yyyy' }} ·
                {{ entrevista.numeroHijos }} {{ entrevista.numeroHijos === 1 ? 'hijo' : 'hijos' }}
              </span>
            </div>
            <div class="flags-row">
              <app-badge [variant]="alertaVariant(entrevista)">
                {{ alerta(entrevista) }}
              </app-badge>
              <app-badge [variant]="entrevista.aceptado ? 'green' : 'red'">
                {{ entrevista.aceptado ? 'Aceptada' : 'Rechazada' }}
              </app-badge>
            </div>
          </header>

          <button type="button" class="toggle" (click)="toggle(entrevista.id)">
            {{ expandido === entrevista.id ? 'Ocultar detalle' : 'Ver detalle' }}
          </button>

          <div class="detalle" *ngIf="expandido === entrevista.id">
            <ul class="riesgos">
              <li>Violencia familiar: <strong>{{ entrevista.riesgoViolencia ? 'Sí' : 'No' }}</strong></li>
              <li>Divorcio o separación: <strong>{{ entrevista.riesgoDivorcio ? 'Sí' : 'No' }}</strong></li>
              <li>Conoce a Dios: <strong>{{ entrevista.conoceADios ? 'Sí' : 'No' }}</strong></li>
            </ul>
            <p class="comentarios">{{ entrevista.comentarios || 'Sin comentarios registrados.' }}</p>
          </div>
        </article>
      </section>
    </section>
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

    .topbar h1,
    .panel h2 {
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
    }

    .section-header {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
      gap: var(--space-4);
      align-items: start;
    }

    .wide {
      grid-column: 1 / -1;
    }

    label {
      display: grid;
      gap: var(--space-2);
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
    }

    input[type="text"],
    input[type="number"],
    textarea {
      width: 100%;
      min-height: var(--tap-target-min, 44px);
      padding: var(--space-2) var(--space-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
      font: inherit;
    }

    textarea {
      resize: vertical;
    }

    fieldset.flags {
      display: grid;
      gap: var(--space-2);
      margin: 0;
      padding: var(--space-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
    }

    legend {
      padding: 0 var(--space-2);
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
    }

    label.check {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      min-height: var(--tap-target-min, 44px);
      color: var(--color-text-primary);
      font-weight: var(--font-weight-regular, 400);
    }

    label.check input {
      width: 20px;
      height: 20px;
    }

    .actions {
      grid-column: 1 / -1;
      display: flex;
      justify-content: flex-end;
    }

    .entrevista {
      display: grid;
      gap: var(--space-3);
      padding: var(--space-4);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
    }

    .entrevista header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: var(--space-3);
      flex-wrap: wrap;
    }

    .entrevista header > div:first-child {
      display: grid;
      gap: var(--space-1);
    }

    .meta {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
    }

    .flags-row {
      display: flex;
      gap: var(--space-2);
      flex-wrap: wrap;
    }

    .toggle {
      justify-self: start;
      min-height: var(--tap-target-min, 44px);
      padding: 0 var(--space-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
      font-weight: var(--font-weight-semibold);
      cursor: pointer;
    }

    .detalle {
      display: grid;
      gap: var(--space-3);
      padding: var(--space-3);
      border-radius: var(--radius-sm);
      background: var(--color-bg-app);
    }

    .riesgos {
      display: grid;
      gap: var(--space-1);
      margin: 0;
      padding-left: var(--space-5);
      color: var(--color-text-secondary);
    }

    .comentarios {
      margin: 0;
      white-space: pre-wrap;
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
  `]
})
export class EntrevistasComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(EntrevistasService);
  private readonly toast = inject(ToastService);

  entrevistas: EntrevistaPadre[] = [];
  loading = true;
  saving = false;
  errorMessage = '';
  expandido: string | null = null;

  readonly form = this.fb.nonNullable.group({
    nombrePadre: ['', [Validators.required, Validators.minLength(3)]],
    numeroHijos: [1, [Validators.required, Validators.min(0)]],
    riesgoViolencia: [false],
    riesgoDivorcio: [false],
    conoceADios: [false],
    comentarios: [''],
    aceptado: [false]
  });

  ngOnInit(): void {
    this.load();
  }

  alerta(entrevista: EntrevistaPadre): NivelAlerta {
    return nivelAlerta(entrevista);
  }

  alertaVariant(entrevista: EntrevistaPadre): BadgeVariant {
    const nivel = nivelAlerta(entrevista);
    if (nivel === 'Alta') return 'red';
    return nivel === 'Media' ? 'orange' : 'gray';
  }

  toggle(id: string): void {
    this.expandido = this.expandido === id ? null : id;
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    this.service.getAll().subscribe({
      next: entrevistas => {
        this.entrevistas = entrevistas;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = error?.error?.message ?? 'No se pudieron cargar las entrevistas.';
      }
    });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.service.create(this.form.getRawValue()).subscribe({
      next: response => {
        this.saving = false;
        this.form.reset({
          nombrePadre: '',
          numeroHijos: 1,
          riesgoViolencia: false,
          riesgoDivorcio: false,
          conoceADios: false,
          comentarios: '',
          aceptado: false
        });
        this.toast.success(response?.message ?? 'Entrevista registrada.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving = false;
        this.toast.error(error?.error?.message ?? 'No se pudo registrar la entrevista.');
      }
    });
  }
}
