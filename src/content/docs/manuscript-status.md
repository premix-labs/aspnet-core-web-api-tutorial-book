---
title: สถานะต้นฉบับ
description: สถานะการเขียนและตรวจสอบเนื้อหาของหนังสือ ASP.NET Core Web API
---

หน้านี้ใช้ติดตามว่าส่วนไหนของหนังสือพร้อมแล้ว ส่วนไหนยังต้องขัดเกลา และส่วนไหนตรวจด้วยโปรเจกต์จริงแล้ว

## สถานะปัจจุบัน

- โครงเว็บไซต์หนังสือ: เสร็จแล้ว
- แผนสารบัญทั้งเล่ม 58 บท: เสร็จแล้ว
- ภาค 1 พื้นฐาน: ขยายรายละเอียดรอบแรกแล้ว
- ภาค 2 Controller และ REST API: ขยายรายละเอียดรอบแรกแล้ว
- ภาค 3 Architecture: ขยายรายละเอียดรอบแรกแล้ว
- ภาค 4 EF Core และฐานข้อมูล: ขยายรายละเอียดรอบแรกแล้ว
- ภาค 5 Validation และ Error Handling: ขยายรายละเอียดรอบแรกแล้ว
- ภาค 6 Authentication ด้วย JWT: ขยายรายละเอียดรอบแรกแล้ว
- ภาค 7 Admin API และ Authorization: ขยายรายละเอียดรอบแรกแล้ว
- ภาค 8 Production Ready: ขยายรายละเอียดรอบแรกแล้ว
- ภาค 9 Production Hardening: เพิ่มและตรวจรอบแรกแล้ว
- Final project: สร้างโปรเจกต์จริงแล้ว
- Validation project บท 1-15: ทำตามและ smoke test ผ่านแล้ว
- Validation project บท 16-22: ทำตาม migration/database/CRUD และ smoke test ผ่านแล้ว
- Validation project บท 23-27: ทำตาม validation/error handling และ smoke test ผ่านแล้ว
- Validation project บท 28-33: ทำตาม authentication/JWT และ smoke test ผ่านแล้ว
- Validation project บท 34-40: ทำตาม admin/authorization/audit log/pagination และ smoke test ผ่านแล้ว
- Validation project บท 41-50: ทำตาม production-ready/test/Docker/publish และ smoke test ผ่านแล้ว
- Validation project บท 51-58: ทำตาม production hardening, email verification และ reset password แล้ว
- การตรวจ build สำหรับ GitHub Pages: ผ่านแล้ว
- การเตรียม metadata สำหรับ repository `aspnet-core-web-api-tutorial-book`: เสร็จแล้ว
- การตรวจภาพรวมก่อน deploy: ผ่านแล้ว

## สิ่งที่แก้ล่าสุด

