import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MeritosService } from '../../core/services/meritos.service';
import { PacesService } from '../../core/services/paces.service';
import { ToastService } from '../../core/services/toast.service';
import { AlumnoPace, Rol } from '../../models';
import { ButtonComponent } from '../../shared/components/button/button.component';
import { ModalComponent } from '../../shared/components/modal/modal.component';

type ActiveModal = 'merito' | 'demerito' | 'score' | null;

/**
 * Panel de acciones rápidas del perfil de alumno.
 *
 * Muestra hasta 3 botones según el rol:
 *   - Otorgar mérito    → cualquier rol autenticado
 *   - Registrar demérito → cualquier rol autenticado
 *   - Actualizar puntos  → solo Principal y Supervisora
 *
 * Cada botón abre un ModalComponent con un formulario mínimo.
 * Al completar satisfactoriamente emite `(changed)` para que el
 * componente padre recargue el perfil.
 */
@Component({
  selector: 'app-quick-actions',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent, ModalComponent],
  template: `
    <!-- ── Action bar ──────────────────────────────────────────── -->
    <aside class="qa-bar" aria-label="Acciones rápidas">
      <span class="qa-label">Acciones rápidas</span>

      <div class="qa-buttons">
        <app-button variant="primary" icon="+" (click)="open('merito')">
          Mérito
        </app-button>

        <app-button variant="danger" (click)="open('demerito')">
          Demérito
        </app-button>

        <app-button
          *ngIf="canScore"
          variant="secondary"
          (click)="open('score')"
        >
          Actualizar puntos
        </app-button>
      </div>
    </aside>

    <!-- ── Modal: Otorgar mérito ──────────────────────────────── -->
    <app-modal
      [visible]="activeModal === 'merito'"
      title="Otorgar mérito"
      confirmLabel="Confirmar mérito"
      confirmVariant="primary"
      [confirmLoading]="submitting"
      (confirmed)="submitMerito()"
      (cancelled)="close()"
    >
      <form [formGroup]="meritoForm" class="modal-form" (ngSubmit)="submitMerito()">
        <label>
          <span>Puntos</span>
          <input type="number" min="1" max="100" formControlName="puntos" />
          <span class="field-error" *ngIf="meritoForm.controls.puntos.touched && meritoForm.controls.puntos.invalid">
            Ingresa entre 1 y 100 puntos.
          </span>
        </label>
        <label class="wide">
          <span>Motivo</span>
          <input type="text" formControlName="motivo" placeholder="Ej. Completó meta antes de tiempo" />
          <span class="field-error" *ngIf="meritoForm.controls.motivo.touched && meritoForm.controls.motivo.invalid">
            El motivo es obligatorio (mínimo 3 caracteres).
          </span>
        </label>
      </form>
    </app-modal>

    <!-- ── Modal: Registrar demérito ─────────────────────────── -->
    <app-modal
      [visible]="activeModal === 'demerito'"
      title="Registrar demérito"
      confirmLabel="Confirmar demérito"
      confirmVariant="danger"
      [confirmLoading]="submitting"
      (confirmed)="submitDemerito()"
      (cancelled)="close()"
    >
      <form [formGroup]="demeritoForm" class="modal-form" (ngSubmit)="submitDemerito()">
        <label>
          <span>Puntos</span>
          <input type="number" min="1" max="100" formControlName="puntos" />
          <span class="field-error" *ngIf="demeritoForm.controls.puntos.touched && demeritoForm.controls.puntos.invalid">
            Ingresa entre 1 y 100 puntos.
          </span>
        </label>
        <label class="wide">
          <span>Motivo</span>
          <input type="text" formControlName="motivo" placeholder="Ej. Interrumpió la clase" />
          <span class="field-error" *ngIf="demeritoForm.controls.motivo.touched && demeritoForm.controls.motivo.invalid">
            El motivo es obligatorio (mínimo 3 caracteres).
          </span>
        </label>
      </form>
    </app-modal>

    <!-- ── Modal: Actualizar puntos Score Station ─────────────── -->
    <app-modal
      *ngIf="canScore"
      [visible]="activeModal === 'score'"
      title="Actualizar puntos — Score Station"
      confirmLabel="Guardar puntaje"
      confirmVariant="primary"
      [confirmLoading]="submitting"
      (confirmed)="submitScore()"
      (cancelled)="close()"
    >
      <form [formGroup]="scoreForm" class="modal-form" (ngSubmit)="submitScore()">
        <label class="wide">
          <span>PACE</span>
          <select formControlName="alumnoPaceId">
            <option value="">Selecciona un PACE</option>
            <option *ngFor="let p of scorablePaces" [value]="p.id">
              {{ p.materia ?? p.pace?.materia }} {{ p.numeroPace ?? p.pace?.numeroPace }} — {{ p.estado }}
            </option>
          </select>
          <span class="field-error" *ngIf="scoreForm.controls.alumnoPaceId.touched && scoreForm.controls.alumnoPaceId.invalid">
            Selecciona un PACE.
          </span>
        </label>
        <label>
          <span>Puntaje obtenido (0–100)</span>
          <input type="number" min="0" max="100" formControlName="puntaje" />
          <span class="field-error" *ngIf="scoreForm.controls.puntaje.touched && scoreForm.controls.puntaje.invalid">
            Ingresa un puntaje válido (0–100).
          </span>
        </label>
        <label>
          <span>Resultado</span>
          <select formControlName="exitoso">
            <option [ngValue]="true">✅ Aprobado</option>
            <option [ngValue]="false">❌ Reprobado</option>
          </select>
        </label>
      </form>
    </app-modal>
  `,
  styles: [`
    :host { display: block; }

    /* ── Action bar ──────────────────────────────────────────── */
    .qa-bar {
      display: flex;
      align-items: center;
      gap: var(--space-4);
      padding: var(--space-4) var(--space-5);
      background: var(--color-bg-surface);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-card);
      flex-wrap: wrap;
    }

    .qa-label {
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-secondary);
      flex-shrink: 0;
      margin-right: var(--space-2);
    }

    .qa-buttons {
      display: flex;
      gap: var(--space-3);
      flex-wrap: wrap;
    }

    /* ── Modal form layout ───────────────────────────────────── */
    .modal-form {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--space-4);
    }

    label {
      display: grid;
      gap: var(--space-2);
      font-size: var(--font-size-sm);
      font-weight: var(--font-weight-semibold);
      color: var(--color-text-secondary);
    }

    label.wide {
      grid-column: span 2;
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
      font-size: var(--font-size-base);
    }

    input:focus,
    select:focus {
      outline: 2px solid var(--color-accent);
      outline-offset: 2px;
    }

    .field-error {
      color: var(--color-danger);
      font-size: var(--font-size-xs);
      font-weight: var(--font-weight-regular);
    }

    @media (max-width: 520px) {
      .modal-form {
        grid-template-columns: 1fr;
      }
      label.wide {
        grid-column: auto;
      }
    }
  `]
})
export class QuickActionsComponent implements OnChanges {
  /** ID del alumno sobre el que se operará. */
  @Input({ required: true }) alumnoId!: string;

