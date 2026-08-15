import { CommonModule } from '@angular/common';
import { Component, Input, OnChanges } from '@angular/core';
import { MetasDia } from '../../models';

interface BarDatum {
  label: string;        // "Lun", "Mar", etc.
  fecha: string;        // ISO date
  completadas: number;
  total: number;
  heightPct: number;    // 0–100 para el alto de la barra completada
  totalPct: number;     // 0–100 para el alto de la barra de fondo
  isToday: boolean;
}

/**
 * Gráfica de barras semanal en CSS puro (sin librería de charts).
 * Recibe `datos: MetasDia[]` — normalmente los últimos 7 días del resumen.
 * Si no hay datos, genera 7 días de placeholder vacíos.
 */
@Component({
  selector: 'app-weekly-chart',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="chart-shell" aria-label="Gráfica de metas por día">
      <header class="chart-header">
        <h2>Metas de la semana</h2>
        <span class="chart-legend">
          <span class="legend-dot legend-dot--completed"></span> Completadas
          <span class="legend-dot legend-dot--total"></span> Total
        </span>
      </header>

      <div class="chart-area" role="img" [attr.aria-label]="ariaDescription">
        <div class="y-axis">
          <span>{{ maxTotal }}</span>
          <span>{{ halfTotal }}</span>
          <span>0</span>
        </div>

        <div class="bars-track">
          <div
            class="bar-col"
            *ngFor="let d of bars"
            [class.bar-col--today]="d.isToday"
            [attr.aria-label]="d.label + ': ' + d.completadas + ' de ' + d.total"
          >
            <!-- Fondo: total del día -->
            <div class="bar bar--total" [style.height.%]="d.totalPct">
              <!-- Relleno: completadas -->
              <div class="bar bar--completed" [style.height.%]="barInnerPct(d)"></div>
            </div>

            <span class="bar-value" *ngIf="d.completadas > 0">{{ d.completadas }}</span>
            <span class="bar-label" [class.bar-label--today]="d.isToday">{{ d.label }}</span>
          </div>
        </div>
      </div>

      <div class="chart-empty" *ngIf="isEmpty">
        Sin datos de metas para esta semana.
      </div>
    </section>
  `,
  styles: [`
    :host { display: block; }

    .chart-shell {
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-card);
      padding: var(--space-5);
      display: grid;
      gap: var(--space-4);
    }

    /* ── Header ─────────────────────────────── */
    .chart-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
      flex-wrap: wrap;
    }

    h2 {
      font-size: var(--font-size-xl);
      line-height: var(--line-height-tight);
    }

    .chart-legend {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      font-size: var(--font-size-sm);
      color: var(--color-text-secondary);
    }

    .legend-dot {
      display: inline-block;
      width: 10px;
      height: 10px;
      border-radius: 2px;
    }

    .legend-dot--completed { background: var(--color-accent); }
    .legend-dot--total     { background: var(--color-accent-light); border: 1px solid var(--color-border); }

    /* ── Chart area ──────────────────────────── */
    .chart-area {
      display: flex;
      align-items: stretch;
      gap: var(--space-3);
      height: 160px;
    }

    /* Y axis labels */
    .y-axis {
      display: flex;
      flex-direction: column;
      justify-content: space-between;
      align-items: flex-end;
      padding-bottom: 24px; /* align with bar-label space */
      font-size: var(--font-size-xs);
      color: var(--color-text-secondary);
      min-width: 24px;
      flex-shrink: 0;
    }

    /* Bar track */
    .bars-track {
      flex: 1;
      display: flex;
      align-items: flex-end;
      gap: var(--space-2);
    }

    /* Each column */
    .bar-col {
      flex: 1;
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: flex-end;
      gap: var(--space-1);
      height: 100%;
      position: relative;
    }

    /* The total-height background bar */
    .bar--total {
      width: 100%;
      max-width: 40px;
      background: var(--color-accent-light);
      border-radius: var(--radius-sm) var(--radius-sm) 0 0;
      position: relative;
      min-height: 4px;
      transition: height 400ms cubic-bezier(.4,0,.2,1);
    }

    /* Completed fill — sits inside total bar, pinned to bottom */
    .bar--completed {
      position: absolute;
      bottom: 0;
      left: 0;
      right: 0;
      background: var(--color-accent);
      border-radius: var(--radius-sm) var(--radius-sm) 0 0;
      min-height: 4px;
      transition: height 400ms cubic-bezier(.4,0,.2,1) 80ms;
    }

    .bar-value {
      font-size: var(--font-size-xs);
      font-weight: var(--font-weight-semibold);
      color: var(--color-accent);
      line-height: 1;
    }

    .bar-label {
      font-size: var(--font-size-xs);
      color: var(--color-text-secondary);
      line-height: 1;
      margin-top: var(--space-1);
    }

    .bar-label--today {
      color: var(--color-accent);
      font-weight: var(--font-weight-semibold);
    }

    /* Today highlight */
    .bar-col--today .bar--total {
      outline: 2px solid var(--color-accent);
      outline-offset: 2px;
      border-radius: var(--radius-sm);
    }

    /* Empty state */
    .chart-empty {
      display: grid;
      place-items: center;
      min-height: 80px;
      border: 1px dashed var(--color-border);
      border-radius: var(--radius-sm);
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
    }

    @media (max-width: 520px) {
      .chart-area { height: 120px; }
      .bar--total  { max-width: 28px; }
    }
  `]
})
export class WeeklyChartComponent implements OnChanges {
  @Input() datos: MetasDia[] = [];

  bars: BarDatum[] = [];
  maxTotal = 0;
  halfTotal = 0;
  isEmpty = false;

  private readonly DAY_LABELS = ['Dom', 'Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb'];

  ngOnChanges(): void {
    this.buildBars();
  }

  private buildBars(): void {
    const todayStr = new Date().toISOString().slice(0, 10);

    // Use provided data or generate 7-day empty placeholders
    const raw: MetasDia[] = this.datos?.length
      ? this.datos.slice(-7)
      : this.generatePlaceholders();

    this.isEmpty = raw.every(d => d.completadas === 0 && d.total === 0);
    this.maxTotal = Math.max(...raw.map(d => d.total), 1);
    this.halfTotal = this.maxTotal <= 2 ? 0 : Math.round(this.maxTotal / 2);

    this.bars = raw.map(d => ({
      label: this.dayLabel(d.fecha),
      fecha: d.fecha,
      completadas: d.completadas,
      total: d.total,
      heightPct: (d.total / this.maxTotal) * 100,
      totalPct: (d.total / this.maxTotal) * 100,
      isToday: d.fecha.slice(0, 10) === todayStr
    }));
  }

  /** Returns how tall the "completed" fill is relative to the total bar (0–100%). */
  barInnerPct(d: BarDatum): number {
    if (!d.total) return 0;
    return Math.min(100, (d.completadas / d.total) * 100);
  }

  get ariaDescription(): string {
    return this.bars
      .map(d => `${d.label}: ${d.completadas} completadas de ${d.total}`)
      .join(', ');
  }

  private dayLabel(isoDate: string): string {
    const d = new Date(isoDate + 'T12:00:00'); // noon avoids DST off-by-one
    return this.DAY_LABELS[d.getDay()];
  }

  private generatePlaceholders(): MetasDia[] {
    const days: MetasDia[] = [];
    for (let i = 6; i >= 0; i--) {
      const d = new Date();
      d.setDate(d.getDate() - i);
      days.push({ fecha: d.toISOString().slice(0, 10), completadas: 0, total: 0 });
    }
    return days;
  }
}
