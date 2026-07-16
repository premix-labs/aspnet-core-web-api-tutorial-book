import { defineCollection, z } from 'astro:content';
import { glob } from 'astro/loaders';

const chapters = defineCollection({
  loader: glob({ pattern: '**/*.mdx', base: './src/content/chapters' }),
  schema: z.object({
    title: z.string(),
    description: z.string().optional(),
    /** Sort order. Use 0 for the introduction. */
    chapter: z.number(),
    /**
     * Optional group label for multi-part books, e.g. "Part 1: Foundation".
     * Chapters are still sorted globally by `chapter`; consecutive chapters sharing
     * a `part` are rendered under one heading in the sidebar and landing grid.
     * Omit entirely for a single-part book — nothing changes.
     */
    part: z.string().optional(),
    /** Any lucide.dev icon name, e.g. "book-open", "settings", "package". */
    icon: z.string().optional(),
    tags: z.array(z.string()).optional(),
    /** Manual override. Leave unset to auto-estimate from word count. */
    readingTime: z.string().optional(),
    /** Hide a chapter from navigation and routing without deleting the file. */
    draft: z.boolean().optional().default(false),
  }),
});

export const collections = { chapters };
