import type { APIRoute } from 'astro';
import { absoluteUrl } from '../utils/url';

// Generated so the Sitemap URL always matches `site` in astro.config.mjs.
export const GET: APIRoute = ({ site }) => {
  const body = `User-agent: *
Allow: /

Sitemap: ${absoluteUrl('sitemap.xml', site)}
`;
  return new Response(body, {
    headers: { 'Content-Type': 'text/plain' },
  });
};
