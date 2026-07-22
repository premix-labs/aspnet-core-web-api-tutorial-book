export interface SiteConfig {
  title: string;
  shortTitle: string;
  tagline: string;
  description: string;
  lang: string;
  ogImage: string;
  defaultTheme: 'dark' | 'light';
  github: string;
  statusLabel: string;
  lastReviewed: string;
  footer: {
    text: string;
  };
}
