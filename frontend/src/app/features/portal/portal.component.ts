import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { PortalService } from '../../core/services/portal.service';
import { weekStartIso } from '../../core/utils/fecha.util';
import { Anotacion, AlumnoPace, Hijo, Merito, Meta } from '../../models';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

/**
 * Portal de consulta para padres de familia (issue #8).
 *
 * Toda la pantalla es de lectura: no hay un solo control que escriba. El padre
 * ve las metas de la semana y la bitácora de méritos de sus hijos, que es lo
 * que pide el issue, y nada de la operación interna del colegio.
 */
@Component({
  selector: 'app-portal',
  standalone: true,
  imports: [CommonModule, BadgeComponent, ButtonComponent, SpinnerComponent],
  template: `
    <section class="page">
      <header class="page-header">
        <div>
          <h1>Mis hijos</h1>
          <p class="subtitle">Consulta del avance semanal y del comportamiento.</p>
        </div>
        <app-button variant="ghost" (click)="reload()">Actualizar</app-button>
      </header>

      <div class="state" *ngIf="loading">
        <app-spinner variant="overlay" />
      </div>

      <div class="state error" *ngIf="!loading && errorMessage">
        <p>{{ errorMessage }}</p>
        <app-button (click)="reload()">Reintentar</app-button>
      </div>

      <div class="empty" *ngIf="!loading && !errorMessage && hijos.length === 0">
        Todavía no hay alumnos vinculados a tu cuenta. Comunícate con el colegio.
      </div>

      <nav class="tabs" *ngIf="hijos.length > 1">
        <button
          type="button"
          *ngFor="let hijo of hijos"
          [class.active]="hijo.id === seleccionadoId"
          (click)="seleccionar(hijo.id)"
        >
          {{ hijo.nombre }}
        </button>
      </nav>

      <ng-container *ngIf="seleccionado as hijo">
        <section class="hero">
          <div class="avatar" aria-hidden="true">{{ iniciales(hijo) }}</div>
          <div>
            <p class="eyebrow">{{ hijo.numeroMatricula }}</p>
            <h2>{{ hijo.nombre }} {{ hijo.apellido }}</h2>
            <div class="hero-meta">
              <app-badge variant="gray">{{ hijo.nivel }}</app-badge>
              <app-badge [variant]="balanceVariant(hijo.balanceMeritos)">
                Balance {{ hijo.balanceMeritos }}
              </app-badge>
            </div>
          </div>
        </section>

        <section class="panel">
          <h3>Privilegios activos</h3>
          <div class="chips" *ngIf="hijo.privilegiosActivos.length > 0; else sinPrivilegios">
            <app-badge variant="green" *ngFor="let privilegio of hijo.privilegiosActivos">
              {{ privilegio }}
            </app-badge>
          </div>
          <ng-template #sinPrivilegios>
            <p class="muted">Sin privilegios activos esta semana.</p>
          </ng-template>
        </section>

        <section class="panel">
          <div class="panel-header">
            <h3>PACEs</h3>
            <app-badge variant="gray">{{ pacesEnCurso.length }} en curso</app-badge>
          </div>

          <p class="muted" *ngIf="detalleCargando">Cargando…</p>
          <p class="muted" *ngIf="!detalleCargando && paces.length === 0">
            Todavía no hay PACEs asignados.
          </p>

          <ng-container *ngIf="pacesEnCurso.length > 0">
            <span class="etiqueta">En curso</span>
            <article class="fila" *ngFor="let pace of pacesEnCurso">
              <div>
                <strong>{{ pace.materia }} {{ pace.numeroPace }}</strong>
                <small>Desde el {{ pace.fechaInicio | date:'dd/MM/yyyy' }}</small>
              </div>
              <app-badge variant="blue">{{ etiquetaEstado(pace.estado) }}</app-badge>
            </article>
          </ng-container>

          <ng-container *ngIf="pacesCerrados.length > 0">
            <span class="etiqueta">Historial</span>
            <article class="fila" *ngFor="let pace of pacesCerrados">
              <div>
                <strong>{{ pace.materia }} {{ pace.numeroPace }}</strong>
                <small>
                  {{ pace.fechaCompletado | date:'dd/MM/yyyy' }}
                  <ng-container *ngIf="pace.puntajeFinal != null"> · {{ pace.puntajeFinal }} puntos</ng-container>
                </small>
              </div>
              <app-badge [variant]="pace.estado === 'Completado' ? 'green' : 'red'">
                {{ pace.estado }}
              </app-badge>
            </article>
          </ng-container>
        </section>

        <div class="grid">
          <section class="panel">
            <div class="panel-header">
              <h3>Metas de la semana</h3>
              <app-badge variant="gray">{{ metas.length }}</app-badge>
            </div>

            <p class="muted" *ngIf="detalleCargando">Cargando…</p>
            <p class="muted" *ngIf="!detalleCargando && metas.length === 0">
              No hay metas registradas esta semana.
            </p>

            <article class="fila" *ngFor="let meta of metas">
              <div>
                <strong>{{ meta.turno }} · {{ rangoDePaginas(meta) }}</strong>
                <small>
                  {{ meta.fechaObjetivo | date:'dd/MM/yyyy' }} ·
                  {{ meta.materia ?? 'PACE' }} {{ meta.numeroPace ?? '' }}
                </small>
              </div>
              <app-badge [variant]="metaVariant(meta.estado)">{{ meta.estado }}</app-badge>
            </article>
          </section>

          <section class="panel">
            <div class="panel-header">
              <h3>Comentarios del colegio</h3>
              <app-badge variant="gray">{{ anotaciones.length }}</app-badge>
            </div>

            <p class="muted" *ngIf="detalleCargando">Cargando…</p>
            <p class="muted" *ngIf="!detalleCargando && anotaciones.length === 0">
              Todavía no hay comentarios registrados.
            </p>

            <div class="semana" *ngFor="let semana of anotacionesPorSemana">
              <span class="etiqueta">{{ semana.etiqueta }}</span>
              <article class="fila" *ngFor="let anotacion of semana.registros">
                <div>
                  <p class="texto">{{ anotacion.texto }}</p>
                  <small>{{ anotacion.fechaCreacion | date:'dd/MM/yyyy' }}</small>
                </div>
              </article>
            </div>
          </section>

          <section class="panel">
            <div class="panel-header">
              <h3>Méritos y deméritos</h3>
              <app-badge variant="gray">{{ meritosVisibles.length }}</app-badge>
            </div>

            <p class="muted" *ngIf="detalleCargando">Cargando…</p>
            <p class="muted" *ngIf="!detalleCargando && meritosVisibles.length === 0">
              No hay registros de comportamiento.
            </p>

            <article class="fila" *ngFor="let merito of meritosVisibles">
              <div>
                <strong>{{ merito.tipo }} · {{ merito.puntos }} puntos</strong>
                <small>{{ merito.motivo }}</small>
                <small>{{ merito.fechaAplicado | date:'dd/MM/yyyy' }}</small>
              </div>
              <app-badge [variant]="merito.tipo === 'Merito' ? 'green' : 'red'">
                {{ merito.tipo }}
              </app-badge>
            </article>
          </section>
        </div>
      </ng-container>
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
    h2 { margin: 0; font-size: 1.25rem; }
    h3 { margin: 0; font-size: 1rem; }

    .subtitle, .muted {
      margin: 0;
      font-size: 0.85rem;
      color: var(--color-text-muted);
    }

    .tabs { display: flex; flex-wrap: wrap; gap: var(--space-2); }

    .tabs button {
      padding: var(--space-2) var(--space-4);
      font: inherit;
      color: var(--color-text-muted);
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      cursor: pointer;
    }

    .tabs button.active {
      color: var(--color-text);
      border-color: var(--color-text);
      font-weight: 600;
    }

    .hero {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      padding: var(--space-5);
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
    }

    .avatar {
      display: grid;
      place-items: center;
      width: 56px;
      height: 56px;
      font-weight: 700;
      border-radius: 50%;
      background: var(--color-bg-page);
      border: 1px solid var(--color-border);
    }

    .eyebrow {
      margin: 0;
      font-size: 0.72rem;
      letter-spacing: 0.06em;
      text-transform: uppercase;
      color: var(--color-text-muted);
    }

    .hero-meta { display: flex; gap: var(--space-2); margin-top: var(--space-2); }

    .grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
      gap: var(--space-5);
      align-items: start;
    }

    .panel {
      display: grid;
      gap: var(--space-3);
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
    }

    .chips { display: flex; flex-wrap: wrap; gap: var(--space-2); }

    .etiqueta {
      font-size: 0.75rem;
      font-weight: 600;
      letter-spacing: 0.04em;
      text-transform: uppercase;
      color: var(--color-text-muted);
    }

    .fila {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-3);
      padding: var(--space-4);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
    }

    .fila > div { display: grid; gap: 2px; min-width: 0; }
    .semana { display: grid; gap: var(--space-2); }
    .texto { margin: 0; white-space: pre-wrap; }
    .fila small { font-size: 0.75rem; color: var(--color-text-muted); }

    .state, .empty {
      padding: var(--space-6);
      text-align: center;
      color: var(--color-text-muted);
    }

    .state.error {
      display: grid;
      gap: var(--space-3);
      justify-items: center;
      color: var(--color-danger);
      background: var(--color-danger-bg);
      border-radius: var(--radius-md);
    }
  `]
})
export class PortalComponent implements OnInit {
  hijos: Hijo[] = [];
  seleccionadoId = '';
  metas: Meta[] = [];
  meritos: Merito[] = [];
  paces: AlumnoPace[] = [];
  anotaciones: Anotacion[] = [];
  loading = true;
  detalleCargando = false;
  errorMessage = '';

