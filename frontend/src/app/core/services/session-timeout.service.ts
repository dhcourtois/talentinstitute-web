import { Injectable, NgZone, OnDestroy } from '@angular/core';
import { Subject } from 'rxjs';

/** Total inactivity time before the warning is shown (ms). */
const INACTIVITY_TIMEOUT_MS = 25 * 60 * 1000; // 25 minutes

/** Time between showing the warning and forcing logout (ms). */
const WARNING_DURATION_MS = 5 * 60 * 1000; // 5 minutes

const ACTIVITY_EVENTS = ['mousemove', 'mousedown', 'keydown', 'touchstart', 'scroll'];

@Injectable({ providedIn: 'root' })
export class SessionTimeoutService implements OnDestroy {
  /** Emits when the warning modal should be shown. */
  readonly showWarning$ = new Subject<void>();

  /** Emits when the session has expired and the user should be logged out. */
  readonly sessionExpired$ = new Subject<void>();

  private inactivityTimer: ReturnType<typeof setTimeout> | null = null;
  private warningTimer: ReturnType<typeof setTimeout> | null = null;
  private active = false;

  private readonly boundOnActivity = () => this.onActivity();

  constructor(private ngZone: NgZone) {}

  /** Begin monitoring user activity. Call this after a successful login. */
  start(): void {
    if (this.active) return;
    this.active = true;
    this.ngZone.runOutsideAngular(() => {
      ACTIVITY_EVENTS.forEach(event =>
        window.addEventListener(event, this.boundOnActivity, { passive: true })
      );
    });
    this.resetTimers();
  }

  /** Stop monitoring. Call this on logout or when the user is not authenticated. */
  stop(): void {
    if (!this.active) return;
    this.active = false;
    ACTIVITY_EVENTS.forEach(event =>
      window.removeEventListener(event, this.boundOnActivity)
    );
    this.clearTimers();
  }

  /** Reset the inactivity countdown (e.g. when the user clicks "Continuar"). */
  resetTimers(): void {
    this.clearTimers();
    this.ngZone.runOutsideAngular(() => {
      this.inactivityTimer = setTimeout(() => {
        this.ngZone.run(() => {
          this.showWarning$.next();
          this.warningTimer = setTimeout(() => {
            this.ngZone.run(() => this.sessionExpired$.next());
          }, WARNING_DURATION_MS);
        });
      }, INACTIVITY_TIMEOUT_MS);
    });
  }

  private onActivity(): void {
    this.resetTimers();
  }

  private clearTimers(): void {
    if (this.inactivityTimer !== null) {
      clearTimeout(this.inactivityTimer);
      this.inactivityTimer = null;
    }
    if (this.warningTimer !== null) {
      clearTimeout(this.warningTimer);
      this.warningTimer = null;
    }
  }

  ngOnDestroy(): void {
    this.stop();
  }
}
