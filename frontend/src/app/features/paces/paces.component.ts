import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';
import { AlumnosService } from '../../core/services/alumnos.service';
import { AuthService } from '../../core/services/auth.service';
import { PacesService } from '../../core/services/paces.service';
import { ToastService } from '../../core/services/toast.service';
import { Alumno, AlumnoPace, EstadoAlumnoPace, Pace, Rol } from '../../models';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-paces',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, BadgeComponent, ButtonComponent, SpinnerComponent],
  template: `
    <section class="page-shell">
      <header class="topbar">
        <div>
          <p class="eyebrow">Módulo 3</p>
          <h1>PACEs y progreso académico</h1>
        </div>
        <app-button variant="ghost" (click)="load()">Actualizar</app-button>
      </header>

      <div class="state" *ngIf="loading">
        <app-spinner variant="overlay" />
      </div>

      <div class="state error" *ngIf="!loading && errorMessage">
        <p>{{ errorMessage }}</p>
        <app-button (click)="load()">Reintentar</app-button>
      </div>

      <ng-container *ngIf="!loading && !errorMessage">
        <section class="panel" *ngIf="canAssign">
          <h2>Asignar PACE a un alumno</h2>
          <form [formGroup]="assignForm" (ngSubmit)="asignar()" class="form-grid">
            <label>
              <span>Alumno</span>
              <select formControlName="alumnoId">
                <option value="">Selecciona un alumno</option>
                <option *ngFor="let alumno of alumnos" [value]="alumno.id">
                  {{ alumno.nombre }} {{ alumno.apellido }} — {{ alumno.nivel }}
                </option>
              </select>
            </label>
            <label>
              <span>PACE</span>
              <select formControlName="paceId">
                <option value="">Selecciona un PACE</option>
                <option *ngFor="let pace of catalogo" [value]="pace.id">
                  {{ pace.materia }} · PACE {{ pace.numeroPace }}
                </option>
              </select>
            </label>
            <div class="actions">
              <app-button [loading]="assigning" [disabled]="assignForm.invalid">Asignar</app-button>
            </div>
          </form>
        </section>

        <section class="panel">
          <div class="section-header">
            <h2>Progreso por alumno</h2>
          </div>

          <label class="single">
            <span>Alumno</span>
            <select [(ngModel)]="alumnoSeleccionado" [ngModelOptions]="{ standalone: true }" (ngModelChange)="cargarProgreso()">
              <option value="">Selecciona un alumno</option>
              <option *ngFor="let alumno of alumnos" [value]="alumno.id">
                {{ alumno.nombre }} {{ alumno.apellido }}
              </option>
            </select>
          </label>

          <div class="empty" *ngIf="!alumnoSeleccionado">
            Selecciona un alumno para ver sus PACEs en curso y su historial.
          </div>

          <div class="state" *ngIf="alumnoSeleccionado && loadingProgreso">
            <app-spinner variant="overlay" />
          </div>

          <div class="section-error" *ngIf="progresoError">{{ progresoError }}</div>

          <ng-container *ngIf="alumnoSeleccionado && !loadingProgreso && !progresoError">
            <h3>En curso <app-badge variant="blue">{{ enCurso.length }}</app-badge></h3>
            <div class="empty" *ngIf="enCurso.length === 0">Sin PACEs en curso.</div>
            <article class="pace-row" *ngFor="let pace of enCurso">
              <div>
                <strong>{{ pace.materia }} · PACE {{ pace.numeroPace }}</strong>
                <span class="meta">Inicio {{ pace.fechaInicio | date:'dd/MM/yyyy' }}</span>
              </div>
              <app-badge [variant]="estadoVariant(pace.estado)">{{ etiquetaEstado(pace.estado) }}</app-badge>
            </article>

            <h3>Historial de completados <app-badge variant="gray">{{ completados.length }}</app-badge></h3>
            <div class="empty" *ngIf="completados.length === 0">Aún no hay PACEs completados.</div>
            <article class="pace-row" *ngFor="let pace of completados">
              <div>
                <strong>{{ pace.materia }} · PACE {{ pace.numeroPace }}</strong>
                <span class="meta">
                  Completado {{ pace.fechaCompletado | date:'dd/MM/yyyy' }}
                  <ng-container *ngIf="pace.puntajeFinal != null">
                    · {{ pace.puntajeFinal }} / {{ pace.puntajeMaximo ?? 100 }}
                  </ng-container>
                </span>
              </div>
              <app-badge [variant]="estadoVariant(pace.estado)">{{ etiquetaEstado(pace.estado) }}</app-badge>
            </article>

            <a class="link" [routerLink]="['/alumno', alumnoSeleccionado]">Abrir perfil del alumno</a>
          </ng-container>
        </section>

        <section class="panel">
          <div class="section-header">
            <h2>Catálogo de PACEs</h2>
            <app-badge variant="gray">{{ catalogoVisible.length }} de {{ catalogo.length }}</app-badge>
          </div>

          <label class="single">
            <span>Materia</span>
            <select [(ngModel)]="materiaFiltro" [ngModelOptions]="{ standalone: true }">
              <option value="">Todas</option>
              <option *ngFor="let materia of materias" [value]="materia">{{ materia }}</option>
            </select>
          </label>

          <form *ngIf="canCreate" [formGroup]="paceForm" (ngSubmit)="crearPace()" class="form-grid">
            <label>
              <span>Materia</span>
              <input type="text" formControlName="materia" placeholder="Math" list="materias" />
              <datalist id="materias">
                <option *ngFor="let materia of materias" [value]="materia"></option>
              </datalist>
            </label>
            <label>
              <span>Número de PACE</span>
              <input
                type="text"
                formControlName="numeroPace"
                maxlength="20"
                placeholder="1045 o RR01"
                autocapitalize="characters"
              />
              <small>Letras y dígitos, sin espacios ni signos.</small>
            </label>
            <label>
              <span>Puntaje máximo</span>
              <input type="number" formControlName="puntajeMaximo" min="1" />
            </label>
            <label>
              <span>Mínimo de aprobación</span>
              <input type="number" formControlName="puntajeMinimoAprobacion" min="0" />
            </label>
            <label>
              <span>Total de páginas</span>
              <input type="number" formControlName="totalPaginas" min="1" placeholder="Opcional" />
              <small>Si lo capturas, el sistema valida que el rango de una meta quepa en el PACE.</small>
            </label>
            <div class="actions">
              <app-button [loading]="creating" [disabled]="paceForm.invalid">Agregar al catálogo</app-button>
            </div>
          </form>

          <div class="empty" *ngIf="catalogoVisible.length === 0">No hay PACEs en el catálogo.</div>

          <table *ngIf="catalogoVisible.length > 0">
            <thead>
              <tr>
                <th scope="col">Materia</th>
                <th scope="col">PACE</th>
                <th scope="col">Puntaje máximo</th>
                <th scope="col">Mínimo de aprobación</th>
                <th scope="col">Páginas</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let pace of catalogoVisible">
                <th scope="row">{{ pace.materia }}</th>
                <td>{{ pace.numeroPace }}</td>
                <td>{{ pace.puntajeMaximo }}</td>
                <td>{{ pace.puntajeMinimoAprobacion ?? '—' }}</td>
                <td>{{ pace.totalPaginas ?? '—' }}</td>
              </tr>
            </tbody>
          </table>
        </section>
      </ng-container>
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

    h3 {
      display: flex;
      align-items: center;
      gap: var(--space-2);
      margin: 0;
      font-size: var(--font-size-md, 1rem);
    }

    .eyebrow {
      margin: 0;
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      text-transform: uppercase;
      letter-spacing: 0.06em;
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

    .section-header {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }

    .form-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: var(--space-4);
      align-items: end;
    }

    label {
      display: grid;
      gap: var(--space-2);
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
    }

    label.single {
      max-width: 320px;
    }

    input,
    select {
      width: 100%;
      min-height: var(--tap-target-min, 44px);
      padding: 0 var(--space-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
      font: inherit;
    }

    .actions {
      grid-column: 1 / -1;
      display: flex;
      justify-content: flex-end;
    }

    .pace-row {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: var(--space-3);
      padding: var(--space-3) var(--space-4);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
    }

    .pace-row > div {
      display: grid;
      gap: var(--space-1);
    }

    .meta {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
    }

    .link {
      justify-self: start;
      color: var(--color-primary);
      font-weight: var(--font-weight-semibold);
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

    .empty,
    .state {
      display: grid;
      place-items: center;
      min-height: 120px;
      padding: var(--space-5);
      border: 1px dashed var(--color-border);
      border-radius: var(--radius-md);
      color: var(--color-text-secondary);
      background: var(--color-bg-surface);
    }

    .state.error,
    .section-error {
      gap: var(--space-4);
      padding: var(--space-4);
      border: 1px solid var(--color-danger);
      border-radius: var(--radius-md);
      color: var(--color-danger);
      background: var(--color-danger-bg);
    }
  `]
})
export class PacesComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly pacesService = inject(PacesService);
  private readonly alumnosService = inject(AlumnosService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  private readonly role: Rol | null = this.auth.getRole();

  alumnos: Alumno[] = [];
  catalogo: Pace[] = [];
  progreso: AlumnoPace[] = [];

  loading = true;
  loadingProgreso = false;
  assigning = false;
  creating = false;
  errorMessage = '';
  progresoError = '';
  materiaFiltro = '';
  alumnoSeleccionado = '';

  readonly assignForm = this.fb.nonNullable.group({
    alumnoId: ['', Validators.required],
    paceId: ['', Validators.required]
  });

  readonly paceForm = this.fb.nonNullable.group({
    materia: ['', Validators.required],
    // El mismo formato que valida el dominio, para avisar antes de enviar.
    numeroPace: ['', [Validators.required, Validators.pattern(/^[A-Za-z0-9]{1,20}$/)]],
    puntajeMaximo: [100, [Validators.required, Validators.min(1)]],
    puntajeMinimoAprobacion: [80, [Validators.required, Validators.min(0)]],
    // Opcional: los PACEs ya capturados no lo tienen y no debe volverse obligatorio
    // de golpe para quien solo quiere dar de alta uno nuevo (issue #6).
    totalPaginas: [null as number | null]
  });

  ngOnInit(): void {
    this.load();
  }

  get canAssign(): boolean {
    return this.role === 'Principal' || this.role === 'Supervisora';
  }

  get canCreate(): boolean {
    return this.role === 'Principal';
  }

  get materias(): string[] {
    return [...new Set(this.catalogo.map(pace => pace.materia).filter(Boolean))].sort();
  }

  get catalogoVisible(): Pace[] {
    const filtrados = this.materiaFiltro
      ? this.catalogo.filter(pace => pace.materia === this.materiaFiltro)
      : this.catalogo;
    // El número dejó de ser entero, así que se compara como texto. Entre
    // códigos del mismo largo el resultado es idéntico al orden numérico.
    return [...filtrados].sort(
      (a, b) => a.materia.localeCompare(b.materia) || a.numeroPace.localeCompare(b.numeroPace)
    );
  }

  get enCurso(): AlumnoPace[] {
    return this.progreso.filter(pace => pace.estado !== 'Completado' && pace.estado !== 'Fallido');
  }

  get completados(): AlumnoPace[] {
    return this.progreso
      .filter(pace => pace.estado === 'Completado' || pace.estado === 'Fallido')
      .sort((a, b) => (b.fechaCompletado ?? '').localeCompare(a.fechaCompletado ?? ''));
  }

  etiquetaEstado(estado: EstadoAlumnoPace): string {
    const etiquetas: Record<EstadoAlumnoPace, string> = {
      Asignado: 'Asignado',
      EnProgreso: 'En progreso',
      ListoParaAutoTest: 'Listo para Score Station',
      AutoTestOk: 'Auto-test aprobado',
      AutoTestFallido: 'Auto-test fallido',
      EnTestFinal: 'En test final',
      Completado: 'Completado',
      Fallido: 'Fallido'
    };
    return etiquetas[estado] ?? estado;
  }

  estadoVariant(estado: EstadoAlumnoPace): BadgeVariant {
    if (estado === 'Completado' || estado === 'AutoTestOk') return 'green';
    if (estado === 'Fallido' || estado === 'AutoTestFallido') return 'red';
    if (estado === 'ListoParaAutoTest' || estado === 'EnTestFinal') return 'orange';
    return 'blue';
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      catalogo: this.pacesService.getCatalogo(),
      alumnos: this.alumnosService.getAll()
    }).subscribe({
      next: resultado => {
        this.catalogo = resultado.catalogo;
        this.alumnos = resultado.alumnos;
        this.loading = false;
        if (this.alumnoSeleccionado) this.cargarProgreso();
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = error?.error?.message ?? 'No se pudo cargar el catálogo de PACEs.';
      }
    });
  }

  cargarProgreso(): void {
    this.progreso = [];
    this.progresoError = '';
    if (!this.alumnoSeleccionado) return;

    this.loadingProgreso = true;
    this.pacesService.getByAlumno(this.alumnoSeleccionado).subscribe({
      next: progreso => {
        this.progreso = progreso;
        this.loadingProgreso = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loadingProgreso = false;
        this.progresoError = error?.error?.message ?? 'No se pudo cargar el progreso del alumno.';
      }
    });
  }

  asignar(): void {
    if (this.assignForm.invalid) {
      this.assignForm.markAllAsTouched();
      return;
    }

    this.assigning = true;
    const { alumnoId, paceId } = this.assignForm.getRawValue();

    this.pacesService.asignar(alumnoId, paceId).subscribe({
      next: () => {
        this.assigning = false;
        this.assignForm.reset({ alumnoId: '', paceId: '' });
        this.toast.success('PACE asignado.');
        if (this.alumnoSeleccionado === alumnoId) this.cargarProgreso();
      },
      error: (error: HttpErrorResponse) => {
        this.assigning = false;
        this.toast.error(error?.error?.message ?? 'No se pudo asignar el PACE.');
      }
    });
  }

  crearPace(): void {
    if (this.paceForm.invalid) {
      this.paceForm.markAllAsTouched();
      return;
    }

    this.creating = true;
    const { totalPaginas, ...pace } = this.paceForm.getRawValue();
    // Vacío se manda ausente; el backend interpreta null como "sin capturar".
    const payload = totalPaginas ? { ...pace, totalPaginas } : pace;

    this.pacesService.create(payload).subscribe({
      next: () => {
        this.creating = false;
        this.paceForm.reset({ materia: '', numeroPace: '', puntajeMaximo: 100, puntajeMinimoAprobacion: 80, totalPaginas: null });
        this.toast.success('PACE agregado al catálogo.');
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.creating = false;
        this.toast.error(error?.error?.message ?? 'No se pudo agregar el PACE.');
      }
    });
  }
}
