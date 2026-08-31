import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { AlumnosService } from '../../core/services/alumnos.service';
import { AuthService } from '../../core/services/auth.service';
import { DashboardService } from '../../core/services/dashboard.service';
import { Alerta, Alumno, DashboardResumen, Rol } from '../../models';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { MetricCardComponent } from './metric-card.component';
import { ScorePendingListComponent } from './score-pending-list.component';
import { WeeklyChartComponent } from './weekly-chart.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, BadgeComponent, ButtonComponent, SpinnerComponent, MetricCardComponent, WeeklyChartComponent, ScorePendingListComponent],
  template: `
    <main class="page-shell">
      <header class="topbar">
        <div>
          <p class="eyebrow">Centro de mando</p>
          <h1>Dashboard</h1>
        </div>
        <div class="topbar-actions">
          <app-button variant="ghost" (click)="reload()">Actualizar</app-button>
        </div>
      </header>

      <section class="state" *ngIf="loading">
        <app-spinner variant="overlay" />
      </section>

      <section class="state error" *ngIf="!loading && errorMessage">
        <p>{{ errorMessage }}</p>
        <app-button (click)="reload()">Reintentar</app-button>
      </section>

      <ng-container *ngIf="!loading && !errorMessage">
        <section class="metrics" *ngIf="canViewExecutiveMetrics">
          <app-metric-card
            label="Alumnos activos"
            [value]="alumnosActivos"
            icon="👥"
            accent="default"
          />
          <app-metric-card
            label="Metas hoy"
            [value]="metasHoy"
            icon="🎯"
            [accent]="metasHoy === 0 ? 'warning' : 'success'"
          />
          <app-metric-card
            label="PACEs en revisión"
            [value]="resumen?.pacesEnRevision ?? 0"
            icon="📋"
            accent="default"
          />
          <app-metric-card
            label="Alertas"
            [value]="totalAlertas"
            icon="⚠️"
            [accent]="totalAlertas > 0 ? 'warning' : 'default'"
          />
        </section>

        <section class="monitor-note" *ngIf="!canViewExecutiveMetrics">
          <div>
            <h2>Alumnos del salón</h2>
            <p>Selecciona un alumno para revisar PACEs, metas y méritos.</p>
          </div>
        </section>

        <section class="alerts" *ngIf="canViewExecutiveMetrics">
          <div class="section-header">
            <h2>Alertas activas</h2>
            <app-badge [variant]="alertas.length ? 'orange' : 'green'">
              {{ alertas.length ? alertas.length + ' pendientes' : 'Sin alertas' }}
            </app-badge>
          </div>

          <div class="empty" *ngIf="alertas.length === 0">
            No hay alumnos con alertas activas.
          </div>

          <a class="alert-row" *ngFor="let alerta of alertas" [routerLink]="['/alumno', alerta.alumnoId]">
            <div>
              <strong>{{ alertaNombre(alerta) }}</strong>
              <span>{{ alertaMensaje(alerta) }}</span>
            </div>
            <app-badge variant="orange">{{ alerta.nivel ?? alerta.tipo ?? 'Seguimiento' }}</app-badge>
          </a>
        </section>

        <div class="two-col" *ngIf="canViewExecutiveMetrics">
          <app-weekly-chart [datos]="resumen?.metasPorDia ?? []" />
          <app-score-pending-list [count]="resumen?.pacesEnRevision ?? 0" />
        </div>

        <section class="students">
          <div class="section-header table-tools">
            <div>
              <h2>Alumnos</h2>
              <p>{{ filteredAlumnos.length }} registros visibles</p>
            </div>
            <div class="filters">
              <input
                type="search"
                [(ngModel)]="search"
                placeholder="Buscar alumno"
                aria-label="Buscar alumno"
              />
              <select [(ngModel)]="nivelFiltro" aria-label="Filtrar por nivel">
                <option value="">Todos los niveles</option>
                <option *ngFor="let nivel of niveles" [value]="nivel">{{ nivel }}</option>
              </select>
            </div>
          </div>

          <div class="table-wrap" *ngIf="filteredAlumnos.length; else emptyStudents">
            <table>
              <thead>
                <tr>
                  <th>Alumno</th>
                  <th>Nivel</th>
                  <th>Balance</th>
                  <th>Privilegios</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let alumno of visibleAlumnos">
                  <td>
                    <strong>{{ alumno.nombre }} {{ alumno.apellido }}</strong>
                    <span>{{ alumno.numeroMatricula }}</span>
                  </td>
                  <td>{{ alumno.nivel }}</td>
                  <td>
                    <app-badge [variant]="balanceVariant(alumno.balanceMeritos ?? 0)">
                      {{ alumno.balanceMeritos ?? 0 }}
                    </app-badge>
                  </td>
                  <td>
                    <span class="privileges">{{ privilegiosActivos(alumno).join(', ') || 'Sin privilegios' }}</span>
                  </td>
                  <td>
                    <a class="row-action" [routerLink]="['/alumno', alumno.id]">Abrir</a>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <ng-template #emptyStudents>
            <div class="empty">No hay alumnos que coincidan con el filtro.</div>
          </ng-template>

          <footer class="pager" *ngIf="totalPages > 1">
            <button type="button" (click)="previousPage()" [disabled]="page === 1">Anterior</button>
            <span>{{ page }} / {{ totalPages }}</span>
            <button type="button" (click)="nextPage()" [disabled]="page === totalPages">Siguiente</button>
          </footer>
        </section>
      </ng-container>
    </main>
  `,
  styles: [`
    :host {
      display: block;
      min-height: 100vh;
      background: var(--color-bg-page);
    }

    .page-shell {
      width: min(1180px, 100%);
      margin: 0 auto;
      padding: var(--space-6);
      display: grid;
      gap: var(--space-6);
    }

    .topbar,
    .section-header,
    .alert-row,
    .pager {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-4);
    }

    .topbar-actions,
    .filters {
      display: flex;
      align-items: center;
      gap: var(--space-3);
      flex-wrap: wrap;
    }

    .eyebrow,
    .section-header p,
    td span,
    .monitor-note p {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
    }

    h1 {
      font-size: var(--font-size-2xl);
      line-height: var(--line-height-tight);
    }

    h2 {
      font-size: var(--font-size-xl);
      line-height: var(--line-height-tight);
    }

    .metrics {
      display: grid;
      grid-template-columns: repeat(4, minmax(0, 1fr));
      gap: var(--space-4);
    }

    .two-col {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-4);
      align-items: start;
    }

    .alerts,
    .students,
    .monitor-note {
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-card);
    }

    .alerts,
    .students,
    .monitor-note {
      padding: var(--space-5);
      display: grid;
      gap: var(--space-4);
    }

    .alert-row {
      min-height: 64px;
      padding: var(--space-4);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      color: inherit;
      text-decoration: none;
      background: var(--color-warning-bg);
    }

    .alert-row div {
      display: grid;
      gap: var(--space-1);
    }

    .alert-row span {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
    }

    input,
    select {
      min-height: var(--tap-target-min);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      padding: 0 var(--space-4);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
    }

    .table-wrap {
      overflow-x: auto;
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
    }

    table {
      width: 100%;
      border-collapse: collapse;
      min-width: 760px;
    }

    th,
    td {
      padding: var(--space-4);
      border-bottom: 1px solid var(--color-border);
      text-align: left;
      vertical-align: middle;
    }

    th {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
      background: var(--color-gray-50);
    }

    td:first-child {
      display: grid;
      gap: var(--space-1);
    }

    tbody tr:last-child td {
      border-bottom: 0;
    }

    .privileges {
      display: inline-block;
      max-width: 280px;
      white-space: normal;
    }

    .row-action {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      min-height: var(--tap-target-min);
      padding: 0 var(--space-4);
      border-radius: var(--radius-sm);
      background: var(--color-accent-light);
      color: var(--color-accent);
      font-weight: var(--font-weight-semibold);
      text-decoration: none;
    }

    .empty,
    .state {
      display: grid;
      place-items: center;
      min-height: 160px;
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

    .pager {
      justify-content: flex-end;
    }

    .pager button {
      min-height: var(--tap-target-min);
      padding: 0 var(--space-4);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      cursor: pointer;
    }

    .pager button:disabled {
      color: var(--color-text-disabled);
      cursor: not-allowed;
    }

    @media (max-width: 820px) {
      .page-shell {
        padding: var(--space-4);
      }

      .topbar,
      .section-header.table-tools {
        align-items: flex-start;
        flex-direction: column;
      }

      .metrics {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }

      .two-col {
        grid-template-columns: 1fr;
      }

      .filters {
        width: 100%;
      }

      input,
      select {
        width: 100%;
      }
    }

    @media (max-width: 520px) {
      .metrics {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DashboardComponent implements OnInit {
  role: Rol | null = null;
  resumen: DashboardResumen | null = null;
  alertas: Alerta[] = [];
  alumnos: Alumno[] = [];
  loading = true;
  errorMessage = '';
  search = '';
  nivelFiltro = '';
  page = 1;
  readonly pageSize = 8;

  constructor(
    private dashboardService: DashboardService,
    private alumnosService: AlumnosService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.role = this.authService.getRole();
    this.load();
  }

  get canViewExecutiveMetrics(): boolean {
    return this.role === 'Principal' || this.role === 'Supervisora';
  }

  get alumnosActivos(): number {
    return this.resumen?.alumnosActivos ?? this.resumen?.totalAlumnosActivos ?? this.alumnos.length;
  }

  get metasHoy(): number {
    return this.resumen?.metasCompletadasHoy ?? this.resumen?.metasHoy ?? 0;
  }

  get totalAlertas(): number {
    return this.resumen?.alertasActivas ?? this.resumen?.totalAlertas ?? this.alertas.length;
  }

  get niveles(): string[] {
    return [...new Set(this.alumnos.map(a => a.nivel).filter(Boolean))].sort();
  }

  get filteredAlumnos(): Alumno[] {
    const term = this.search.trim().toLowerCase();
    return this.alumnos.filter(alumno => {
      const matchesNivel = !this.nivelFiltro || alumno.nivel === this.nivelFiltro;
      const fullName = `${alumno.nombre} ${alumno.apellido} ${alumno.numeroMatricula}`.toLowerCase();
      const matchesSearch = !term || fullName.includes(term);
      return matchesNivel && matchesSearch;
    });
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredAlumnos.length / this.pageSize));
  }

  get visibleAlumnos(): Alumno[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filteredAlumnos.slice(start, start + this.pageSize);
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    const request = this.canViewExecutiveMetrics
      ? forkJoin({
          resumen: this.dashboardService.getResumen(),
          alertas: this.dashboardService.getAlertas(),
          alumnos: this.alumnosService.getAll()
        })
      : forkJoin({
          resumen: of(null as DashboardResumen | null),
          alertas: of([] as Alerta[]),
          alumnos: this.alumnosService.getAll()
        });

    request.subscribe({
      next: result => {
        this.resumen = result.resumen;
        this.alertas = result.alertas;
        this.alumnos = result.alumnos;
        this.page = 1;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.errorMessage = 'No se pudo cargar el dashboard.';
      }
    });
  }

  reload(): void {
    this.load();
  }

  previousPage(): void {
    this.page = Math.max(1, this.page - 1);
  }

  nextPage(): void {
    this.page = Math.min(this.totalPages, this.page + 1);
  }

  alertaNombre(alerta: Alerta): string {
    return alerta.nombreAlumno ?? (`${alerta.nombre ?? ''} ${alerta.apellido ?? ''}`.trim() || 'Alumno');
  }

  alertaMensaje(alerta: Alerta): string {
    return alerta.mensaje ?? `Última meta completada: ${alerta.ultimaMetaFecha ?? 'sin registro'}`;
  }

  balanceVariant(balance: number): BadgeVariant {
    if (balance < 0) return 'red';
    if (balance === 0) return 'gray';
    return 'green';
  }

  privilegiosActivos(alumno: Alumno): string[] {
    if (alumno.privilegiosActivos?.length) {
      return alumno.privilegiosActivos;
    }

    const status = alumno.privilegeStatus;
    if (!status) return [];

    return Object.entries(status)
      .filter(([, active]) => active)
      .map(([name]) => name.charAt(0).toUpperCase() + name.slice(1));
  }
}
