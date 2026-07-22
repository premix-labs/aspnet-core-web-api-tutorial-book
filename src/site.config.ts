import type { SiteConfig } from './types/site-config';

export const siteConfig = {
  title: 'ASP.NET Core Web API Tutorial',
  shortTitle: 'ASP.NET Core API',
  tagline: 'จากพื้นฐานสู่ Backend ที่พร้อมต่อยอด',
  description:
    'หนังสือสอน ASP.NET Core Web API สำหรับมือใหม่ ผ่านโปรเจกต์ Backend API ด้วย .NET, EF Core, JWT, Testing และ Docker',
  lang: 'th',
  ogImage: '',
  defaultTheme: 'light',
  github: 'https://github.com/premix-labs/aspnet-core-web-api-tutorial-book',
  statusLabel: 'กำลังพัฒนา',
  lastReviewed: 'July 2026',

  footer: {
    text: 'ASP.NET Core Web API Tutorial',
  },
} satisfies SiteConfig;
