import type { APIRoute } from 'astro';
import { getCollection } from 'astro:content';
import { absoluteUrl } from '../utils/url';

// Dependency-free sitemap: lists the home page and every non-draft chapter.
export const GET: APIRoute = async ({ site }) => {
  const chapters = await getCollection('chapters', ({ data }) => !data.draft);
  const paths = ['', ...chapters.sort((a, b) => a.data.chapter - b.data.chapter).map((c) => c.id)];

  const urls = paths.map((path) => `  <url><loc>${absoluteUrl(path, site)}</loc></url>`).join('\n');

  const body = `<?xml version="1.0" encoding="UTF-8"?>
<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">
${urls}
</urlset>`;

  return new Response(body, {
    headers: { 'Content-Type': 'application/xml' },
  });
};
