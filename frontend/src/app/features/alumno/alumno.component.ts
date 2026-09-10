import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { Observable, forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AlumnosService } from '../../core/services/alumnos.service';
import { AuthService } from '../../core/services/auth.service';
import { MeritosService } from '../../core/services/meritos.service';
import { PacesService } from '../../core/services/paces.service';
import { ProgresoService } from '../../core/services/progreso.service';
import { ToastService } from '../../core/services/toast.service';
import { todayIso, weekStartIso } from '../../core/utils/fecha.util';
import { Alumno, AlumnoPace, EstadoMeta, Merito, Meta, ModoPrivilegio, Pace, PrivilegioFlag, Rol } from '../../models';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { ProgressBarComponent } from '../../shared/components/progress-bar/progress-bar.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { AnotacionesPanelComponent } from './anotaciones-panel.component';
import { QuickActionsComponent } from './quick-actions.component';

@Component({
  selector: 'app-alumno',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, BadgeComponent, ButtonComponent, ProgressBarComponent, SpinnerComponent, QuickActionsComponent, AnotacionesPanelComponent],
  template: `
    <main class="page-shell">
      <header class="topbar">
        <a routerLink="/dashboard" class="back-link">Volver</a>
        <div class="topbar-actions">
          <app-badge variant="blue">{{ role ?? 'Sesión' }}</app-badge>
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

      <ng-container *ngIf="!loading && !errorMessage && alumno">
        <section class="hero">
          <div class="avatar" aria-hidden="true">{{ initials(alumno) }}</div>
          <div class="hero-copy">
            <p class="eyebrow">{{ alumno.numeroMatricula }}</p>
            <h1>{{ alumno.nombre }} {{ alumno.apellido }}</h1>
            <div class="hero-meta">
              <app-badge variant="gray">{{ alumno.nivel }}</app-badge>
              <app-badge [variant]="balanceVariant(alumno.balanceMeritos ?? 0)">
                Balance {{ alumno.balanceMeritos ?? 0 }}
              </app-badge>
              <span *ngIf="alumno.fechaIngreso">Ingreso {{ alumno.fechaIngreso | date:'dd/MM/yyyy' }}</span>
            </div>
          </div>
        </section>

        <section class="privilege-band">
          <div *ngFor="let item of privilegeItems(alumno)" class="privilege">
            <span>{{ item.label }}</span>
            <app-badge [variant]="item.active ? 'green' : 'gray'">{{ item.active ? 'Activo' : 'Inactivo' }}</app-badge>

            <ng-container *ngIf="canOverridePrivileges; else soloLectura">
              <select
                class="privilege-mode"
                [value]="item.mode"
                [disabled]="savingPrivilege === item.flag"
                [attr.aria-label]="'Modo del privilegio ' + item.label"
                (change)="setPrivilege(item.flag, $event)"
              >
                <option value="auto">Automático</option>
                <option value="activo">Forzar activo</option>
                <option value="inactivo">Forzar inactivo</option>
              </select>
            </ng-container>
            <ng-template #soloLectura>
              <small class="privilege-note" *ngIf="item.mode !== 'auto'">Definido a mano</small>
            </ng-template>
          </div>
        </section>

        <app-quick-actions
          [alumnoId]="alumnoId"
          [paces]="paces"
          [role]="role"
          (changed)="reload()"
        />

        <div class="content-grid">
          <section class="panel paces-panel">
            <div class="section-header">
              <h2>PACEs activos</h2>
              <app-badge variant="blue">{{ paces.length }}</app-badge>
            </div>

            <div class="section-error" *ngIf="pacesError">{{ pacesError }}</div>
            <div class="empty" *ngIf="!pacesError && paces.length === 0">Este alumno aún no tiene PACEs asignados.</div>

            <article class="pace-card" *ngFor="let pace of paces">
              <div>
                <strong>{{ pace.materia }} {{ pace.numeroPace || pace.pace?.numeroPace || '' }}</strong>
                <span>{{ pace.estado }}</span>
              </div>
              <app-progress-bar [value]="paceProgress(pace)" />
              <div class="pace-actions">
                <app-badge [variant]="paceVariant(pace.estado)">{{ pace.estado }}</app-badge>
              </div>
            </article>

            <form class="inline-form" [formGroup]="assignForm" (ngSubmit)="assignPace()" *ngIf="canManagePaces">
              <label>
                <span>Asignar PACE</span>
                <select formControlName="paceId">
                  <option value="">Selecciona un PACE</option>
                  <option *ngFor="let pace of catalogo" [value]="pace.id">
                    {{ pace.materia }} {{ pace.numeroPace }}
                  </option>
                </select>
              </label>
              <app-button [loading]="assigning" [disabled]="assignForm.invalid">Asignar</app-button>
            </form>
          </section>

          <section class="panel goals-panel">
            <div class="section-header">
              <h2>Metas de la semana</h2>
              <app-badge variant="gray">{{ metas.length }}</app-badge>
            </div>

            <div class="section-error" *ngIf="metasError">{{ metasError }}</div>
            <div class="empty" *ngIf="!metasError && metas.length === 0">No hay metas registradas esta semana.</div>

            <article class="goal-row" *ngFor="let meta of metas">
              <div>
                <strong>{{ meta.turno }} · {{ meta.paginasObjetivo }} páginas</strong>
                <span>{{ meta.fechaObjetivo | date:'dd/MM/yyyy' }} · {{ meta.materia ?? 'PACE' }} {{ meta.numeroPace ?? '' }}</span>
              </div>
              <app-badge [variant]="metaVariant(meta.estado)">{{ meta.estado }}</app-badge>
              <div class="goal-actions">
                <button type="button" (click)="updateMeta(meta, 'Completada')" [disabled]="meta.estado !== 'EnProgreso' && meta.estado !== 'Pendiente'">
                  Completar
                </button>
                <button type="button" (click)="updateMeta(meta, 'Rechazada')" [disabled]="meta.estado !== 'EnProgreso' && meta.estado !== 'Pendiente'">
                  Rechazar
                </button>
                <ng-container *ngIf="canScore">
                  <input
                    type="number"
                    min="0"
                    max="100"
                    [value]="scoreInputs[meta.id] ?? ''"
                    (input)="setScore(meta.id, $event)"
                    aria-label="Puntaje Score Station"
                  />
                  <button type="button" (click)="scoreMeta(meta)" [disabled]="scoreInputs[meta.id] == null">
                    Score
                  </button>
                </ng-container>
              </div>
            </article>

            <form class="inline-form goal-form" [formGroup]="goalForm" (ngSubmit)="createGoal()">
              <label>
                <span>PACE</span>
                <select formControlName="alumnoPaceId">
                  <option value="">Selecciona un PACE</option>
                  <option *ngFor="let pace of paces" [value]="pace.id">
                    {{ pace.materia }} {{ pace.numeroPace || pace.pace?.numeroPace || '' }}
                  </option>
                </select>
              </label>
              <label>
                <span>Turno</span>
                <select formControlName="turno">
                  <option value="Mañana">Mañana</option>
                  <option value="Tarde">Tarde</option>
                </select>
              </label>
              <label>
                <span>Páginas</span>
                <input type="number" min="1" formControlName="paginasObjetivo" />
              </label>
              <label>
                <span>Fecha</span>
                <input type="date" formControlName="fechaObjetivo" />
              </label>
              <app-button [loading]="creatingGoal" [disabled]="goalForm.invalid">Agregar meta</app-button>
            </form>
          </section>

          <section class="panel merits-panel">
            <div class="section-header">
              <h2>Méritos y deméritos</h2>
              <app-badge variant="gray">{{ meritos.length }}</app-badge>
            </div>

            <form class="inline-form merit-form" [formGroup]="meritForm" (ngSubmit)="registerMerit()">
              <label>
                <span>Tipo</span>
                <select formControlName="tipo">
                  <option value="Merito">Mérito</option>
                  <option value="Demerito">Demérito</option>
                </select>
              </label>
              <label>
                <span>Puntos</span>
                <input type="number" min="1" formControlName="puntos" />
              </label>
              <label class="wide">
                <span>Motivo</span>
                <input type="text" formControlName="motivo" placeholder="Motivo del registro" />
              </label>
              <app-button [loading]="registeringMerit" [disabled]="meritForm.invalid">Registrar</app-button>
            </form>

            <div class="section-error" *ngIf="meritosError">{{ meritosError }}</div>
            <div class="empty" *ngIf="!meritosError && meritos.length === 0">No hay méritos o deméritos registrados.</div>

            <div class="merit-week" *ngFor="let semana of meritosPorSemana">
              <div class="week-divider">
                <span class="week-label">{{ semana.etiqueta }}</span>
                <app-badge [variant]="balanceVariant(semana.balance)">
                  {{ semana.balance > 0 ? '+' : '' }}{{ semana.balance }} en la semana
                </app-badge>
              </div>

              <article class="merit-row" *ngFor="let merito of semana.registros" [class.revoked]="merito.revocado">
                <div>
                  <strong>{{ merito.tipo }} · {{ merito.puntos }} puntos</strong>
                  <span>{{ merito.motivo }}</span>
                  <small>{{ merito.fechaAplicado | date:'dd/MM/yyyy HH:mm' }} · {{ merito.staffName ?? 'Personal Escolar' }}</small>
                  <small *ngIf="merito.revocado" class="revoked-note">
                    Eliminado{{ merito.fechaRevocacion ? (' el ' + (merito.fechaRevocacion | date:'dd/MM/yyyy')) : '' }}
                    {{ merito.staffRevocoName ? 'por ' + merito.staffRevocoName : '' }}
                  </small>
                </div>
                <div class="merit-actions">
                  <app-badge [variant]="merito.revocado ? 'gray' : merito.tipo === 'Merito' ? 'green' : 'red'">
                    {{ merito.revocado ? 'Eliminado' : merito.tipo }}
                  </app-badge>
                  <button
                    type="button"
                    class="link-danger"
                    *ngIf="canDeleteMerits && !merito.revocado"
                    [disabled]="deletingMeritId === merito.id"
                    (click)="deleteMerit(merito)"
                  >
                    {{ deletingMeritId === merito.id ? 'Eliminando…' : 'Eliminar' }}
                  </button>
                </div>
              </article>
            </div>
          </section>

          <app-anotaciones-panel class="notes-panel" [alumnoId]="alumnoId" />
        </div>
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
    .topbar-actions,
    .section-header,
    .hero,
    .hero-meta,
    .privilege,
    .pace-card,
    .goal-row,
    .merit-row,
    .goal-actions {
      display: flex;
      align-items: center;
      gap: var(--space-4);
    }

    .topbar,
    .section-header,
    .pace-card,
    .goal-row,
    .merit-row {
      justify-content: space-between;
    }

    .back-link {
      display: inline-flex;
      align-items: center;
      min-height: var(--tap-target-min);
      color: var(--color-accent);
      font-weight: var(--font-weight-semibold);
      text-decoration: none;
    }

    .hero,
    .privilege-band,
    .panel {
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
    }

    .hero {
      padding: var(--space-6);
    }

    .avatar {
      display: grid;
      place-items: center;
      width: 72px;
      height: 72px;
      flex: 0 0 auto;
      border-radius: var(--radius-md);
      background: var(--color-accent);
      color: var(--color-text-inverse);
      font-size: var(--font-size-xl);
      font-weight: var(--font-weight-bold);
    }

    .hero-copy {
      display: grid;
      gap: var(--space-2);
      min-width: 0;
    }

    .eyebrow,
    .pace-card span,
    .goal-row span,
    .merit-row span {
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

    .privilege-band {
      display: grid;
      grid-template-columns: repeat(5, minmax(0, 1fr));
      gap: var(--space-3);
      padding: var(--space-4);
    }

    .privilege {
      justify-content: space-between;
      min-height: var(--tap-target-min);
      padding: 0 var(--space-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
    }

    .content-grid {
      display: grid;
      grid-template-columns: minmax(280px, 0.85fr) minmax(0, 1.15fr);
      gap: var(--space-6);
      align-items: start;
    }

    .panel {
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
    }

    .goals-panel,
    .merits-panel {
      grid-column: 2;
    }

    .pace-card,
    .goal-row,
    .merit-row {
      min-height: 72px;
      padding: var(--space-4);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
    }

    .pace-card {
      display: grid;
      align-items: stretch;
    }

    .pace-card > div:first-child,
    .goal-row > div:first-child,
    .merit-row > div:first-child {
      display: grid;
      gap: var(--space-1);
      min-width: 0;
    }

    .privilege-mode {
      width: 100%;
      padding: var(--space-1) var(--space-2);
      font-size: 0.75rem;
    }

    .privilege-note {
      font-size: 0.7rem;
      font-style: italic;
      color: var(--color-text-muted);
    }

    .notes-panel {
      display: block;
      grid-column: 1 / -1;
    }

    /* Agrupación semanal de méritos (issue #19) */
    .merit-week {
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

    .merit-actions {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }

    .merit-row.revoked > div:first-child strong,
    .merit-row.revoked > div:first-child span {
      text-decoration: line-through;
      color: var(--color-text-muted);
    }

    .revoked-note {
      font-style: italic;
    }

    .link-danger {
      padding: 0;
      border: 0;
      background: none;
      font: inherit;
      color: var(--color-danger);
      cursor: pointer;
      text-decoration: underline;
    }

    .link-danger:disabled {
      color: var(--color-text-muted);
      cursor: progress;
      text-decoration: none;
    }

    .inline-form {
      display: grid;
      grid-template-columns: minmax(0, 1fr) auto;
      gap: var(--space-3);
      align-items: end;
      padding-top: var(--space-4);
      border-top: 1px solid var(--color-border);
    }

    .goal-form,
    .merit-form {
      grid-template-columns: repeat(4, minmax(0, 1fr)) auto;
    }

    .merit-form .wide {
      grid-column: span 2;
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
      min-height: var(--tap-target-min);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      padding: 0 var(--space-3);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
    }

    .goal-actions {
      flex-wrap: wrap;
      justify-content: flex-end;
    }

    .goal-actions button {
      min-height: var(--tap-target-min);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      padding: 0 var(--space-3);
      background: var(--color-bg-surface);
      cursor: pointer;
      font-weight: var(--font-weight-semibold);
    }

    .goal-actions button:disabled {
      color: var(--color-text-disabled);
      cursor: not-allowed;
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

    .section-error {
      padding: var(--space-4);
      border: 1px solid var(--color-danger);
      border-radius: var(--radius-md);
      color: var(--color-danger);
      background: var(--color-danger-bg);
    }

    @media (max-width: 980px) {
      .content-grid,
      .goals-panel,
      .merits-panel,
      .paces-panel {
        display: grid;
        grid-template-columns: 1fr;
        grid-column: auto;
        grid-row: auto;
      }

      .privilege-band {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }

    @media (max-width: 680px) {
      .page-shell {
        padding: var(--space-4);
      }

      .topbar,
      .hero,
      .pace-card,
      .goal-row,
      .merit-row,
      .section-header {
        align-items: flex-start;
        flex-direction: column;
      }

      .hero-meta,
      .topbar-actions {
        flex-wrap: wrap;
      }

      .inline-form,
      .goal-form,
      .merit-form,
      .merit-form .wide {
        grid-template-columns: 1fr;
        grid-column: auto;
      }

      .privilege-band {
        grid-template-columns: 1fr;
      }

      .goal-actions {
        justify-content: flex-start;
      }
    }
  `]
})
export class AlumnoComponent implements OnInit {
  private readonly fb = inject(FormBuilder);

