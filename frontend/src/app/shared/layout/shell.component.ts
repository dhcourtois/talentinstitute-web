import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { ItemNavegacion, navegacionPara } from '../../core/auth/permissions';
import { Rol } from '../../models';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <div class="shell">
      <a class="skip-link" href="#contenido">Ir al contenido</a>

      <aside class="sidebar">
        <div class="brand">
          <span class="brand-mark" aria-hidden="true">TI</span>
          <div>
            <p class="eyebrow">Sistema ACE</p>
            <strong>Talent Institute</strong>
          </div>
        </div>

        <nav aria-label="Módulos">
          <a
            *ngFor="let item of navegacion"
            [routerLink]="item.ruta"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{ exact: false }"
          >
            {{ item.etiqueta }}
          </a>
        </nav>

        <div class="session">
          <p class="eyebrow">Sesión</p>
          <strong>{{ role ?? 'Sin rol' }}</strong>
          <button type="button" (click)="logout()">Cerrar sesión</button>
        </div>
      </aside>

      <main id="contenido" class="content">
        <router-outlet />
      </main>
    </div>
  `,
  styles: [`
    .shell {
      display: grid;
      grid-template-columns: 248px minmax(0, 1fr);
      min-height: 100vh;
      background: var(--color-bg-app, #f5f6f8);
    }

    .skip-link {
      position: absolute;
      left: -9999px;
    }

    .skip-link:focus {
      left: var(--space-4);
      top: var(--space-4);
      z-index: 10;
      padding: var(--space-2) var(--space-3);
      background: var(--color-bg-surface);
      border-radius: var(--radius-sm);
    }

    .sidebar {
      display: flex;
      flex-direction: column;
      gap: var(--space-6);
      padding: var(--space-5) var(--space-4);
      border-right: 1px solid var(--color-border);
      background: var(--color-bg-surface);
    }

    .brand {
      display: flex;
      align-items: center;
      gap: var(--space-3);
    }

    .brand-mark {
      display: grid;
      place-items: center;
      width: 40px;
      height: 40px;
      border-radius: var(--radius-md);
      background: var(--color-primary);
      color: #fff;
      font-weight: var(--font-weight-bold);
    }

    .eyebrow {
      margin: 0;
      color: var(--color-text-secondary);
      font-size: var(--font-size-sm);
      text-transform: uppercase;
      letter-spacing: 0.06em;
    }

    nav {
      display: flex;
      flex-direction: column;
      gap: var(--space-1);
    }

    nav a {
      display: flex;
      align-items: center;
      min-height: var(--tap-target-min, 44px);
      padding: 0 var(--space-3);
      border-radius: var(--radius-sm);
      color: var(--color-text-secondary);
      font-weight: var(--font-weight-semibold);
      text-decoration: none;
    }

    nav a:hover {
      background: var(--color-bg-app);
      color: var(--color-text-primary);
    }

    nav a.active {
      background: var(--color-primary);
      color: #fff;
    }

    .session {
      display: grid;
      gap: var(--space-2);
      margin-top: auto;
      padding-top: var(--space-4);
      border-top: 1px solid var(--color-border);
    }

    .session button {
      min-height: var(--tap-target-min, 44px);
      border: 1px solid var(--color-border);
      border-radius: var(--radius-sm);
      background: var(--color-bg-surface);
      color: var(--color-text-primary);
      font-weight: var(--font-weight-semibold);
      cursor: pointer;
    }

    .content {
      min-width: 0;
    }

    @media (max-width: 860px) {
      .shell {
        grid-template-columns: 1fr;
      }

      .sidebar {
        border-right: none;
        border-bottom: 1px solid var(--color-border);
      }

      nav {
        flex-direction: row;
        flex-wrap: wrap;
      }

      .session {
        margin-top: 0;
      }
    }
  `]
})
export class ShellComponent {
  private readonly auth = inject(AuthService);

  readonly role: Rol | null = this.auth.getRole();
  readonly navegacion: ItemNavegacion[] = navegacionPara(this.role);

  logout(): void {
    this.auth.logout();
  }
}
