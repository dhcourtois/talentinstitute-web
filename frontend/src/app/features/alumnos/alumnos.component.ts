import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AlumnosService } from '../../core/services/alumnos.service';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { todayIso } from '../../core/utils/fecha.util';
import { Alumno, PrivilegioFlag, Rol } from '../../models';
import { BadgeComponent, BadgeVariant } from '../../shared/components/badge/badge.component';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { SpinnerComponent } from '../../shared/components/spinner/spinner.component';

@Component({
  selector: 'app-alumnos',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterLink, BadgeComponent, ButtonComponent, SpinnerComponent],
  template: `
    <section class="page-shell">
      <header class="topbar">
        <div>
          <p class="eyebrow">Módulo 2</p>
          <h1>Gestión de alumnos</h1>
        </div>
        <div class="topbar-actions">
          <app-button variant="ghost" (click)="load()">Actualizar</app-button>
          <app-button variant="secondary" [disabled]="visibles.length === 0" (click)="exportCsv()">
            Exportar CSV
          </app-button>
          <app-button variant="secondary" [disabled]="visibles.length === 0" (click)="print()">
            Imprimir
          </app-button>
        </div>
      </header>

      <section class="panel no-print" *ngIf="canManage">
        <h2>{{ editandoId ? 'Editar alumno' : 'Registrar alumno' }}</h2>
        <form [formGroup]="form" (ngSubmit)="save()" class="form-grid">
          <label>
            <span>Matrícula</span>
            <input
              type="text"
              formControlName="numeroMatricula"
              placeholder="MAT-001"
              [attr.aria-describedby]="editandoId ? 'matricula-fija' : null"
            />
            <small id="matricula-fija" *ngIf="editandoId">La matrícula no se modifica.</small>
          </label>
          <label>
            <span>Nombre</span>
            <input type="text" formControlName="nombre" />
          </label>
          <label>
            <span>Apellido</span>
            <input type="text" formControlName="apellido" />
          </label>
          <label>
            <span>Nivel</span>
            <input type="text" formControlName="nivel" placeholder="1 Primaria" list="niveles" />
            <datalist id="niveles">
              <option *ngFor="let nivel of niveles" [value]="nivel"></option>
            </datalist>
          </label>
          <label>
            <span>Fecha de ingreso</span>
            <input type="date" formControlName="fechaIngreso" aria-describedby="ingreso-ayuda" />
            <small id="ingreso-ayuda">
              {{ editandoId ? 'Puedes corregirla si el alumno ingresó antes.' : 'Si la dejas vacía se toma la fecha de hoy.' }}
            </small>
          </label>
          <div class="actions">
            <app-button *ngIf="editandoId" variant="ghost" type="button" (click)="cancelEdit()">
              Cancelar
            </app-button>
            <app-button [loading]="saving" [disabled]="form.invalid">
              {{ editandoId ? 'Guardar cambios' : 'Registrar' }}
            </app-button>
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
        <div class="filters no-print">
          <label>
            <span>Buscar</span>
            <input
              type="search"
              [(ngModel)]="busqueda"
              [ngModelOptions]="{ standalone: true }"
              placeholder="Nombre o matrícula"
            />
          </label>
          <label>
            <span>Nivel</span>
            <select [(ngModel)]="nivelFiltro" [ngModelOptions]="{ standalone: true }">
              <option value="">Todos</option>
              <option *ngFor="let nivel of niveles" [value]="nivel">{{ nivel }}</option>
            </select>
          </label>
          <app-badge variant="gray">{{ visibles.length }} de {{ alumnos.length }}</app-badge>
        </div>

        <div class="empty" *ngIf="visibles.length === 0">No hay alumnos que coincidan con el filtro.</div>

        <table *ngIf="visibles.length > 0">
          <caption>Alumnos — {{ hoy | date:'dd/MM/yyyy' }}</caption>
          <thead>
            <tr>
              <th scope="col">Alumno</th>
              <th scope="col">Nivel</th>
              <th scope="col">Ingreso</th>
              <th scope="col">Balance</th>
              <th scope="col">Privilegios activos</th>
              <th scope="col" class="no-print"><span class="sr-only">Acciones</span></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let alumno of visibles">
              <th scope="row">
                <strong>{{ alumno.nombre }} {{ alumno.apellido }}</strong>
                <span class="meta">{{ alumno.numeroMatricula }}</span>
              </th>
              <td>{{ alumno.nivel }}</td>
              <td>{{ (alumno.fechaIngreso | date:'dd/MM/yyyy') || '—' }}</td>
              <td>
                <app-badge [variant]="balanceVariant(alumno.balanceMeritos ?? 0)">
                  {{ alumno.balanceMeritos ?? 0 }}
                </app-badge>
              </td>
              <td>{{ privilegiosActivos(alumno).join(', ') || 'Sin privilegios' }}</td>
              <td class="row-actions no-print">
                <a [routerLink]="['/alumno', alumno.id]">Abrir</a>
                <button type="button" *ngIf="canManage" (click)="startEdit(alumno)">Editar</button>
              </td>
            </tr>
          </tbody>
        </table>
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
      flex-wrap: wrap;
    }

    .topbar h1,
    .panel h2 {
      margin: 0;
    }

    .topbar-actions {
      display: flex;
      gap: var(--space-2);
      flex-wrap: wrap;
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

    .form-grid,
    .filters {
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

    small {
      color: var(--color-text-secondary);
      font-weight: var(--font-weight-regular, 400);
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
      gap: var(--space-2);
    }

    table {
      width: 100%;
      border-collapse: collapse;
    }

    caption {
      margin-bottom: var(--space-3);
      color: var(--color-text-secondary);
      text-align: left;
    }

    th, td {
      padding: var(--space-3);
      text-align: left;
      border-bottom: 1px solid var(--color-border);
      vertical-align: top;
    }

    thead th {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      text-transform: uppercase;
      letter-spacing: 0.04em;
    }

    tbody th strong {
      display: block;
    }

    .meta {
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
    }

    .row-actions {
      display: flex;
      gap: var(--space-2);
      justify-content: flex-end;
    }

    .row-actions a,
    .row-actions button {
      display: inline-flex;
      align-items: center;
      min-height: var(--tap-target-min, 44px);
      padding: 0 var(--space-3);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
      font-weight: var(--font-weight-semibold);
      text-decoration: none;
      cursor: pointer;
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

    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      overflow: hidden;
      clip: rect(0 0 0 0);
      white-space: nowrap;
    }

    @media print {
      .no-print {
        display: none !important;
      }

      .page-shell {
        padding: 0;
      }

      .panel {
        border: none;
        padding: 0;
      }
    }
  `]
})
export class AlumnosComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly service = inject(AlumnosService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  private readonly role: Rol | null = this.auth.getRole();

  alumnos: Alumno[] = [];
  loading = true;
  saving = false;
  errorMessage = '';
  busqueda = '';
  nivelFiltro = '';
  editandoId: string | null = null;
  readonly hoy = new Date();

  readonly form = this.fb.nonNullable.group({
    numeroMatricula: ['', [Validators.required, Validators.minLength(3)]],
    nombre: ['', [Validators.required, Validators.minLength(2)]],
    apellido: ['', [Validators.required, Validators.minLength(2)]],
    nivel: ['', Validators.required],
    // Opcional: al registrar, vacía significa "hoy"; al editar se precarga con
    // la fecha vigente para que guardar sin tocarla no la mueva (issue #22).
    fechaIngreso: ['']
  });

  ngOnInit(): void {
    this.load();
  }

  get canManage(): boolean {
    return this.role === 'Principal' || this.role === 'Supervisora';
  }

  get niveles(): string[] {
    return [...new Set(this.alumnos.map(alumno => alumno.nivel).filter(Boolean))].sort();
  }

  get visibles(): Alumno[] {
    const termino = this.busqueda.trim().toLowerCase();
    return this.alumnos.filter(alumno => {
      const coincideNivel = !this.nivelFiltro || alumno.nivel === this.nivelFiltro;
      const texto = `${alumno.nombre} ${alumno.apellido} ${alumno.numeroMatricula}`.toLowerCase();
      return coincideNivel && (!termino || texto.includes(termino));
    });
  }

  balanceVariant(balance: number): BadgeVariant {
    if (balance > 0) return 'green';
    return balance < 0 ? 'red' : 'gray';
  }

  privilegiosActivos(alumno: Alumno): PrivilegioFlag[] {
    if (alumno.privilegiosActivos?.length) {
      return alumno.privilegiosActivos;
    }

    const status = alumno.privilegeStatus;
    if (!status) return [];

    const activos: PrivilegioFlag[] = [];
    if (status.oficina) activos.push('Oficina');
    if (status.comedor) activos.push('Comedor');
    if (status.patio) activos.push('Patio');
    if (status.biblioteca) activos.push('Biblioteca');
    if (status.actividades) activos.push('Actividades');
    return activos;
  }

  load(): void {
    this.loading = true;
    this.errorMessage = '';

    this.service.getAll().subscribe({
      next: alumnos => {
        this.alumnos = alumnos;
        this.loading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = error?.error?.message ?? 'No se pudo cargar la lista de alumnos.';
      }
    });
  }

  startEdit(alumno: Alumno): void {
    this.editandoId = alumno.id;
    this.form.patchValue({
      numeroMatricula: alumno.numeroMatricula,
      nombre: alumno.nombre,
      apellido: alumno.apellido,
      nivel: alumno.nivel,
      fechaIngreso: this.fechaParaInput(alumno.fechaIngreso)
    });
    this.form.controls.numeroMatricula.disable();
  }

  cancelEdit(): void {
    this.editandoId = null;
    this.form.controls.numeroMatricula.enable();
    this.form.reset({ numeroMatricula: '', nombre: '', apellido: '', nivel: '', fechaIngreso: '' });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    const { numeroMatricula, nombre, apellido, nivel, fechaIngreso } = this.form.getRawValue();
    // Vacía se manda como ausente: el backend interpreta null como "no la toques".
    const ingreso = fechaIngreso ? { fechaIngreso } : {};

    const peticion = this.editandoId
      ? this.service.update(this.editandoId, { nombre, apellido, nivel, ...ingreso })
      : this.service.create({ numeroMatricula, nombre, apellido, nivel, ...ingreso });

    peticion.subscribe({
      next: response => {
        this.saving = false;
        this.toast.success(response?.message ?? 'Alumno guardado.');
        this.cancelEdit();
        this.load();
      },
      error: (error: HttpErrorResponse) => {
        this.saving = false;
        this.toast.error(error?.error?.message ?? 'No se pudo guardar el alumno.');
      }
    });
  }

  /** El input[type=date] solo acepta `yyyy-MM-dd`; el backend manda ISO completo. */
  private fechaParaInput(fecha?: string): string {
    return fecha ? fecha.slice(0, 10) : '';
  }

  print(): void {
    window.print();
  }

  exportCsv(): void {
    const encabezados = ['Matrícula', 'Nombre', 'Apellido', 'Nivel', 'Ingreso', 'Balance', 'Privilegios activos'];
    const filas = this.visibles.map(alumno => [
      alumno.numeroMatricula,
      alumno.nombre,
      alumno.apellido,
      alumno.nivel,
      this.fechaParaInput(alumno.fechaIngreso),
      `${alumno.balanceMeritos ?? 0}`,
      this.privilegiosActivos(alumno).join(' / ')
    ]);

    const csv = [encabezados, ...filas]
      .map(fila => fila.map(celda => `"${`${celda}`.replace(/"/g, '""')}"`).join(','))
      .join('\r\n');

    // BOM para que Excel respete los acentos.
    const blob = new Blob([`﻿${csv}`], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const enlace = document.createElement('a');
    enlace.href = url;
    enlace.download = `alumnos-${todayIso(this.hoy)}.csv`;
    enlace.click();
    URL.revokeObjectURL(url);
  }
}
