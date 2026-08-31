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
import { Alumno, AlumnoPace, EstadoMeta, Merito, Meta, Pace, Rol } from '../../models';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { ProgressBarComponent } from '../../shared/components/progress-bar/progress-bar.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';
import { QuickActionsComponent } from './quick-actions.component';

@Component({
  selector: 'app-alumno',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink, BadgeComponent, ButtonComponent, ProgressBarComponent, SpinnerComponent, QuickActionsComponent],
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

            <article class="merit-row" *ngFor="let merito of meritos.slice(0, 10)">
              <div>
                <strong>{{ merito.tipo }} · {{ merito.puntos }} puntos</strong>
                <span>{{ merito.motivo }}</span>
                <small>{{ merito.fechaAplicado | date:'dd/MM/yyyy HH:mm' }} · {{ merito.staffName ?? 'Personal Escolar' }}</small>
              </div>
              <app-badge [variant]="merito.revocado ? 'gray' : merito.tipo === 'Merito' ? 'green' : 'red'">
                {{ merito.revocado ? 'Revocado' : merito.tipo }}
              </app-badge>
            </article>
          </section>
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
  scoreInputs: Record<string, number | null> = {};

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

  privilegeItems(alumno: Alumno): Array<{ label: string; active: boolean }> {
    const status = alumno.privilegeStatus;
    return [
      { label: 'Oficina', active: status?.oficina ?? alumno.privilegiosActivos?.includes('Oficina') ?? false },
      { label: 'Comedor', active: status?.comedor ?? alumno.privilegiosActivos?.includes('Comedor') ?? false },
      { label: 'Patio', active: status?.patio ?? alumno.privilegiosActivos?.includes('Patio') ?? false },
      { label: 'Biblioteca', active: status?.biblioteca ?? alumno.privilegiosActivos?.includes('Biblioteca') ?? false },
      { label: 'Actividades', active: status?.actividades ?? alumno.privilegiosActivos?.includes('Actividades') ?? false }
    ];
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
