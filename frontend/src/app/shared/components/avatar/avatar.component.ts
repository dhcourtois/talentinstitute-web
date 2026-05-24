import { Component, Input, computed, signal } from '@angular/core';

const PALETTE = [
  '#0071E3', '#34C759', '#FF9500', '#FF3B30',
  '#5AC8FA', '#AF52DE', '#FF2D55', '#5856D6'
];

@Component({
  selector: 'app-avatar',
  standalone: true,
  template: `
    <div
      class="avatar"
      [style.background-color]="bgColor()"
      [attr.aria-label]="'Avatar de ' + name"
      [title]="name"
    >
      {{ initials() }}
    </div>
  `,
  styleUrl: './avatar.component.css'
})
export class AvatarComponent {
  @Input() set name(v: string) { this._name.set(v); }

  private _name = signal('');

  initials = computed(() => {
    const parts = this._name().trim().split(/\s+/).filter(Boolean);
    if (!parts.length) return '?';
    if (parts.length === 1) return parts[0][0].toUpperCase();
    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  });

  bgColor = computed(() => {
    const name = this._name();
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return PALETTE[Math.abs(hash) % PALETTE.length];
  });
}
