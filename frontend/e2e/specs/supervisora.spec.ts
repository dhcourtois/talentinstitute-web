/**
 * supervisora.spec.ts
 *
 * Flujo Supervisora:
 *   Dashboard carga métricas → tabla alumnos → perfil alumno
 *   → QuickActions → Score Station modal
 */

import { test, expect } from '@playwright/test';
import { goToDashboard } from '../helpers';

test.describe('Flujo Supervisora', () => {
  test('dashboard carga las 4 métricas ejecutivas', async ({ page }) => {
    await goToDashboard(page);

    await expect(page.getByText('Alumnos activos')).toBeVisible();
    await expect(page.getByText('Metas hoy')).toBeVisible();
    await expect(page.getByText('PACEs en revisión')).toBeVisible();
    await expect(page.getByText('Alertas')).toBeVisible();
  });

  test('tabla de alumnos muestra registros y navega al perfil', async ({ page }) => {
    await goToDashboard(page);

    const rows = page.locator('tbody tr');
    await expect(rows.first()).toBeVisible({ timeout: 8_000 });

    const firstName = await rows.first().locator('td strong').first().textContent();
    await rows.first().getByRole('link', { name: 'Abrir' }).click();

    await expect(page).toHaveURL(/\/alumno\//);
    await expect(page.locator('section.hero h1'))
      .toContainText(firstName?.split(' ')[0] ?? '');
  });

  test('perfil muestra PACEs activos y banda de privilegios', async ({ page }) => {
    await goToDashboard(page);
    await page.locator('tbody tr').first().getByRole('link', { name: 'Abrir' }).click();
    await expect(page).toHaveURL(/\/alumno\//);

    await expect(page.locator('section.privilege-band')).toBeVisible();
    await expect(page.getByRole('heading', { name: 'PACEs activos' })).toBeVisible();
  });

  test('score station: modal abre y cancela correctamente', async ({ page }) => {
    await goToDashboard(page);
    await page.locator('tbody tr').first().getByRole('link', { name: 'Abrir' }).click();
    await expect(page).toHaveURL(/\/alumno\//);

    const btn = page.getByRole('button', { name: 'Actualizar puntos' });
    if (!await btn.isVisible({ timeout: 2_000 }).catch(() => false)) {
      test.skip(true, 'Sin PACEs en estado scorable');
      return;
    }

    await btn.click();
    await expect(page.getByRole('dialog')).toBeVisible();
    await expect(page.getByRole('dialog')).toContainText('Score Station');

    await page.getByRole('button', { name: 'Cancelar' }).click();
    await expect(page.getByRole('dialog')).not.toBeVisible();
  });
});