  /** Lista de AlumnoPace del alumno (para el selector de Score Station). */
  @Input() paces: AlumnoPace[] = [];

  /** Rol del usuario autenticado. */
  @Input() role: Rol | null = null;

  /** Se emite cada vez que una acción se completa con éxito. */
  @Output() changed = new EventEmitter<void>();

  private readonly fb = inject(FormBuilder);
  private readonly meritosService = inject(MeritosService);
  private readonly pacesService = inject(PacesService);
  private readonly toast = inject(ToastService);

  activeModal: ActiveModal = null;
  submitting = false;
  scorablePaces: AlumnoPace[] = [];

  readonly meritoForm = this.fb.nonNullable.group({
    puntos: [1, [Validators.required, Validators.min(1), Validators.max(100)]],
    motivo: ['', [Validators.required, Validators.minLength(3)]]
  });

  readonly demeritoForm = this.fb.nonNullable.group({
    puntos: [1, [Validators.required, Validators.min(1), Validators.max(100)]],
    motivo: ['', [Validators.required, Validators.minLength(3)]]
  });

  readonly scoreForm = this.fb.nonNullable.group({
    alumnoPaceId: ['', Validators.required],
    puntaje: [0, [Validators.required, Validators.min(0), Validators.max(100)]],
    exitoso: [true, Validators.required]
  });

  get canScore(): boolean {
    return this.role === 'Principal' || this.role === 'Supervisora';
  }

  ngOnChanges(): void {
    // Only PACEs in states that can receive a score
    const scorableStates = ['ListoParaAutoTest', 'AutoTestOk', 'AutoTestFallido', 'EnTestFinal'];
    this.scorablePaces = this.paces.filter(p => scorableStates.includes(p.estado));
  }

  open(modal: ActiveModal): void {
    this.activeModal = modal;
    this.submitting = false;
  }

  close(): void {
    if (this.submitting) return;
    this.activeModal = null;
    this.meritoForm.reset({ puntos: 1, motivo: '' });
    this.demeritoForm.reset({ puntos: 1, motivo: '' });
    this.scoreForm.reset({ alumnoPaceId: '', puntaje: 0, exitoso: true });
  }

  submitMerito(): void {
    if (this.meritoForm.invalid) {
      this.meritoForm.markAllAsTouched();
      return;
    }
    const { puntos, motivo } = this.meritoForm.getRawValue();
    this.submit(() =>
      this.meritosService.registrar({ alumnoId: this.alumnoId, tipo: 'Merito', puntos, motivo }),
      'Mérito registrado. ✅'
    );
  }

  submitDemerito(): void {
    if (this.demeritoForm.invalid) {
      this.demeritoForm.markAllAsTouched();
      return;
    }
    const { puntos, motivo } = this.demeritoForm.getRawValue();
    this.submit(() =>
      this.meritosService.registrar({ alumnoId: this.alumnoId, tipo: 'Demerito', puntos, motivo }),
      'Demérito registrado.'
    );
  }

  submitScore(): void {
    if (this.scoreForm.invalid) {
      this.scoreForm.markAllAsTouched();
      return;
    }
    const { alumnoPaceId, exitoso } = this.scoreForm.getRawValue();
    this.submit(() =>
      this.pacesService.checkProgress(alumnoPaceId, exitoso),
      'Puntaje actualizado. ✅'
    );
  }

  private submit<T>(apiCall: () => import('rxjs').Observable<T>, successMessage: string): void {
    this.submitting = true;
    apiCall().subscribe({
      next: () => {
        this.submitting = false;
        this.activeModal = null;
        this.close();
        this.toast.success(successMessage);
        this.changed.emit();
      },
      error: () => {
        this.submitting = false;
        this.toast.error('No se pudo completar la acción. Intenta de nuevo.');
      }
    });
  }
}
