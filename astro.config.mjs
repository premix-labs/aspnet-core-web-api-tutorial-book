// @ts-check
import { defineConfig } from 'astro/config';
import mdx from '@astrojs/mdx';
import tailwindcss from '@tailwindcss/vite';
import { fileURLToPath } from 'node:url';

// `site`/`base` are read from env vars so the same config works whether this deploys to a
// GitHub Pages *user/org* site (root domain, no base) or a *project* site (subpath, needs
// base). A CI workflow computes these per-repo; see .github/workflows/deploy.yml for the
// pattern — e.g.:
//   SITE=https://<owner>.github.io
//   BASE_PATH=/<repo-name>   (omit entirely for a user/org root site)
const site = process.env.SITE ?? 'https://example.com';
const base = process.env.BASE_PATH;
const picomatchShim = fileURLToPath(new URL('./scripts/shims/picomatch.mjs', import.meta.url));

// https://astro.build/config
export default defineConfig({
  site,
  ...(base ? { base } : {}),
  integrations: [mdx()],
  markdown: {
    shikiConfig: {
      theme: 'one-dark-pro',
      wrap: false,
    },
  },
  vite: {
    resolve: {
      alias: { picomatch: picomatchShim },
    },
    plugins: [tailwindcss()],
    build: {
      chunkSizeWarningLimit: 800,
    },
  },
});
