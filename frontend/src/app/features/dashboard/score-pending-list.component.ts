import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AlumnoPace, EstadoAlumnoPace } from '../../models';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge.component';

/** Minimal projection used when the full AlumnoPace list is available */
export interface ScorePendingItem {
  alumnoPaceId: string;
  alumnoId: string;
  nombreAlumno: string;
  materia: string;
  numeroPace: string;
  estado: EstadoAlumnoPace;
}

const ESTADO_LABELS: Partial<Record<EstadoAlumnoPace, string>> = {
  ListoParaAutoTest: 'Listo p/ AutoTest',
  AutoTestOk:        'AutoTest OK',
  AutoTestFallido:   'AutoTest Fallido',
  EnTestFinal:       'En Test Final',
};

const ESTADO_VARIANT: Partial<Record<EstadoAlumnoPace, BadgeVariant>> = {
  ListoParaAutoTest: 'blue',
  AutoTestOk:        'green',
  AutoTestFallido:   'red',
  EnTestFinal:       'orange',
};

/**
 * Panel de PACEs pendientes de Score Station.
 *
 * Modos de uso:
 *   1. Solo count: pasar `count` — muestra un resumen numérico con link al listado.
 *   2. Lista completa: pasar `items: ScorePendingItem[]` — muestra cada PACE con
 *      estado, materia y link al perfil del alumno.
 *
 * Cuando el backend exponga `GET /api/v1/Dashboard/paces-pendientes`,
 * mapear la respuesta a `ScorePendingItem[]` y pasar vía [items].
 */
@Component({
  selector: 'app-score-pending-list',
  standalone: true,
  imports: [CommonModule, RouterLink, BadgeComponent],
  template: `
    <section class="score-shell" aria-label="PACEs pendientes de Score Station">
      <header class="score-header">
        <h2>Score Station</h2>
        <app-badge [variant]="count > 0 ? 'orange' : 'green'">
          {{ count }} {{ count === 1 ? 'pendiente' : 'pendientes' }}
        </app-badge>
      </header>

      <!-- List mode: full items provided -->
      <ng-container *ngIf="hasItems; else countMode">
        <div class="score-empty" *ngIf="items.length === 0">
          No hay PACEs esperando revisión. ✅
        </div>

        <a
          class="pace-row"
          *ngFor="let item of visibleItems"
          [routerLink]="['/alumno', item.alumnoId]"
          [attr.aria-label]="item.nombreAlumno + ' — ' + item.materia + ' PACE ' + item.numeroPace"
        >
          <div class="pace-row__info">
            <strong>{{ item.nombreAlumno }}</strong>
            <span>{{ item.materia }} · PACE {{ item.numeroPace }}</span>
          </div>
          <app-badge [variant]="estadoVariant(item.estado)">
            {{ estadoLabel(item.estado) }}
          </app-badge>
        </a>

        <footer class="score-footer" *ngIf="items.length > maxVisible">
          <span>Mostrando {{ maxVisible }} de {{ items.length }}</span>
        </footer>
      </ng-container>

      <!-- Count mode: only the number is available -->
      <ng-template #countMode>
        <div class="score-count-mode" *ngIf="count === 0">
          <span class="count-icon">✅</span>
          <span>Sin PACEs en espera de revisión.</span>
        </div>

        <div class="score-count-mode score-count-mode--pending" *ngIf="count > 0">
          <span class="count-number">{{ count }}</span>
          <div>
            <p>PACEs esperan pasar por Score Station.</p>
            <p class="count-hint">Abre el perfil de cada alumno para revisar y validar.</p>
          </div>
        </div>
      </ng-template>
    </section>
  `,
  styles: [`
    :host { display: block; }

    .score-shell {
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-card);
      padding: var(--space-5);
      display: grid;
      gap: var(--space-4);
    }

    /* ── Header ─────────────────────────────── */
    .score-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
    }

    h2 {
      font-size: var(--font-size-xl);
      line-height: var(--line-height-tight);
    }

    /* ── Row (list mode) ─────────────────────── */
    .pace-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      padding: var(--space-3) var(--space-4);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      color: inherit;
      text-decoration: none;
      transition: background 120ms ease;
    }

    .pace-row:hover {
      background: var(--color-accent-light);
    }

    .pace-row__info {
      display: grid;
      gap: var(--space-1);
      min-width: 0;
    }

    .pace-row__info strong {
      font-size: var(--font-size-base);
      white-space: nowrap;
      overflow: hidden;
      text-overflow: ellipsis;
    }

    .pace-row__info span {
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
    }

    /* ── Count mode ──────────────────────────── */
    .score-count-mode {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      padding: var(--space-4);
      border-radius: var(--radius-sm);
      background: var(--color-bg-page);
    }

    .score-count-mode--pending {
      background: var(--color-warning-bg, #fff8e6);
      border: 1px solid var(--color-warning, #f59e0b);
    }

    .count-icon {
      font-size: var(--font-size-2xl);
      line-height: 1;
    }

    .count-number {
      font-size: var(--font-size-2xl);
      font-weight: var(--font-weight-bold);
      color: var(--color-warning);
      line-height: 1;
      flex-shrink: 0;
    }

    .score-count-mode p {
      margin: 0;
      font-size: var(--font-size-sm);
      color: var(--color-text-primary);
      line-height: var(--line-height-base);
    }

    .count-hint {
      color: var(--color-text-secondary) !important;
      margin-top: var(--space-1) !important;
    }

    /* ── Footer ─────────────────────────────── */
    .score-footer {
      font-size: var(--font-size-xs);
      color: var(--color-text-secondary);
      text-align: center;
    }

    /* ── Empty ───────────────────────────────── */
    .score-empty {
      padding: var(--space-4);
      text-align: center;
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
    }
  `]
})
export class ScorePendingListComponent implements OnChanges {
  /** Número total de PACEs pendientes (del resumen del dashboard). */
  @Input() count: number = 0;

