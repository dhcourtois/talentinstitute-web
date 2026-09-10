import { CommonModule } from '@angular/common';
import { Component, Input, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { AnotacionesService } from '../../core/services/anotaciones.service';
import { ToastService } from '../../core/services/toast.service';
import { todayIso } from '../../core/utils/fecha.util';
import { Anotacion } from '../../models';
import { BadgeComponent } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';

/**
 * Observaciones semanales del alumno (issue #20).
 *
 * Vive en su propio componente y no dentro de `AlumnoComponent` porque esa
 * pantalla ya concentra PACEs, metas y méritos; sumarle una cuarta sección
 * habría empujado su CSS contra el presupuesto de bundle.
 */
@Component({
  selector: 'app-anotaciones-panel',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, BadgeComponent, ButtonComponent],
  template: `
    <section class="panel">
      <div class="section-header">
        <h2>Anotaciones de la semana</h2>
        <app-badge variant="gray">{{ anotaciones.length }}</app-badge>
      </div>

      <form [formGroup]="form" (ngSubmit)="registrar()" class="anotacion-form">
        <label class="wide">
          <span>Observación</span>
          <textarea
            formControlName="texto"
            rows="3"
            [attr.maxlength]="largoMaximo"
            placeholder="Qué observaste esta semana del alumno"
          ></textarea>
          <small>{{ restantes }} caracteres disponibles</small>
        </label>
        <label>
          <span>Fecha</span>
          <input type="date" formControlName="fecha" />
          <small>Se archiva en la semana de esta fecha.</small>
        </label>
        <app-button [loading]="guardando" [disabled]="form.invalid">Agregar</app-button>
      </form>

      <div class="section-error" *ngIf="errorMessage">{{ errorMessage }}</div>
      <div class="empty" *ngIf="!errorMessage && !cargando && anotaciones.length === 0">
        No hay observaciones registradas para este alumno.
      </div>

      <div class="semana" *ngFor="let semana of porSemana">
        <div class="week-divider">
          <span class="week-label">{{ semana.etiqueta }}</span>
          <app-badge variant="gray">{{ semana.registros.length }}</app-badge>
        </div>

        <article class="anotacion" *ngFor="let anotacion of semana.registros">
          <p>{{ anotacion.texto }}</p>
          <small>
            {{ anotacion.fechaCreacion | date:'dd/MM/yyyy HH:mm' }} ·
            {{ anotacion.staffName ?? 'Personal Escolar' }}
          </small>
        </article>
      </div>
    </section>
  `,
  styles: [`
    .panel {
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
    }

    .section-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-3);
    }

    h2 {
      margin: 0;
      font-size: 1.05rem;
    }

    .anotacion-form {
      display: grid;
      grid-template-columns: minmax(0, 2fr) minmax(0, 1fr) auto;
      gap: var(--space-3);
      align-items: end;
      padding-bottom: var(--space-4);
      border-bottom: 1px solid var(--color-border);
    }

    label {
      display: grid;
      gap: var(--space-2);
      min-width: 0;
    }

    textarea,
    input {
      width: 100%;
      padding: var(--space-2) var(--space-3);
      font: inherit;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      resize: vertical;
    }

    label span {
      font-size: 0.8rem;
      font-weight: 600;
      color: var(--color-text-muted);
    }

    small {
      font-size: 0.72rem;
      color: var(--color-text-muted);
    }

    .semana {
      display: grid;
      gap: var(--space-3);
    }

    .week-divider {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-3);
      padding-bottom: var(--space-2);
      border-bottom: 2px solid var(--color-border);
    }

    .week-label {
      font-size: 0.8rem;
      font-weight: 600;
      letter-spacing: 0.04em;
      text-transform: uppercase;
      color: var(--color-text-muted);
    }

    .anotacion {
      display: grid;
      gap: var(--space-1);
      padding: var(--space-4);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
    }

    .anotacion p {
      margin: 0;
      white-space: pre-wrap;
    }

    .empty,
    .section-error {
      padding: var(--space-4);
      font-size: 0.85rem;
      color: var(--color-text-muted);
      text-align: center;
    }

    .section-error {
      color: var(--color-danger);
      background: var(--color-danger-bg);
      border-radius: var(--radius-sm);
    }

    @media (max-width: 720px) {
      .anotacion-form {
        grid-template-columns: minmax(0, 1fr);
      }
    }
  `]
})
export class AnotacionesPanelComponent implements OnInit {
  private readonly fb = inject(FormBuilder);