  constructor(private portal: PortalService) {}

  ngOnInit(): void {
    this.reload();
  }

  get seleccionado(): Hijo | null {
    return this.hijos.find(hijo => hijo.id === this.seleccionadoId) ?? null;
  }

  /** Un registro eliminado por el colegio no debe seguir pesando en casa. */
  get meritosVisibles(): Merito[] {
    return this.meritos.filter(merito => !merito.revocado);
  }

  get pacesEnCurso(): AlumnoPace[] {
    return this.paces.filter(pace => pace.estado !== 'Completado' && pace.estado !== 'Fallido');
  }

  /** Los ya cerrados, del más reciente al más antiguo. */
  get pacesCerrados(): AlumnoPace[] {
    return this.paces
      .filter(pace => pace.estado === 'Completado' || pace.estado === 'Fallido')
      .sort((a, b) => (b.fechaCompletado ?? '').localeCompare(a.fechaCompletado ?? ''));
  }

  /** Las observaciones partidas por semana, igual que las ve el colegio. */
  get anotacionesPorSemana(): Array<{ inicio: string; etiqueta: string; registros: Anotacion[] }> {
    const grupos = new Map<string, { inicio: string; etiqueta: string; registros: Anotacion[] }>();

    for (const anotacion of this.anotaciones) {
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
    const corto = (f: Date) =>
      `${`${f.getDate()}`.padStart(2, '0')}/${`${f.getMonth() + 1}`.padStart(2, '0')}`;
    return `Semana · ${corto(inicio)} – ${corto(fin)}`;
  }

  /** La nomenclatura del SOW, no el identificador interno. */
  etiquetaEstado(estado: string): string {
    const etiquetas: Record<string, string> = {
      ListoParaAutoTest: 'Listo para Score Station',
      AutoTestOk: 'Auto-test aprobado',
      AutoTestFallido: 'Auto-test fallido',
      EnTestFinal: 'En test final',
      EnProgreso: 'En progreso'
    };
    return etiquetas[estado] ?? estado;
  }

  reload(): void {
    this.loading = true;
    this.errorMessage = '';

    this.portal.getHijos().subscribe({
      next: hijos => {
        this.hijos = hijos;
        this.loading = false;

        if (hijos.length > 0) {
          const sigueDisponible = hijos.some(hijo => hijo.id === this.seleccionadoId);
          this.seleccionar(sigueDisponible ? this.seleccionadoId : hijos[0].id);
        }
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = error?.error?.message ?? 'No se pudo cargar la información de tus hijos.';
      }
    });
  }

  seleccionar(alumnoId: string): void {
    this.seleccionadoId = alumnoId;
    this.detalleCargando = true;

    // Cada sección se degrada por separado: que falle una no debe dejar la
    // pantalla del padre en blanco.
    forkJoin({
      metas: this.portal.getMetas(alumnoId, weekStartIso()).pipe(catchError(() => of([] as Meta[]))),
      meritos: this.portal.getMeritos(alumnoId).pipe(catchError(() => of([] as Merito[]))),
      paces: this.portal.getPaces(alumnoId).pipe(catchError(() => of([] as AlumnoPace[]))),
      anotaciones: this.portal.getAnotaciones(alumnoId).pipe(catchError(() => of([] as Anotacion[])))
    }).subscribe(resultado => {
      this.metas = resultado.metas;
      this.meritos = resultado.meritos;
      this.paces = resultado.paces;
      this.anotaciones = resultado.anotaciones;
      this.detalleCargando = false;
    });
  }

  rangoDePaginas(meta: Meta): string {
    if (meta.paginaInicial == null || meta.paginaFinal == null) {
      return `${meta.paginasObjetivo} páginas`;
    }

    return meta.paginaInicial === meta.paginaFinal
      ? `Página ${meta.paginaInicial}`
      : `Páginas ${meta.paginaInicial} a ${meta.paginaFinal}`;
  }

  iniciales(hijo: Hijo): string {
    return `${hijo.nombre.charAt(0)}${hijo.apellido.charAt(0)}`.toUpperCase();
  }

  balanceVariant(balance: number): BadgeVariant {
    if (balance < 0) return 'red';
    if (balance === 0) return 'gray';
    return 'green';
  }

  metaVariant(estado: string): BadgeVariant {
    if (estado === 'Aprobada' || estado === 'Completada') return 'green';
    if (estado === 'Rechazada') return 'red';
    if (estado === 'Scored') return 'blue';
    return 'orange';
  }
}
