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
- Astro/Starlight สำหรับเว็บไซต์หนังสือ

## คำสั่ง

```powershell
npm install
npm run dev
npm run build
```

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
npm run build
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
