import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app.component';

/**
 * Las dos pruebas que venían del andamiaje de Angular comprobaban una
 * propiedad `title` y un encabezado "Hello, frontend" que el componente nunca
 * llegó a tener. No solo fallaban: el error de compilación impedía que Karma
 * cargara la suite completa, así que ninguna prueba del frontend se ejecutaba.
 */
describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [provideRouter([])]
    }).compileComponents();
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
});
