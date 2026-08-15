import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

export type MetricCardAccent = 'default' | 'warning' | 'danger' | 'success';

/**
 * Tarjeta reutilizable para las 4 métricas ejecutivas del Dashboard.
 * Inputs:
 *   label   — etiqueta descriptiva (e.g. "Alumnos activos")
 *   value   — número o string a mostrar en grande
 *   accent  — variante de color: default | warning | danger | success
 *   icon    — emoji o carácter Unicode opcional (e.g. "🎯")
 */
@Component({
  selector: 'app-metric-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <article [class]="'metric-card metric-card--' + accent" [attr.aria-label]="label + ': ' + value">
      <header class="metric-card__header">
        <span class="metric-card__icon" aria-hidden="true" *ngIf="icon">{{ icon }}</span>
        <span class="metric-card__label">{{ label }}</span>
      </header>
      <strong class="metric-card__value">{{ value }}</strong>
      <p class="metric-card__sub" *ngIf="sub">{{ sub }}</p>
    </article>
  `,
  styles: [`
    :host {
      display: contents;
    }

    .metric-card {
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-card);
      padding: var(--space-5);
      display: grid;
      gap: var(--space-2);
      transition: box-shadow 150ms ease;
    }

    .metric-card:hover {
      box-shadow: var(--shadow-md, 0 4px 12px rgba(0,0,0,.1));
    }

    .metric-card__header {
      display: flex;
      align-items: center;
      gap: var(--space-2);
    }

    .metric-card__icon {
      font-size: var(--font-size-base);
      line-height: 1;
    }

    .metric-card__label {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
    }

    .metric-card__value {
      font-size: var(--font-size-2xl);
      line-height: var(--line-height-tight);
      font-weight: var(--font-weight-bold);
      color: var(--color-text-primary);
    }

    .metric-card__sub {
      font-size: var(--font-size-xs);
      color: var(--color-text-secondary);
      margin: 0;
    }

    /* Accent variants */
    .metric-card--warning .metric-card__value {
      color: var(--color-warning);
    }

    .metric-card--danger .metric-card__value {
      color: var(--color-danger);
    }

    .metric-card--success .metric-card__value {
      color: var(--color-success);
    }

    /* Left border indicator */
    .metric-card--warning {
      border-left: 3px solid var(--color-warning);
    }

    .metric-card--danger {
      border-left: 3px solid var(--color-danger);
    }

    .metric-card--success {
      border-left: 3px solid var(--color-success);
    }
  `]
})
export class MetricCardComponent {
  @Input({ required: true }) label!: string;
  @Input({ required: true }) value!: string | number;
  @Input() accent: MetricCardAccent = 'default';
  @Input() icon: string = '';
  @Input() sub: string = '';
}