  alumnoId = '';
  alumno: Alumno | null = null;
  paces: AlumnoPace[] = [];
  catalogo: Pace[] = [];
  metas: Meta[] = [];
  meritos: Merito[] = [];
  role: Rol | null = null;
  loading = true;
  errorMessage = '';
  pacesError = '';
  metasError = '';
  meritosError = '';
  creatingGoal = false;
  registeringMerit = false;
  assigning = false;
  deletingMeritId: string | null = null;
  savingPrivilege: PrivilegioFlag | null = null;
  scoreInputs: Record<string, number | null> = {};

  /**
   * Méritos agrupados por semana (issue #19). Se calcula al cargar y no en un
   * getter: la detección de cambios lo llamaría en cada ciclo y reagrupar la
   * lista completa en cada uno es trabajo que no hace falta repetir.
   */
  meritosPorSemana: SemanaDeMeritos[] = [];

  readonly goalForm = this.fb.nonNullable.group({
    alumnoPaceId: ['', Validators.required],
    turno: ['Mañana' as 'Mañana' | 'Tarde', Validators.required],
    paginasObjetivo: [1, [Validators.required, Validators.min(1)]],
    fechaObjetivo: [this.today(), Validators.required]
  });

  readonly meritForm = this.fb.nonNullable.group({
    tipo: ['Merito' as 'Merito' | 'Demerito', Validators.required],
    puntos: [1, [Validators.required, Validators.min(1)]],
    motivo: ['', [Validators.required, Validators.minLength(3)]]
  });

