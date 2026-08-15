import { Component, Input } from '@angular/core';

export type BadgeVariant = 'green' | 'orange' | 'red' | 'blue' | 'gray';

@Component({
  selector: 'app-badge',
  standalone: true,
  template: `<span [class]="'badge badge--' + variant"><ng-content /></span>`,
  styleUrl: './badge.component.css'
})
export class BadgeComponent {
  @Input() variant: BadgeVariant = 'gray';
}
