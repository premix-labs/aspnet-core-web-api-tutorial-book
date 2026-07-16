// site.config.ts — Single place to rebrand this template for this book.
// Everything here flows into the landing page and the reading layout.

export const siteConfig = {
  /** Short brand name shown in the header logo lockup. */
  title: 'ASP.NET Core',
  /** Small line under the title in the header. */
  tagline: 'Web API Tutorial',
  /** Used for <meta description>, OG description and the header pill. */
  description:
    'หนังสือสอน ASP.NET Core Web API สำหรับมือใหม่ ผ่านโปรเจกต์ Backend API ด้วย .NET, EF Core, JWT, Test และ Docker',
  /** HTML lang attribute — Thai, since chapter content is authored in Thai. */
  lang: 'th',
  /**
   * Social share image for link previews (og:image / Twitter card).
   * Drop a 1200×630 PNG in `public/` and point here, e.g. '/og-image.png'.
   * Leave empty to omit the tags. Resolved against `site` in astro.config.mjs.
   */
  ogImage: '',
  /** Theme applied on first visit, before the user picks one. 'dark' | 'light' */
  defaultTheme: 'dark' as 'dark' | 'light',
  /** GitHub repo URL. Leave empty to hide the header GitHub button. */
  github: '',

  hero: {
    eyebrow: 'ASP.NET Core · EF Core · JWT',
    /** Rendered as: {prefix} <accent>{accent}</accent> {suffix} */
    prefix: 'สร้าง',
    accent: 'Secure Admin Web API',
    suffix: 'ด้วย ASP.NET Core',
    subtitle:
      'เรียนรู้ ASP.NET Core Web API แบบค่อยเป็นค่อยไป ตั้งแต่ Controller, Entity Framework Core, Authentication/Authorization ด้วย JWT จนถึง Testing และ Docker — สร้างโปรเจกต์เดียวต่อเนื่องตั้งแต่ต้นจนจบ',
  },

  /** Extra stat pills shown under the hero, after the auto-computed chapter count. */
  extraStats: [
    { value: '9', label: 'Parts' },
    { value: '.NET 10', label: 'LTS' },
  ],

  features: [
    {
      icon: 'server',
      title: 'REST API แบบ Controller',
      desc: 'Controller, Service, Repository และ DTO แยกชั้นให้ถูกต้อง',
    },
    {
      icon: 'database',
      title: 'SQL Server + EF Core',
      desc: 'Entity Framework Core, migrations และ database CRUD จริง',
    },
    {
      icon: 'shield-check',
      title: 'Auth ด้วย JWT',
      desc: 'Register, login, role-based authorization และ protect endpoint',
    },
    { icon: 'search', title: 'ค้นหาได้ทั้งเล่ม', desc: 'Full-text search แบบออฟไลน์ผ่าน Pagefind' },
  ],

  footer: {
    text: 'Open Source',
  },
} as const;