  readonly assignForm = this.fb.nonNullable.group({
    paceId: ['', Validators.required]
  });

  constructor(
    private route: ActivatedRoute,
    private alumnosService: AlumnosService,
    private pacesService: PacesService,
    private progresoService: ProgresoService,
    private meritosService: MeritosService,
    private authService: AuthService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.role = this.authService.getRole();
    this.alumnoId = this.route.snapshot.paramMap.get('id') ?? '';
    this.load();
  }

  get canScore(): boolean {
    return this.role === 'Principal' || this.role === 'Supervisora';
  }

  get canManagePaces(): boolean {
    return this.canScore;
  }

  /** Solo el Principal elimina méritos, por acuerdo con el cliente (issue #19). */
  get canDeleteMerits(): boolean {
    return this.role === 'Principal';
  }

  /**
   * Forzar un privilegio es una excepción a la regla de méritos, así que queda
   * en el Principal igual que en el endpoint (issue #21).
   */
  get canOverridePrivileges(): boolean {
    return this.role === 'Principal';
  }

  load(): void {
    if (!this.alumnoId) {
      this.loading = false;
      this.errorMessage = 'Alumno no encontrado.';
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.pacesError = '';
    this.metasError = '';
    this.meritosError = '';

    // Solo el alumno es indispensable para pintar el perfil. Las demas secciones
    // se degradan por separado para que una de ellas no deje la pantalla vacia.
    forkJoin({
      alumno: this.alumnosService.getById(this.alumnoId),
      paces: this.optional(this.pacesService.getByAlumno(this.alumnoId), [] as AlumnoPace[], message => (this.pacesError = message)),
      metas: this.optional(this.progresoService.getSemana(this.alumnoId, this.weekStart()), [] as Meta[], message => (this.metasError = message)),
      meritos: this.optional(this.meritosService.getByAlumno(this.alumnoId), [] as Merito[], message => (this.meritosError = message)),
      catalogo: this.canManagePaces
        ? this.optional(this.pacesService.getCatalogo(), [] as Pace[], message => (this.pacesError = message))
        : of([] as Pace[])
    }).subscribe({
      next: result => {
        this.alumno = result.alumno;
        this.paces = result.paces;
        this.metas = result.metas;
        this.meritos = result.meritos;
        this.meritosPorSemana = this.agruparPorSemana(result.meritos);
        this.catalogo = result.catalogo;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = this.describeError(error, 'No se pudo cargar el perfil del alumno.');
      }
    });
  }

  reload(): void {
    this.load();
  }

  createGoal(): void {
    if (this.goalForm.invalid || !this.alumno) {
      this.goalForm.markAllAsTouched();
      return;
    }

    this.creatingGoal = true;
    const value = this.goalForm.getRawValue();

    this.progresoService.crearMeta({
      alumnoId: this.alumno.id,
      alumnoPaceId: value.alumnoPaceId,
      turno: value.turno,
      paginasObjetivo: value.paginasObjetivo,
      fechaObjetivo: value.fechaObjetivo
    }).subscribe({
      next: () => {
        this.creatingGoal = false;
        this.toast.success('Meta registrada.');
        this.goalForm.patchValue({ paginasObjetivo: 1 });
        this.load();
      },
      error: () => {
        this.creatingGoal = false;
        this.toast.error('No se pudo registrar la meta.');
      }
    });
  }

  registerMerit(): void {
    if (this.meritForm.invalid || !this.alumno) {
      this.meritForm.markAllAsTouched();
      return;
    }

    this.registeringMerit = true;
    const value = this.meritForm.getRawValue();

    this.meritosService.registrar({
      alumnoId: this.alumno.id,
      tipo: value.tipo,
      puntos: value.puntos,
      motivo: value.motivo
    }).subscribe({
      next: () => {
        this.registeringMerit = false;
        this.toast.success('Registro guardado.');
        this.meritForm.patchValue({ puntos: 1, motivo: '' });
        this.load();
      },
      error: () => {
        this.registeringMerit = false;
        this.toast.error('No se pudo guardar el registro.');
      }
    });
  }

  deleteMerit(merito: Merito): void {
    const etiqueta = merito.tipo === 'Merito' ? 'el mérito' : 'el demérito';
    const confirmado = window.confirm(
      `¿Eliminar ${etiqueta} de ${merito.puntos} puntos?\n\n` +
        `Motivo: ${merito.motivo}\n\n` +
        'El balance del alumno se recalcula y el registro queda marcado como eliminado.'
    );
    if (!confirmado) return;

    this.deletingMeritId = merito.id;
    this.meritosService.revocar(merito.id).subscribe({
      next: () => {
        this.deletingMeritId = null;
        this.toast.success('Registro eliminado.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.deletingMeritId = null;
        this.toast.error(this.describeError(error, 'No se pudo eliminar el registro.'));
      }
    });
  }

  /**
   * Parte la lista en semanas de lunes a domingo. Sin esta separación los
   * registros de semanas distintas se leen como un solo bloque y se confunden
   * con el acumulado de la semana en curso (issue #19).
   */
  private agruparPorSemana(meritos: Merito[]): SemanaDeMeritos[] {
    const grupos = new Map<string, SemanaDeMeritos>();

    for (const merito of [...meritos].sort((a, b) => b.fechaAplicado.localeCompare(a.fechaAplicado))) {
      const inicio = weekStartIso(new Date(merito.fechaAplicado));
      let grupo = grupos.get(inicio);

      if (!grupo) {
        grupo = { inicio, etiqueta: this.etiquetaSemana(inicio), balance: 0, registros: [] };
        grupos.set(inicio, grupo);
      }

      grupo.registros.push(merito);

      // Un registro eliminado ya no cuenta: su balance fue revertido en el backend.
      if (!merito.revocado) {
        grupo.balance += merito.tipo === 'Merito' ? merito.puntos : -merito.puntos;
      }
    }

    return [...grupos.values()].sort((a, b) => b.inicio.localeCompare(a.inicio));
  }

  private etiquetaSemana(inicioIso: string): string {
    const [anio, mes, dia] = inicioIso.split('-').map(Number);
    const inicio = new Date(anio, mes - 1, dia);
    const fin = new Date(anio, mes - 1, dia + 6);

    const corto = (fecha: Date) =>
      `${`${fecha.getDate()}`.padStart(2, '0')}/${`${fecha.getMonth() + 1}`.padStart(2, '0')}`;

    const esSemanaActual = inicioIso === weekStartIso();
    return `${esSemanaActual ? 'Esta semana' : 'Semana'} · ${corto(inicio)} – ${corto(fin)}`;
  }

  assignPace(): void {
    if (this.assignForm.invalid || !this.alumno) {
      this.assignForm.markAllAsTouched();
      return;
    }

    this.assigning = true;
    this.pacesService.asignar(this.alumno.id, this.assignForm.controls.paceId.value).subscribe({
      next: () => {
        this.assigning = false;
        this.toast.success('PACE asignado.');
        this.assignForm.reset();
        this.load();
      },
      error: () => {
        this.assigning = false;
        this.toast.error('No se pudo asignar el PACE.');
      }
    });
  }

  updateMeta(meta: Meta, estado: EstadoMeta): void {
    this.progresoService.actualizarEstadoMeta(meta.id, { estado }).subscribe({
      next: () => {
        this.toast.success('Meta actualizada.');
        this.load();
      },
      error: () => this.toast.error('No se pudo actualizar la meta.')
    });
  }

  scoreMeta(meta: Meta): void {
    const puntajeObtenido = this.scoreInputs[meta.id];
    if (puntajeObtenido == null) return;

    this.progresoService.actualizarEstadoMeta(meta.id, {
      estado: 'Scored',
      puntajeObtenido
    }).subscribe({
      next: () => {
        this.scoreInputs[meta.id] = null;
        this.toast.success('Score registrado.');
        this.load();
      },
      error: () => this.toast.error('No se pudo registrar el score.')
    });
  }

  setScore(metaId: string, event: Event): void {
    const value = Number((event.target as HTMLInputElement).value);
    this.scoreInputs[metaId] = Number.isFinite(value) ? value : null;
  }

  initials(alumno: Alumno): string {
    return `${alumno.nombre.charAt(0)}${alumno.apellido.charAt(0)}`.toUpperCase();
  }

  privilegeItems(alumno: Alumno): Array<{ flag: PrivilegioFlag; label: string; active: boolean; mode: ModoPrivilegio }> {
    const status = alumno.privilegeStatus;
    const manuales = alumno.privilegiosManuales;

    const item = (flag: PrivilegioFlag, active: boolean, manual: boolean | null | undefined) => ({
      flag,
      label: flag,
      active,
      mode: this.modoDe(manual)
    });

    return [
      item('Oficina', status?.oficina ?? alumno.privilegiosActivos?.includes('Oficina') ?? false, manuales?.oficina),
      item('Comedor', status?.comedor ?? alumno.privilegiosActivos?.includes('Comedor') ?? false, manuales?.comedor),
      item('Patio', status?.patio ?? alumno.privilegiosActivos?.includes('Patio') ?? false, manuales?.patio),
      item('Biblioteca', status?.biblioteca ?? alumno.privilegiosActivos?.includes('Biblioteca') ?? false, manuales?.biblioteca),
      item('Actividades', status?.actividades ?? alumno.privilegiosActivos?.includes('Actividades') ?? false, manuales?.actividades)
    ];
  }

  /** Ausente o nulo significa que el privilegio lo sigue decidiendo el balance. */
  private modoDe(manual: boolean | null | undefined): ModoPrivilegio {
    if (manual === true) return 'activo';
    if (manual === false) return 'inactivo';
    return 'auto';
  }

  setPrivilege(flag: PrivilegioFlag, event: Event): void {
    if (!this.alumno) return;

    const modo = (event.target as HTMLSelectElement).value as ModoPrivilegio;
    const activo = modo === 'auto' ? null : modo === 'activo';

    this.savingPrivilege = flag;
    this.alumnosService.actualizarPrivilegio(this.alumno.id, flag, activo).subscribe({
      next: response => {
        this.savingPrivilege = null;
        this.toast.success(response?.message ?? 'Privilegio actualizado.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.savingPrivilege = null;
        this.toast.error(this.describeError(error, 'No se pudo actualizar el privilegio.'));
        // La lista se recarga para que el select no quede mostrando un modo
        // que el backend nunca llegó a aceptar.
        this.load();
      }
    });
  }

  balanceVariant(balance: number): BadgeVariant {
    if (balance < 0) return 'red';
    if (balance === 0) return 'gray';
    return 'green';
  }

  paceVariant(estado: string): BadgeVariant {
    if (estado === 'Completado') return 'green';
    if (estado === 'Fallido' || estado === 'AutoTestFallido') return 'red';
    if (estado === 'ListoParaAutoTest' || estado === 'AutoTestOk' || estado === 'EnTestFinal') return 'orange';
    return 'blue';
  }

  metaVariant(estado: EstadoMeta): BadgeVariant {
    if (estado === 'Aprobada' || estado === 'Completada') return 'green';
    if (estado === 'Rechazada') return 'red';
    if (estado === 'Scored') return 'blue';
    return 'orange';
  }

  paceProgress(pace: AlumnoPace): number {
    const progressByState: Record<string, number> = {
      Asignado: 10,
      EnProgreso: 35,
      ListoParaAutoTest: 60,
      AutoTestOk: 72,
      AutoTestFallido: 50,
      EnTestFinal: 88,
      Completado: 100,
      Fallido: 100
    };
    return progressByState[pace.estado] ?? 0;
  }

  private optional<T>(source: Observable<T>, fallback: T, onError: (message: string) => void): Observable<T> {
    return source.pipe(
      catchError((error: HttpErrorResponse) => {
        onError(this.describeError(error, 'No se pudo cargar esta sección.'));
        return of(fallback);
      })
    );
  }

  private describeError(error: HttpErrorResponse, fallback: string): string {
    const message = error?.error?.message;
    return typeof message === 'string' && message.trim() ? message : fallback;
  }

  private today(): string {
    return todayIso();
  }

  private weekStart(): string {
    return weekStartIso();
  }
}

/** Un bloque semanal de la bitácora de méritos y deméritos. */
interface SemanaDeMeritos {
  /** Lunes de la semana, en formato `yyyy-MM-dd`. Ordena los grupos. */
  inicio: string;
  etiqueta: string;
  /** Neto de la semana, ya sin los registros eliminados. */
  balance: number;
  registros: Merito[];
}
