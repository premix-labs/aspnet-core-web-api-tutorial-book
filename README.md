# ASP.NET Core Web API Tutorial Book

หนังสือสอน ASP.NET Core Web API สำหรับมือใหม่ ผ่านโปรเจกต์ Backend API

ชื่อ repository สำหรับ deploy:

```text
aspnet-core-web-api-tutorial-book
```

## Stack หลัก

- .NET 10 LTS
- ASP.NET Core Web API แบบ Controllers
- Entity Framework Core
- SQL Server เป็นฐานข้อมูลหลัก
- JWT Bearer Authentication
- OpenAPI
- xUnit และ integration tests
- Docker Compose
- Astro + Tailwind 4 สำหรับเว็บไซต์หนังสือ (จาก `astro-book-template`)
- Mermaid diagrams สำหรับอธิบาย flow สำคัญ — เขียนเป็น ` ```mermaid ` fenced block ปกติ

## คำสั่ง

```powershell
npm install
npm run dev
npm run verify
```

| Command                           | Action                                                               |
| --------------------------------- | -------------------------------------------------------------------- |
| `npm run new-chapter -- "ชื่อบท"` | สร้างไฟล์บทใหม่พร้อม frontmatter ที่ถูกต้อง                          |
| `npm run audit:book`              | ตรวจความเสี่ยงด้านการสอน ความปลอดภัย และ portability                 |
| `npm run verify`                  | ตรวจ format, lint, types, tests, เนื้อหา, build และ links            |
| `npm run verify:enterprise`       | เพิ่ม browser, accessibility, visual, performance และ security gates |
| `npm run template:status`         | แสดงเวอร์ชัน template และ managed-file drift                         |
| `npm run preview`                 | Preview production build locally                                     |

เนื้อหาหนังสือเก็บใน `src/content/chapters/` เป็น Astro Content Collection ที่ validate ด้วย Zod
(`src/content.config.ts`) Frontmatter รองรับ `part` สำหรับจัดกลุ่มบทตามภาค — sidebar และหน้าแรกจัดกลุ่มให้อัตโนมัติ

ทุกบทใช้ component ได้โดยไม่ต้อง import: `<Callout>`, `<Steps>`, `<Tabs>`/`<Tab>`, `<Figure>`, `<Kbd>`, `<Badge>`
รีแบรนด์ได้ที่ `src/site.config.ts`

## เอกสารควบคุมคุณภาพ

เอกสารภายในสำหรับพัฒนา ตรวจ และ release หนังสืออยู่ที่ `docs/internal` และใช้ `Book Documentation Standard v1` เหมือนหนังสือเล่มอื่น อ่านมาตรฐานได้ที่ `docs/internal/README.md`

```text
docs/internal/
  README.md
  book-plan.md
  api-contract.md
  final-project-structure.md
  manuscript-status.md
  release-checklist.md
  style-guide.md
  teaching-principles.md
  validation-report.md
  decisions/
  qa/
```

ก่อนแก้บทเรียนหรือ example project ให้อ่าน:

- `AGENTS.md`
- `skills/tutorial-book-auditor/SKILL.md`
- `skills/tutorial-book-auditor/references/teaching-principles.md`
- `docs/internal/teaching-principles.md`
- `docs/internal/style-guide.md`

ก่อน release ให้อ่าน:

- `docs/internal/release-checklist.md`
- `docs/internal/validation-report.md`
- `docs/internal/qa/browser-test-plan.md`
- `docs/internal/qa/accessibility-checklist.md`
- `docs/internal/qa/security-review-checklist.md`

## Deploy

โปรเจกต์นี้มี GitHub Actions สำหรับ deploy ไป GitHub Pages แล้วใน `.github/workflows/deploy.yml`

ขั้นตอน deploy:

1. สร้าง repository บน GitHub ชื่อ `aspnet-core-web-api-tutorial-book`
2. ไปที่ `Settings > Pages`
3. ตั้งค่า `Source` เป็น `GitHub Actions`
4. push เข้า branch `main`
5. เปิดแท็บ `Actions` แล้วตรวจ workflow `Deploy to GitHub Pages`

ถ้าเริ่มจากโฟลเดอร์ local ที่ยังไม่มี git repository:

```powershell
git init
git branch -M main
git add .
git commit -m "Prepare ASP.NET Core Web API tutorial book"
git remote add origin https://github.com/<github-username>/aspnet-core-web-api-tutorial-book.git
git push -u origin main
```

workflow จะตั้งค่า URL และ base path ให้เอง:

- ถ้า repository ชื่อ `<username>.github.io` เว็บจะอยู่ที่ `https://<username>.github.io`
- ถ้าเป็น repository ปกติ เว็บจะอยู่ที่ `https://<username>.github.io/<repository-name>/`

ก่อน push ควรรัน:

```powershell
npm run verify
```

ถ้าต้องการตรวจ GitHub Pages base path ก่อน push ให้รันด้วยชื่อ repository จริง:

```powershell
$env:SITE="https://<github-username>.github.io"
$env:BASE_PATH="/aspnet-core-web-api-tutorial-book"
npm run build
```

## ตรวจตัวอย่าง Backend

รันจากโฟลเดอร์ repository:

```powershell
dotnet test examples/final-backend-api/Backend.Api.Tests/Backend.Api.Tests.csproj
dotnet test examples/validation/progressive-backend-api/Backend.Api.Tests/Backend.Api.Tests.csproj
dotnet publish examples/final-backend-api/Backend.Api/Backend.Api.csproj -c Release
docker compose -f examples/final-backend-api/docker-compose.yml config
docker compose -f examples/validation/progressive-backend-api/docker-compose.yml config
npm run build
```
