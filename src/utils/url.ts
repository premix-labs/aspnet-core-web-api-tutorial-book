/**
 * `import.meta.env.BASE_URL` is documented as always ending in `/`, but that only holds
 * when the configured `base` itself ends in `/`. This repo's own astro.config.mjs sets
 * `base` from an env var (`BASE_PATH=/my-book`, no trailing slash) to match GitHub Actions'
 * per-repo deploy convention — and confirmed by an actual build, BASE_URL then comes through
 * as `/my-book` with NO trailing slash, silently producing `/my-bookintro`-style broken
 * paths everywhere. Never trust the raw value; always go through this.
 */
function normalizedBaseUrl(): string {
  const base = import.meta.env.BASE_URL;
  return base.endsWith('/') ? base : `${base}/`;
}

/**
 * Resolves a *bare* content path (no leading base — e.g. a chapter id like `'intro'`,
 * or `''` for home) against `site` + the configured `base`, as a full production URL.
 * Use for sitemap.xml, robots.txt, and `siteConfig.ogImage`.
 *
 * Two more traps, both silent, both handled here:
 * 1. `new URL(path, site)` alone: when `site` has no trailing slash (e.g. a GitHub Pages
 *    project site like `https://user.github.io/book`), WHATWG URL resolution treats the
 *    last segment as a "file" and drops it — `new URL('intro', 'https://user.github.io/book')`
 *    resolves to `https://user.github.io/intro`, losing `/book`.
 * 2. `Astro.site` never includes `base` — they're separate config keys. A URL built from
 *    `site` alone silently omits the subpath a GitHub Pages *project* site is actually
 *    served under, even though every on-page link (via `withBase`) has it.
 */
export function absoluteUrl(path: string, site?: URL): string {
  const siteHref = site ? site.href : 'https://example.com/';
  const normalizedSite = siteHref.endsWith('/') ? siteHref : `${siteHref}/`;
  const basePath = normalizedBaseUrl().replace(/^\/+/, ''); // e.g. 'book-name/' or ''
  const normalizedPath = path.replace(/^\/+/, '');
  return new URL(basePath + normalizedPath, normalizedSite).href;
}

/**
 * Resolves a path that *already includes* `base` — namely `Astro.url.pathname`, which Astro
 * itself prefixes with `base` — against `site`, as a full production URL. Use for canonical
 * links / `og:url`. Do NOT use with a bare content path here: `base` would never get added
 * (nothing re-derives it), and don't reach for `absoluteUrl` instead: it WOULD add `base`,
 * double-prefixing a path that already has it (`/book/book/intro`).
 */
export function absoluteUrlFromFullPath(fullPath: string, site?: URL): string {
  const siteHref = site ? site.href : 'https://example.com/';
  const normalizedSite = siteHref.endsWith('/') ? siteHref : `${siteHref}/`;
  const cleanPath = fullPath.replace(/^\/+/, '');
  return new URL(cleanPath, normalizedSite).href;
}

/**
 * Prefixes an internal, root-relative path with Astro's configured `base`.
 *
 * Astro does NOT rewrite hardcoded `href="/foo"` strings for you when `base`
 * is set to a subpath (e.g. deploying to a GitHub Pages *project* site at
 * `user.github.io/book-name/`) — every internal link must be built through
 * this helper or it 404s in production while working fine in local dev
 * (where `base` defaults to `/`). Use for hrefs only; hash fragments like
 * `#article-content` don't need it.
 */
export function withBase(path: string): string {
  const cleanPath = path.replace(/^\/+/, '');
  return `${normalizedBaseUrl()}${cleanPath}`;
}
