/**
 * monitora.spec.ts
 *
 * Flujo Monitora:
 *   Dashboard (solo lista alumnos) → seleccionar alumno → marcar meta
 *   como completada → otorgar mérito → verificar balance actualizado.
 */

import { test, expect } from '@playwright/test';
import { goToDashboard, openMeritoModal } from '../helpers';

test.describe('Flujo Monitora', () => {
  test('dashboard solo muestra lista de alumnos — sin métricas ejecutivas', async ({ page }) => {
    await goToDashboard(page);

    // La Monitora ve la tabla pero NO las métricas de Supervisora/Principal
    await expect(page.locator('tbody tr').first()).toBeVisible({ timeout: 8_000 });
    await expect(page.getByText('Alumnos activos')).not.toBeVisible();
    await expect(page.getByText('Alertas activas')).not.toBeVisible();
  });

  test('puede navegar al perfil de un alumno', async ({ page }) => {
    await goToDashboard(page);

    await page.locator('tbody tr').first().getByRole('link', { name: 'Abrir' }).click();
    await expect(page).toHaveURL(/\/alumno\//);
    await expect(page.locator('section.hero')).toBeVisible();
  });

  test('puede otorgar un mérito y el balance se actualiza', async ({ page }) => {
    await goToDashboard(page);
    await page.locator('tbody tr').first().getByRole('link', { name: 'Abrir' }).click();
    await expect(page).toHaveURL(/\/alumno\//);

    // Lee el balance actual
    const balanceBadge = page.locator('section.hero .hero-meta app-badge').filter({ hasText: /Balance/ });
    await expect(balanceBadge).toBeVisible();
    const beforeText = await balanceBadge.textContent();
    const beforeBalance = parseInt(beforeText?.replace(/\D/g, '') ?? '0');

    // Otorga un mérito de 3 puntos
    await openMeritoModal(page, 'Mérito', 3, 'Premio por puntualidad');
    await page.getByRole('button', { name: 'Confirmar mérito' }).click();

    // Espera que el modal se cierre y la página recargue
    await expect(page.getByRole('dialog')).not.toBeVisible({ timeout: 8_000 });

    // Verifica que el balance aumentó
    await expect(balanceBadge).toBeVisible({ timeout: 8_000 });
    const afterText = await balanceBadge.textContent();
    const afterBalance = parseInt(afterText?.replace(/\D/g, '') ?? '0');

    expect(afterBalance).toBeGreaterThan(beforeBalance);
  });

  test('NO ve el botón Actualizar puntos (Score Station) — solo Supervisora/Principal', async ({ page }) => {
    await goToDashboard(page);
    await page.locator('tbody tr').first().getByRole('link', { name: 'Abrir' }).click();
    await expect(page).toHaveURL(/\/alumno\//);

    await expect(page.getByRole('button', { name: 'Actualizar puntos' })).not.toBeVisible();
  });
});