  /**
   * Lista completa de PACEs pendientes.
   * Cuando se provea, activa el modo de lista detallada en lugar del modo conteo.
   */
  @Input() items: ScorePendingItem[] = [];

  /** Máximo de filas a mostrar sin paginar. */
  @Input() maxVisible: number = 6;

  hasItems = false;
  visibleItems: ScorePendingItem[] = [];

  ngOnChanges(): void {
    // Items mode activates only when the array is explicitly provided AND non-null
    this.hasItems = Array.isArray(this.items);
    this.visibleItems = this.items.slice(0, this.maxVisible);

    // Keep count in sync if items are provided
    if (this.hasItems && !this.count) {
      this.count = this.items.length;
    }
  }

  estadoLabel(estado: EstadoAlumnoPace): string {
    return ESTADO_LABELS[estado] ?? estado;
  }

  estadoVariant(estado: EstadoAlumnoPace): BadgeVariant {
    return ESTADO_VARIANT[estado] ?? 'gray';
  }

  /** Helper to map AlumnoPace[] (from getByAlumno) into ScorePendingItem[]. */
  static fromAlumnoPaces(paces: AlumnoPace[], nombreAlumno: string): ScorePendingItem[] {
    const pendingStates: EstadoAlumnoPace[] = [
      'ListoParaAutoTest', 'AutoTestOk', 'AutoTestFallido', 'EnTestFinal'
    ];
    return paces
      .filter(p => pendingStates.includes(p.estado))
      .map(p => ({
        alumnoPaceId: p.id,
        alumnoId: p.alumnoId,
        nombreAlumno,
        materia: p.materia ?? p.pace?.materia ?? '—',
        numeroPace: p.numeroPace ?? p.pace?.numeroPace ?? '',
        estado: p.estado
      }));
  }
}
