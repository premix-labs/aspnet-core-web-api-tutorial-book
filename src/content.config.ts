import { defineCollection } from 'astro:content';
import { glob } from 'astro/loaders';
import { z } from 'astro/zod';

const chapters = defineCollection({
  loader: glob({ pattern: '**/*.mdx', base: './src/content/chapters' }),
  schema: z.object({
    title: z.string(),
    description: z.string().min(20),
    /** Sort order. Use 0 for the introduction. */
    chapter: z.number(),
    /**
     * Optional group label for multi-part books, e.g. "Part 1: Foundation".
     * Chapters are still sorted globally by `chapter`; consecutive chapters sharing
     * a `part` are rendered under one heading in the sidebar and learning path.
     * Omit entirely for a single-part book — nothing changes.
     */
    part: z.string().optional(),
    /** Any lucide.dev icon name, e.g. "book-open", "settings", "package". */
    icon: z.string().optional(),
    tags: z.array(z.string()).optional(),
    difficulty: z.enum(['beginner', 'intermediate', 'advanced']),
    prerequisites: z.array(z.string()),
    learningOutcomes: z.array(z.string()).min(1),
    testedWith: z.array(z.string()).min(1),
    lastVerified: z.coerce.date(),
    status: z.enum(['draft', 'review', 'published', 'deprecated']),
    /** Manual override. Leave unset to auto-estimate from word count. */
    readingTime: z.string().optional(),
    /** Hide a chapter from navigation and routing without deleting the file. */
    draft: z.boolean().optional().default(false),
  }),
});

export const collections = { chapters };
