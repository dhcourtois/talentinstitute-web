/**
 * auth-errors.spec.ts
 *
 * Flujo de errores de autenticación:
 *   - Credenciales incorrectas → mensaje inline sin redirigir
 *   - Acceso directo a ruta protegida sin sesión → redirect a /login
 */

import { test, expect } from '@playwright/test';
import { BASE_URL } from '../helpers';

// Este spec NO usa storageState — testea flujos sin sesión
test.use({ storageState: { cookies: [], origins: [] } });

test.describe('Flujo de errores de autenticación', () => {
  test('credenciales incorrectas muestran mensaje inline', async ({ page }) => {
    await page.goto(`${BASE_URL}/login`);

    await page.getByLabel('Correo').fill('no-existe@test.com');
    await page.getByLabel('Contraseña').fill('wrongpassword');
    await page.getByRole('button', { name: 'Ingresar' }).click();

    // El mensaje de error aparece sin redirigir
    await expect(page.getByText(/credenciales/i)).toBeVisible({ timeout: 6_000 });
    await expect(page).toHaveURL(/login/);
  });

  test('acceso a /dashboard sin token redirige a /login', async ({ page }) => {
    await page.goto(`${BASE_URL}/dashboard`);
    await expect(page).toHaveURL(/login/, { timeout: 5_000 });
  });

  test('acceso a /alumno sin token redirige a /login', async ({ page }) => {
    await page.goto(`${BASE_URL}/alumno/00000000-0000-0000-0000-000000000000`);
    await expect(page).toHaveURL(/login/, { timeout: 5_000 });
  });
});
