import { Component, Input, computed, signal } from '@angular/core';

@Component({
  selector: 'app-progress-bar',
  standalone: true,
  template: `
    <div class="progress" role="progressbar" [attr.aria-valuenow]="value" aria-valuemin="0" aria-valuemax="100">
      <div class="progress__fill" [class]="colorClass()" [style.width.%]="clampedValue()"></div>
      <span class="progress__label">{{ clampedValue() }}%</span>
    </div>
  `,
  styleUrl: './progress-bar.component.css'
})
export class ProgressBarComponent {
  @Input() set value(v: number) { this._value.set(v); }

  private _value = signal(0);

  clampedValue = computed(() => Math.min(100, Math.max(0, this._value())));

  colorClass = computed(() => {
    const v = this.clampedValue();
    if (v < 40) return 'progress__fill--red';
    if (v < 70) return 'progress__fill--orange';
    return 'progress__fill--green';
  });
}
