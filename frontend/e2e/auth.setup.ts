/**
 * auth.setup.ts
 *
 * Setup global de autenticación para Playwright.
 * Hace login con cada rol y guarda el estado de sesión en `e2e/.auth/`.
 * Los proyectos de test reutilizan estos estados — no se re-loguea en cada test.
 *
 * Ejecutar con: npx playwright test --project=setup
 */

import { test as setup, expect } from '@playwright/test';
import path from 'path';

const BASE_URL = process.env['BASE_URL'] ?? 'http://localhost:4200';

const ROLES = [
  {
    file:     'supervisora.json',
    email:    'supervisora@talentinstitute.com',
    password: 'supervisora123',
    rol:      'Supervisora',
  },
  {
    file:     'monitora.json',
    email:    'monitora@talentinstitute.com',
    password: 'monitora123',
    rol:      'Monitora',
  },
  {
    file:     'principal.json',
    email:    'principal@talentinstitute.com',
    password: 'principal123',
    rol:      'Principal',
  },
] as const;

for (const { file, email, password, rol } of ROLES) {
  setup(`authenticate as ${rol}`, async ({ page }) => {
    await page.goto(`${BASE_URL}/login`);

    // Llena el formulario de login
    await page.getByLabel('Correo').fill(email);
    await page.getByLabel('Contraseña').fill(password);

    // Selecciona el rol visual (botones de toggle)
    await page.getByRole('button', { name: rol, exact: true }).click();

    // Envía el formulario
    await page.getByRole('button', { name: 'Ingresar' }).click();

    // Si aparece el overlay de primer acceso, lo descarta
    const continuar = page.getByRole('button', { name: 'Continuar' });
    if (await continuar.isVisible({ timeout: 3000 }).catch(() => false)) {
      await continuar.click();
    }

    // Espera a que el dashboard cargue
    await expect(page).toHaveURL(/dashboard/, { timeout: 10_000 });

    // Guarda el estado (localStorage con JWT + cookies)
    const authFile = path.join(__dirname, '.auth', file);
    await page.context().storageState({ path: authFile });
  });
}
