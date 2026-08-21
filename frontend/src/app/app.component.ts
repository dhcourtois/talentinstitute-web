import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Subscription, interval } from 'rxjs';
import { ToastComponent } from './shared/components/toast/toast.component';
import { SessionTimeoutModalComponent } from './shared/components/session-timeout-modal/session-timeout-modal.component';
import { SessionTimeoutService } from './core/services/session-timeout.service';
import { AuthService } from './core/services/auth.service';

const WARNING_DURATION_SECONDS = 5 * 60; // must match WARNING_DURATION_MS in the service

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ToastComponent, SessionTimeoutModalComponent],
  template: `
    <router-outlet />
    <app-toast />
    <app-session-timeout-modal
      [visible]="showTimeoutWarning"
      [countdown]="countdown"
      (continued)="onContinueSession()"
      (loggedOut)="onForceLogout()"
    />
  `,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit, OnDestroy {
  showTimeoutWarning = false;
  countdown = WARNING_DURATION_SECONDS;

  private subs = new Subscription();
  private countdownSub: Subscription | null = null;

  constructor(
    private sessionTimeout: SessionTimeoutService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.sessionTimeout.start();
    }

    this.subs.add(
      this.sessionTimeout.showWarning$.subscribe(() => {
        this.showTimeoutWarning = true;
        this.countdown = WARNING_DURATION_SECONDS;
        this.startCountdown();
      })
    );

    this.subs.add(
      this.sessionTimeout.sessionExpired$.subscribe(() => {
        this.dismissWarning();
        this.auth.logout();
      })
    );
  }

  onContinueSession(): void {
    this.dismissWarning();
    this.sessionTimeout.resetTimers();
  }

  onForceLogout(): void {
    this.dismissWarning();
    this.sessionTimeout.stop();
    this.auth.logout();
  }

  private startCountdown(): void {
    this.countdownSub?.unsubscribe();
    this.countdownSub = interval(1000).subscribe(() => {
      this.countdown = Math.max(0, this.countdown - 1);
    });
  }

  private dismissWarning(): void {
    this.showTimeoutWarning = false;
    this.countdownSub?.unsubscribe();
    this.countdownSub = null;
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
    this.countdownSub?.unsubscribe();
  }
}

