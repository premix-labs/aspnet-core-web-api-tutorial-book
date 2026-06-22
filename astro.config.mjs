// @ts-check
import { defineConfig } from 'astro/config';
import mermaid from 'astro-mermaid';
import starlight from '@astrojs/starlight';

const site = process.env.SITE ?? 'http://localhost:4321';
const base = process.env.BASE_PATH;

export default defineConfig({
	site,
	...(base ? { base } : {}),
	output: 'static',
	integrations: [
		mermaid({
			autoTheme: true,
			enableLog: false,
			mermaidConfig: {
				flowchart: {
					curve: 'basis',
				},
				sequence: {
					mirrorActors: false,
				},
			},
		}),
		starlight({
			title: 'ASP.NET Core Web API Tutorial Book',
			description:
				'หนังสือสอน ASP.NET Core Web API สำหรับมือใหม่ ผ่านโปรเจกต์ Backend API ด้วย .NET, EF Core, JWT, Test และ Docker',
			customCss: ['./src/styles/custom.css'],
			sidebar: [
				{
					label: 'เริ่มต้น',
					items: [
						{ label: 'หน้าแรก', link: '/' },
						{ label: 'แผนทั้งเล่ม', link: 'book-plan/' },
						{ label: 'สถานะต้นฉบับ', link: 'manuscript-status/' },
					],
				},
				{
					label: 'ภาค 1: พื้นฐาน',
					items: [{ autogenerate: { directory: 'part-1-foundation' } }],
				},
				{
					label: 'ภาค 2: REST API',
					items: [{ autogenerate: { directory: 'part-2-rest-api' } }],
				},
				{
					label: 'ภาค 3: Architecture',
					items: [{ autogenerate: { directory: 'part-3-architecture' } }],
				},
				{
					label: 'ภาค 4: Database',
					items: [{ autogenerate: { directory: 'part-4-database' } }],
				},
				{
					label: 'ภาค 5: Validation & Errors',
					items: [{ autogenerate: { directory: 'part-5-validation-errors' } }],
				},
				{
					label: 'ภาค 6: Authentication',
					items: [{ autogenerate: { directory: 'part-6-auth' } }],
				},
				{
					label: 'ภาค 7: Admin System',
					items: [{ autogenerate: { directory: 'part-7-admin' } }],
				},
				{
					label: 'ภาค 8: Production Ready',
					items: [{ autogenerate: { directory: 'part-8-production-ready' } }],
				},
				{
					label: 'ภาค 9: Production Hardening',
					items: [{ autogenerate: { directory: 'part-9-production-hardening' } }],
				},
				{
					label: 'ภาคผนวก',
					items: [{ autogenerate: { directory: 'appendix' } }],
				},
			],
		}),
	],
});
