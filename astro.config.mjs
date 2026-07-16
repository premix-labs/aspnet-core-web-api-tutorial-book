// @ts-check
import { defineConfig } from 'astro/config';
import mdx from '@astrojs/mdx';
import tailwindcss from '@tailwindcss/vite';

// `site`/`base` are read from env vars so the same config works whether this deploys to a
// GitHub Pages *user/org* site (root domain, no base) or a *project* site (subpath, needs
// base). .github/workflows/deploy.yml computes these per-repo automatically.
const site = process.env.SITE ?? 'https://premix-labs.github.io';
const base = process.env.BASE_PATH;

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
    plugins: [tailwindcss()],
    build: {
      chunkSizeWarningLimit: 800,
    },
  },
});