- สร้างโปรเจกต์ validation ที่ `examples/validation/progressive-backend-api` แล้วทำตามบท 1-15 ด้วย code จริง
- ตรวจ CRUD runtime ของบท 1-15 ผ่านครบ `200`, `201`, `204` และ `404`
- ทำตามบท 16-22 ต่อจาก validation project ด้วย EF Core, SQL Server, migration, repository แบบ async และ seed data
- ปรับบท 16, 20, 22 และคำสั่ง migration ในบทหลัง ๆ ให้ใช้ `dotnet-ef` แบบ local tool ผ่าน `dotnet tool run dotnet-ef`
- ทำตามบท 23-27 ต่อจาก validation project ด้วย Data Annotations, custom validation, global exception handler และ ProblemDetails
- ปรับบท 21, 23 และ 24 ให้ flow `Controller -> Service -> Repository` ต่อเนื่อง ไม่กลับไปเรียก repository จาก Controller โดยตรง
- ปรับบท 25 ให้ใช้ `IProblemDetailsService.TryWriteAsync` และบท 26 ให้ validation error มี `traceId`
- ทำตามบท 28-33 ต่อจาก validation project ด้วย register, login, password hashing, JWT, current user และ `[Authorize]`
- ปรับบท 29 ให้ `DataSeeder` อัปเกรด seed เดิมที่ยังเป็น `pending-auth` และแก้ตัวอย่าง `UserResponse` ให้ตรงกับ record
- ปรับบท 31 ให้ติดตั้ง `Microsoft.AspNetCore.Authentication.JwtBearer` version `10.0.9`
- ทำตามบท 34-40 ต่อจาก validation project ด้วย admin role, admin endpoint, user management, self-protection, audit log และ pagination
- ปรับบท 34 ให้ seed admin ด้วย `EnsureUserAsync` แทนการ reset database และเพิ่ม `Roles.Normalize`
- ทำตามบท 41-50 ต่อจาก validation project ด้วย structured logging, config/env, OpenAPI metadata, unit/integration test, Dockerfile, Docker Compose และ publish smoke test
- ปรับบท 46 ให้ test config มาก่อน startup ด้วย environment variables และปิด `DataSeeding`
- ปรับบท 47-48 ให้ Docker run/compose ใช้ connection string, `DataSeeding__Enabled=false` และ build context ที่ถูกต้อง
- ตรวจภาพรวมก่อน deploy แล้ว ไม่พบข้อความงานค้างจริง และยืนยันว่าไฟล์ภาษาไทยเป็น Unicode ไทย ไม่ใช่ไฟล์ encoding เสีย
- ปรับบท 10 ให้ใช้ `Backend.Api.http` ให้ตรงกับไฟล์ที่ template สร้างจริง
- ขยายบทที่ 1-5 ให้ละเอียดขึ้นเรื่องภาพรวม Web API, เครื่องมือ, การสร้าง project, โครงสร้างไฟล์ และพื้นฐาน HTTP/REST/JSON
- ขยายบทที่ 6-10 ให้มีขั้นตอนสร้าง Controller, CRUD endpoint, routing, status code และไฟล์ `.http` ที่ทำตามได้จริง
- ขยายบทที่ 11-15 ให้เชื่อมจาก Controller ไปสู่ Service, Repository, DI, DTO, mapping และ response format ที่สอดคล้องกับ final project
- สร้าง final project จริงที่ `examples/final-backend-api`
- เพิ่ม ASP.NET Core Web API project แบบ Controller
- เพิ่ม EF Core SQL Server, migration และ database seed
- เพิ่ม register/login, password hashing, JWT และ current user endpoint
- เพิ่ม admin user management พร้อม role-based authorization
- เพิ่ม audit log, pagination, filtering และ sorting
- เพิ่ม unit test และ integration test
- เพิ่ม Dockerfile, Docker Compose และ README ของ final project
- ปรับ Docker Compose ให้ใช้ host port `18080` เพื่อลดโอกาสชนกับ service อื่น
- ปรับหน้าภาคผนวกโครงสร้าง final project ให้ตรงกับโค้ดจริง

## ผลตรวจล่าสุด

คำสั่งที่ผ่านแล้ว:

```powershell
npm run build
dotnet build
dotnet test
docker compose config
docker compose build
docker compose up -d
```

ตรวจ runtime ผ่านแล้ว:

- `GET http://localhost:18080/health/live` ตอบ `200`
- `POST http://localhost:18080/api/auth/login` ด้วย `admin@example.com / Passw0rd!` ได้ JWT
- `GET http://localhost:18080/api/admin/users` ด้วย admin token ตอบรายการ user ได้

หลังทดสอบเสร็จได้หยุด container ด้วย `docker compose down` แล้ว

ตรวจ validation project บท 1-15 ผ่านแล้ว:

- `GET /api/users` ตอบ `200`
- `POST /api/users` ตอบ `201`
- `PUT /api/users/{id}` ตอบ `200`
- `DELETE /api/users/{id}` ตอบ `204`
- `GET /api/users/{id}` หลังลบตอบ `404`

ตรวจ validation project บท 16-22 ผ่านแล้ว:

- `dotnet tool run dotnet-ef migrations add InitialCreate` สำเร็จ
- `dotnet tool run dotnet-ef database update` สำเร็จ
- seed user เริ่มต้นได้ `2` รายการ
- CRUD กับ SQL Server ตอบ `200`, `201`, `204`, `404` และ duplicate email ตอบ `409`

ตรวจ validation project บท 23-27 ผ่านแล้ว:

