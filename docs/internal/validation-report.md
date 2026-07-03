---
title: รายงานตรวจทำตามหนังสือ
description: บันทึกผลการทดลองสร้างโปรเจกต์ใหม่และทำตามหนังสือทีละบท
---

# รายงานตรวจทำตามหนังสือ

หน้านี้บันทึกผล validation ที่ต้องใช้ยืนยันว่าผู้อ่านทำตามหนังสือจากศูนย์ได้จริง

## Validation Strategy

ใช้สอง project:

```text
examples/validation/progressive-backend-api
examples/final-backend-api
```

- `progressive-backend-api` ใช้ทำตามหนังสือทีละบท
- `final-backend-api` ใช้เป็น reference ของ end-state

## สถานะการตรวจ

- 2026-07-03: ปรับปรุง Part 6 Authentication ตาม `tutorial-book-auditor`, `style-guide.md` และ `release-checklist.md` เพิ่ม prerequisite/checkpoint ปิดภาค, แยก progressive auth จาก final hardening, เพิ่มคำเตือนเรื่อง JWT secret/claim, ปรับ signing key validation เป็น byte length, เกลา expected auth errors และรัน `npm run build` + `dotnet test` ของ validation project ผ่าน
- 2026-07-03: เปลี่ยน API contract ของหนังสือและ example projects ให้ใช้ `/api/v1` ตั้งแต่ต้น ทั้ง users, auth และ admin เพื่อไม่ต้องย้าย route กลางเล่ม
- 2026-07-03: ปรับปรุง Part 5 Validation/Error Handling ตาม `tutorial-book-auditor`, `style-guide.md` และ `release-checklist.md` เพิ่ม prerequisite/build gate, ทำ route examples ให้ยึด `/api/v1/users`, ปรับคำสั่งสร้างไฟล์ให้รันซ้ำได้ และเพิ่ม checklist ปิดภาค validation
- 2026-07-03: ปรับปรุง Part 4 Database ตาม `tutorial-book-auditor`, `style-guide.md` และ `release-checklist.md` เพิ่ม prerequisite/build gate, ทำ EF Core package version ให้สอดคล้องกับ `dotnet-ef`, เพิ่ม LocalDB option, ลด route mismatch, เพิ่ม checklist ปิดภาคฐานข้อมูล
- 2026-07-03: ปรับปรุง Part 3 Architecture ตาม `tutorial-book-auditor`, `style-guide.md` และ `release-checklist.md` เพิ่ม prerequisite, files changed, build gate, boundary ของ in-memory/DTO/mapping และ checklist ปิดภาค
- 2026-07-03: ปรับปรุง Part 2 Controller/REST API เพิ่ม prerequisite/build gate, ระบุไฟล์ที่เปลี่ยน, ทำ query string example ให้ชัดว่า optional, ผูก status code กับ action จริง, เกลา `.http` body blocks และรัน `npm run build` ผ่าน
- 2026-07-03: ปรับปรุง Part 1 Foundation ให้ route ตัวอย่างตรง API contract, ลดตัวอย่าง auth/password ที่มาก่อนบท, ทำ package version ในบทโครงสร้างเป็น template-aware, เพิ่ม prerequisite/build gate และรัน `npm run build` ผ่าน
- 2026-07-03: ปรับเนื้อหาทดสอบ API ให้ไม่ fix local API port ของ `dotnet run`; ใช้ placeholder `http://localhost:<http-port>` และ `https://localhost:<https-port>` แทน และรัน `npm run build` ผ่าน
- 2026-07-03: เพิ่ม `Book Documentation Standard v1` ใน `docs/internal/README.md` และรัน `npm run build` ผ่าน มี warning เดิมเรื่อง Vite chunk ใหญ่ แต่ build สำเร็จ
- บท 1-15 Controller, REST API, Architecture: ผ่านแล้ว
- บท 16-22 EF Core, SQL Server, Migration, Seed data: ผ่านแล้ว
- บท 23-27 Validation และ Error Handling: ผ่านแล้ว
- บท 28-33 Authentication และ JWT: ผ่านแล้ว
- บท 34-40 Admin, Authorization, Audit Log, Pagination: ผ่านแล้ว
- บท 41-50 Production Ready, Tests, Docker, Publish: ผ่านแล้ว
- บท 51-58 Production Hardening: ผ่านแล้ว

## Smoke Tests ที่ต้องรักษา

```text
GET /api/v1/users                         200 หรือ 401/403 ตามบท
POST /api/v1/auth/register                200
POST /api/v1/auth/login                   200
GET /api/v1/auth/me                       200 เมื่อส่ง token
GET /api/v1/admin/users                   200 เมื่อเป็น admin
GET /api/v1/admin/users                   403 เมื่อเป็น user
PATCH /api/v1/admin/users/{id}/role       200 หรือ validation error
PATCH /api/v1/admin/users/{id}/status     200 หรือ self-protection error
GET /health/live                       200
GET /health/ready                      200 เมื่อ dependency พร้อม
```

## Commands

Website:

```powershell
npm run build
```

Final project:

```powershell
cd examples/final-backend-api
dotnet restore
dotnet tool restore
dotnet build -c Release
dotnet test -c Release
dotnet publish Backend.Api/Backend.Api.csproj -c Release
docker compose config
```

Validation project:

```powershell
cd examples/validation/progressive-backend-api
dotnet restore
dotnet tool restore
dotnet build -c Release
dotnet test -c Release
dotnet publish Backend.Api/Backend.Api.csproj -c Release
docker compose config
```

## ประเด็นที่ต้องบันทึกเมื่อเจอ

- command ในบทใช้ไม่ได้จริง
- chapter ใช้ไฟล์หรือ class ที่ยังไม่เคยสร้าง
- expected response ไม่ตรงกับ runtime
- migration หรือ model snapshot ไม่ตรง
- test fail หลังแก้ chapter
- Docker config ไม่ตรงกับ docs
- secret หรือ connection string จริงหลุดเข้า tracked files
