import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { AppComponent } from './app.component';
import { SessionTimeoutService } from './core/services/session-timeout.service';

/**
 * Las dos pruebas que venían del andamiaje de Angular comprobaban una
 * propiedad `title` y un encabezado "Hello, frontend" que el componente nunca
 * llegó a tener. No solo fallaban: el error de compilación impedía que Karma
 * cargara la suite completa, así que ninguna prueba del frontend se ejecutaba.
 *
 * Se reescriben contra lo que el componente sí hace hoy: montar el router, los
 * avisos y el modal de cierre de sesión por inactividad (QA_004).
 */
describe('AppComponent', () => {
  let sessionTimeout: SessionTimeoutService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    sessionTimeout = TestBed.inject(SessionTimeoutService);
  });

  afterEach(() => {
    // El servicio engancha listeners en window; sin detenerlo se filtran entre pruebas.
    sessionTimeout.stop();
  });

  it('se crea', () => {
    const fixture = TestBed.createComponent(AppComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('monta el router-outlet y el contenedor de avisos', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('router-outlet')).toBeTruthy();
    expect(compiled.querySelector('app-toast')).toBeTruthy();
  });

  it('arranca sin el aviso de sesión visible', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    expect(fixture.componentInstance.showTimeoutWarning).toBeFalse();
  });

  it('muestra el aviso cuando el servicio de inactividad lo emite (QA_004)', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    sessionTimeout.showWarning$.next();
    fixture.detectChanges();

    expect(fixture.componentInstance.showTimeoutWarning).toBeTrue();
  });

  it('al continuar la sesión oculta el aviso', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();

    sessionTimeout.showWarning$.next();
    fixture.detectChanges();

    fixture.componentInstance.onContinueSession();
    fixture.detectChanges();

    expect(fixture.componentInstance.showTimeoutWarning).toBeFalse();
  });
});
