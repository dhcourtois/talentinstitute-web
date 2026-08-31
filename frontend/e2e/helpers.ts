/**
 * helpers.ts — Utilidades compartidas para los tests E2E de Talent Institute
 */
import { Page, expect } from '@playwright/test';
import path from 'path';

export const BASE_URL = process.env['BASE_URL'] ?? 'http://localhost:4200';

/** Rutas de los archivos de estado de sesión por rol */
export const AUTH_FILES = {
  supervisora: path.join(__dirname, '.auth', 'supervisora.json'),
  monitora:    path.join(__dirname, '.auth', 'monitora.json'),
  principal:   path.join(__dirname, '.auth', 'principal.json'),
} as const;

/**
 * Navega al dashboard y espera a que cargue correctamente.
 * Útil como primer paso de cada test después de aplicar el estado de sesión.
 */
export async function goToDashboard(page: Page): Promise<void> {
  await page.goto(`${BASE_URL}/dashboard`);
  await expect(page.getByRole('heading', { name: 'Dashboard' })).toBeVisible({ timeout: 10_000 });
}

/**
 * Navega al perfil de un alumno por su ID y espera el hero.
 */
export async function goToAlumno(page: Page, alumnoId: string): Promise<void> {
  await page.goto(`${BASE_URL}/alumno/${alumnoId}`);
  await expect(page.locator('section.hero')).toBeVisible({ timeout: 10_000 });
}

/**
 * Espera a que un toast de éxito aparezca en pantalla.
 */
export async function expectToast(page: Page, text: string | RegExp): Promise<void> {
  await expect(page.locator('.toast').filter({ hasText: text }))
    .toBeVisible({ timeout: 5_000 });
}

/**
 * Abre un modal de QuickActions y lo llena.
 */
export async function openMeritoModal(
  page: Page,
  tipo: 'Mérito' | 'Demérito',
  puntos: number,
  motivo: string
): Promise<void> {
  const btn = tipo === 'Mérito'
    ? page.locator('button.btn--primary', { hasText: 'Mérito' })
    : page.locator('button.btn--danger', { hasText: 'Demérito' });

  await btn.click();
  await expect(page.getByRole('dialog')).toBeVisible();

  await page.getByRole('dialog').getByLabel('Puntos').fill(String(puntos));
  await page.getByRole('dialog').getByLabel('Motivo').fill(motivo);
}
