import { defineConfig, devices } from '@playwright/test';

/**
 * Configuración de Playwright — Talent Institute ACE
 *
 * baseURL: Se resuelve desde la variable de entorno BASE_URL.
 *   - Local:   BASE_URL=http://localhost:4200
 *   - Staging: BASE_URL=https://talentinstitute-web.azurewebsites.net
 *
 * Navegadores: Chromium (desktop) + WebKit (Safari/iPad — prioritario por el SOW)
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: process.env['CI'] ? 1 : undefined,

  reporter: [
    ['html', { outputFolder: 'e2e-report', open: 'never' }],
    ['list'],
  ],

  use: {
    baseURL: process.env['BASE_URL'] ?? 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
    // Simula iPad (tablet-first según SOW)
    viewport: { width: 1024, height: 768 },
  },

  projects: [
    // ── Setup: crea los archivos de estado de sesión por rol ────────────────
    {
      name: 'setup',
      testMatch: /.*\.setup\.ts/,
    },

    // ── Desktop Chrome ──────────────────────────────────────────────────────
    {
      name: 'chromium',
      use: {
        ...devices['Desktop Chrome'],
        storageState: 'e2e/.auth/supervisora.json',
      },
      dependencies: ['setup'],
    },

    // ── iPad / Safari (tablet-first) ────────────────────────────────────────
    {
      name: 'webkit-ipad',
      use: {
        ...devices['iPad Pro 11'],
        storageState: 'e2e/.auth/supervisora.json',
      },
      dependencies: ['setup'],
    },

    // ── Monitora (rol específico) ───────────────────────────────────────────
    {
      name: 'chromium-monitora',
      use: {
        ...devices['Desktop Chrome'],
        storageState: 'e2e/.auth/monitora.json',
      },
      dependencies: ['setup'],
      testMatch: /.*monitora.*/,
    },
  ],

  // Levanta ng serve si no hay servidor externo (solo en local sin CI)
  // Comentado por defecto para no interferir con servidores existentes.
  // webServer: {
  //   command: 'npx ng serve',
  //   url: 'http://localhost:4200',
  //   reuseExistingServer: true,
  // },
});