- validation error ตอบ `400` พร้อม `VALIDATION_FAILED` และ `traceId`
- custom validation domain ที่ห้ามใช้ตอบ `400`
- not found ตอบ `404` พร้อม `USER_NOT_FOUND`
- duplicate email ตอบ `409` พร้อม `EMAIL_ALREADY_EXISTS`
- API ไม่ส่ง stack trace และ business error ใช้ `ProblemDetails`

ตรวจ validation project บท 28-33 ผ่านแล้ว:

- login ด้วย `demo-user@example.com / User1234!` ตอบ `200` และได้ Bearer JWT
- `GET /api/auth/me` ไม่ส่ง token ตอบ `401`
- `GET /api/auth/me` พร้อม token ตอบข้อมูล current user ได้
- `GET /api/users` ไม่ส่ง token ตอบ `401`
- `GET /api/users` พร้อม token ตอบ `200`
- password ผิดตอบ `401 INVALID_CREDENTIALS`
- inactive user ตอบ `403 USER_INACTIVE`
- register สำเร็จตอบ `200` พร้อม access token/refresh token และไม่ส่ง `passwordHash`

ตรวจ validation project บท 34-40 ผ่านแล้ว:

- admin token เข้า `GET /api/admin/users/ping` ได้ `200`
- ไม่ส่ง token ได้ `401` และ user token ได้ `403`
- `GET /api/admin/users` ตอบ paged response และไม่ส่ง `passwordHash`
- filter/search/sort ทำงาน
- role invalid ตอบ `400 VALIDATION_FAILED`
- self demote/deactivate ตอบ `403`
- เปลี่ยน role/status สำเร็จและเกิด audit log

ตรวจ validation project บท 41-50 ผ่านแล้ว:

- `dotnet build -c Release` ผ่าน 0 warning / 0 error
- `dotnet test -c Release` ผ่าน 26 tests
- `dotnet publish` ผ่าน
- published app smoke test ได้ `401` จาก `/api/auth/me`
- Docker image build ผ่านและ container smoke test ได้ `401`
- Docker Compose `up --build -d` ผ่านและ smoke test ได้ `401`

## งานก่อนเผยแพร่จริงบน GitHub Pages

- ตั้งค่า GitHub Pages ใน repository เป็น Source: GitHub Actions
- push repository ชื่อ `aspnet-core-web-api-tutorial-book` ขึ้น branch `main`
- ตรวจ workflow `Deploy to GitHub Pages`
- เปิดเว็บจริงจาก GitHub Pages แล้วตรวจหน้าแรก, sidebar, search และบท 1-58
- งาน polish เช่น เพิ่มภาพประกอบ flow หรือเกลาภาษาไทย ทำเพิ่มได้ แต่ไม่ใช่ blocker ของการ deploy

## Definition of Done

หนังสือจะถือว่าพร้อมเผยแพร่เมื่อผ่านเงื่อนไขเหล่านี้

- ทุกบทมีเป้าหมาย ชุดคำสั่ง ตัวอย่าง code และ checkpoint
- ผู้อ่านสามารถทำตามตั้งแต่บทแรกจนบทสุดท้ายได้โดยไม่ต้องเดา
- Final project build, test และ Docker run ผ่าน
- คำสั่งในหนังสือใช้ได้จริงบน Windows
- มีคำอธิบาย error ที่มือใหม่เจอบ่อย
- เว็บไซต์ build ผ่านด้วย `npm run build`
- GitHub Pages workflow พร้อมใช้งาน

## Production Hardening ที่เพิ่มล่าสุด

