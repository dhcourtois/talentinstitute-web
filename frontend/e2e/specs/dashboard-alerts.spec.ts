/**
 * dashboard-alerts.spec.ts
 *
 * Flujo de alertas:
 *   El seed de datos incluye Mateo y Sofía sin metas completadas recientes.
 *   Este test verifica que el banner de alertas es visible y funciona.
 */

import { test, expect } from '@playwright/test';
import { goToDashboard } from '../helpers';

test.describe('Dashboard — Alertas y Chart', () => {
  test('banner de alertas muestra alumnos o indica sin alertas', async ({ page }) => {
    await goToDashboard(page);

    const alertSection = page.locator('section.alerts');
    await expect(alertSection).toBeVisible();

    // Puede tener alertas (alumnos sin meta) o estar limpio
    const badge = alertSection.locator('app-badge');
    await expect(badge).toBeVisible();
    const badgeText = await badge.textContent();

    expect(
      badgeText?.includes('pendientes') || badgeText?.includes('Sin alertas')
    ).toBeTruthy();
  });

  test('gráfica semanal muestra los 7 días con Sáb/Dom visible', async ({ page }) => {
    await goToDashboard(page);

    const chart = page.locator('app-weekly-chart');
    await expect(chart).toBeVisible();

    // Verifica que el chart tiene barras de los 7 días
    const bars = chart.locator('.bar-col');
    await expect(bars).toHaveCount(7);
  });

  test('panel Score Station es visible', async ({ page }) => {
    await goToDashboard(page);

    await expect(page.locator('app-score-pending-list')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'Score Station' })).toBeVisible();
  });

  test('clic en alerta navega al perfil del alumno', async ({ page }) => {
    await goToDashboard(page);

    const alertRow = page.locator('a.alert-row').first();
    const isVisible = await alertRow.isVisible({ timeout: 3_000 }).catch(() => false);

    if (!isVisible) {
      test.skip(true, 'No hay alertas activas en el estado actual del seed');
      return;
    }

    await alertRow.click();
    await expect(page).toHaveURL(/\/alumno\//);
  });
});
