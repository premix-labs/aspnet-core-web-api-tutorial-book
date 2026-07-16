import * as LucideIcons from '@lucide/astro';
import { BookOpen } from '@lucide/astro';

type IconComponent = typeof BookOpen;

function toPascalCase(name: string): string {
  return name
    .trim()
    .split(/[\s_-]+/)
    .map((part) => part.charAt(0).toUpperCase() + part.slice(1))
    .join('');
}

/**
 * Resolves any lucide.dev icon name ("book-open", "settings", ...) to its
 * `@lucide/astro` component. Falls back to BookOpen for unknown/missing names,
 * so chapter frontmatter can reference any icon without touching layout code.
 */
export function getIcon(name?: string): IconComponent {
  if (!name) return BookOpen;
  const key = toPascalCase(name);
  return (LucideIcons as unknown as Record<string, IconComponent>)[key] ?? BookOpen;
}