  @Input({ required: true }) alumnoId = '';

  readonly largoMaximo = 2000;

  anotaciones: Anotacion[] = [];
  porSemana: SemanaDeAnotaciones[] = [];
  cargando = true;
  guardando = false;
  errorMessage = '';

  readonly form = this.fb.nonNullable.group({
    texto: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(2000)]],
    fecha: [todayIso(), Validators.required]
  });

  constructor(
    private service: AnotacionesService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.cargar();
  }

  get restantes(): number {
    return this.largoMaximo - (this.form.controls.texto.value?.length ?? 0);
  }

  cargar(): void {
    if (!this.alumnoId) {
      this.cargando = false;
      return;
    }

    this.cargando = true;
    this.errorMessage = '';

    this.service.getByAlumno(this.alumnoId).subscribe({
      next: anotaciones => {
        this.anotaciones = anotaciones;
        this.porSemana = this.agrupar(anotaciones);
        this.cargando = false;
      },
      error: (error: HttpErrorResponse) => {
        this.cargando = false;
        this.errorMessage = error?.error?.message ?? 'No se pudieron cargar las anotaciones.';
      }
    });
  }

  registrar(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.guardando = true;
    const { texto, fecha } = this.form.getRawValue();

    this.service.registrar({ alumnoId: this.alumnoId, texto, fecha }).subscribe({
      next: () => {
        this.guardando = false;
        this.toast.success('Anotación registrada.');
        // La fecha se conserva: quien captura varias observaciones de la misma
        // semana no debería volver a elegirla en cada una.
        this.form.patchValue({ texto: '' });
        this.form.controls.texto.markAsUntouched();
        this.cargar();
      },
      error: (error: HttpErrorResponse) => {
        this.guardando = false;
        this.toast.error(error?.error?.message ?? 'No se pudo registrar la anotación.');
      }
    });
  }

  /**
   * El backend ya devuelve las anotaciones ordenadas por semana; aquí solo se
   * parten en bloques para que el historial se lea semana por semana, que es
   * lo que pide el issue.
   */
  private agrupar(anotaciones: Anotacion[]): SemanaDeAnotaciones[] {
    const grupos = new Map<string, SemanaDeAnotaciones>();

    for (const anotacion of anotaciones) {
      const inicio = anotacion.semanaInicio.slice(0, 10);
      let grupo = grupos.get(inicio);

      if (!grupo) {
        grupo = { inicio, etiqueta: this.etiquetaSemana(inicio), registros: [] };
        grupos.set(inicio, grupo);
      }

      grupo.registros.push(anotacion);
    }

    return [...grupos.values()].sort((a, b) => b.inicio.localeCompare(a.inicio));
  }

  private etiquetaSemana(inicioIso: string): string {
    const [anio, mes, dia] = inicioIso.split('-').map(Number);
    const inicio = new Date(anio, mes - 1, dia);
    const fin = new Date(anio, mes - 1, dia + 6);

    const corto = (fecha: Date) =>
      `${`${fecha.getDate()}`.padStart(2, '0')}/${`${fecha.getMonth() + 1}`.padStart(2, '0')}`;

    return `Semana · ${corto(inicio)} – ${corto(fin)}`;
  }
}

interface SemanaDeAnotaciones {
  /** Lunes de la semana, en formato `yyyy-MM-dd`. Ordena los grupos. */
  inicio: string;
  etiqueta: string;
  registros: Anotacion[];
}