- เพิ่มภาค 9 บท 51-58 เพื่อยกระดับจาก production-ready พื้นฐานเป็น production hardening
- final project เพิ่ม normalized email, unique index, refresh token rotation, token revoke, refresh token reuse detection, email verification, reset password, account lockout, audit log สำหรับ auth/security events, SQL Server transient retry, rate limiting, CORS policy, security headers และ liveness/readiness health checks พร้อม integration tests
- เพิ่ม migration `ProductionHardening` พร้อม data migration สำหรับ `NormalizedEmail` เพื่อรองรับฐานข้อมูลที่มี user เดิม
- เพิ่ม integration tests สำหรับ duplicate email แบบต่างตัวพิมพ์, refresh token rotation/reuse detection, account lockout, login protection, email verification, reset password, audit log, health checks, CORS และ security headers
- เพิ่ม Session / Device Management: `GET /api/auth/sessions`, `DELETE /api/auth/sessions/{familyId}`, `DELETE /api/auth/sessions`, `RefreshToken.UserAgent`, migration `AddRefreshTokenUserAgent` และ audit log สำหรับ revoke session
- ตรวจ Login Protection แล้ว: ใช้ `PasswordHasher<TUser>`, failed login count, account lockout, generic invalid-credentials response, audit log สำหรับ login failed/locked/succeeded และ test reset failed count หลัง login สำเร็จ
- ตรวจ Email Verification Flow แล้ว: token hash, expiry, single-use, resend revoke token เดิม, no-op response สำหรับ missing/verified email, consume token และ audit log `EMAIL_VERIFIED`
- ตรวจ Password Reset Flow แล้ว: missing/inactive email no-op, token hash, expiry, single-use, request ใหม่ revoke token เดิม, reset สำเร็จ revoke refresh token ทั้งหมด, reset lockout state และ audit log `PASSWORD_RESET_COMPLETED`
- ตรวจ Authorization / Role Policy แล้ว: admin endpoints แยก `401` no token และ `403` user token, admin token เข้าได้ และ `UsersController` เดิมใน validation/progressive ถูกจำกัดเป็น admin-only
- ตรวจ Global Exception Handling แล้ว: final/progressive ใช้ `ProblemDetails`/`ValidationProblemDetails` พร้อม `code` และ `traceId`, domain exceptions map status code ถูกต้อง และ tests ตรวจว่าไม่ leak stack trace
- ตรวจ Observability แล้ว: มี `X-Correlation-Id` response header, รับ correlation id จาก request ได้, ใช้ logging scope `CorrelationId`, health checks/readiness มีอยู่ และ runbook อธิบาย monitor/troubleshoot แล้ว
- ตรวจ Configuration / Secrets แล้ว: tracked appsettings ไม่เก็บ connection string หรือ JWT signing key สำหรับ final/progressive end-state, Docker Compose รับ `MSSQL_SA_PASSWORD` และ `JWT_SIGNING_KEY` จาก environment/`.env`, มี `.env.example` และ docs อธิบาย user secrets/secret manager ชัดเจน
- ตรวจ Database / EF Core แล้ว: เพิ่ม production query indexes สำหรับ admin user list และ audit log lookup, เพิ่ม migration `AddProductionQueryIndexes` ทั้งสอง project และเพิ่ม metadata tests สำหรับ indexes/unique constraints สำคัญ
- ตรวจ Docker / Deployment แล้ว: Dockerfile ทั้งสอง project ใช้ runtime image แบบ non-root (`USER 1654`), `.dockerignore` กัน `.env`/publish output และ `docker build` ตรวจแล้วว่า image user เป็น `1654`
- ตรวจ Book Reader Experience แล้ว: บทเนื้อหาทุกบทมี checkpoint, บท 36-40 ใช้ progressive model ที่ผู้อ่านมี ณ จุดนั้น (`int`, `CreatedAtUtc`, `UpdatedAtUtc`, `Email`, `PagedResponse<T>`) และบท 51 อธิบายลำดับย้ายไป production model (`Guid`, `DateTimeOffset`, `CreatedAt`, `NormalizedEmail`) ก่อนใช้ refresh token/session hardening
- validation/progressive project ทำตามภาค 9 แล้วและผ่าน `dotnet test` 51 tests; final project ผ่าน 46 tests
- verification ล่าสุดผ่าน: final/progressive tests, publish ทั้งสอง project, docker build ทั้งสอง image, docker compose config ทั้งสองชุดด้วย env ที่จำเป็น, `npm run build` และ EF pending model changes ทั้งสอง project
- validation/progressive project ปรับ end-state ให้ตรงกับ final project แล้ว: `Guid Id`, `DateTimeOffset`, register/login response พร้อม refresh token, `search` query และ `PATCH` สำหรับ admin role/status
