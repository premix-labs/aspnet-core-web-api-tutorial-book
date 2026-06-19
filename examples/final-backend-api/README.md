# Final Backend API

โปรเจกต์นี้คือ final project สำหรับหนังสือ **ASP.NET Core Web API Tutorial Book**

ระบบตัวอย่างเป็น Backend API ที่รวมเรื่องหลักของหนังสือไว้ในโปรเจกต์เดียว:

- Register และ Login
- Password hashing
- JWT authentication
- Refresh token rotation
- Session/device management
- Email verification
- Forgot/reset password
- Account lockout
- Role-based authorization
- Admin user management
- Validation และ global error handling
- EF Core และ SQL Server
- Audit log
- Pagination, filtering และ sorting
- OpenAPI
- Unit test และ integration test
- Dockerfile และ Docker Compose

## โครงสร้าง

```text
final-backend-api/
  Backend.Api/
    Controllers/
    Data/
    Dtos/
    Exceptions/
    Models/
    Options/
    Repositories/
    Services/
    Migrations/
    Program.cs
    Dockerfile
    Backend.Api.http
  Backend.Api.Tests/
  docker-compose.yml
  dotnet-tools.json
  Backend.Api.slnx
```

## คำสั่งตรวจโปรเจกต์

```powershell
dotnet restore
dotnet tool restore
dotnet build
dotnet test
```

## รันด้วย Docker Compose

คำสั่งนี้จะ build API, เปิด SQL Server, apply migration และ seed user เริ่มต้น

เตรียมไฟล์ `.env` จากตัวอย่างก่อน แล้วเปลี่ยนค่าให้เป็นของเครื่องหรือ environment ของคุณเอง:

```powershell
copy .env.example .env
```

```powershell
docker compose up --build
```

API จะอยู่ที่

```text
http://localhost:18080
```

บัญชีที่ seed มาให้:

```text
admin@example.com / Passw0rd!
user@example.com  / Passw0rd!
```

หยุด container:

```powershell
docker compose down
```

ลบ database volume ด้วย:

```powershell
docker compose down -v
```

## รันแบบ local development

เปิด SQL Server ด้วย Docker:

```powershell
$env:LOCAL_SQL_PASSWORD="Replace_With_Strong_Local_Password_123!"

docker run --name backend-api-sql `
  -e ACCEPT_EULA=Y `
  -e "MSSQL_SA_PASSWORD=$env:LOCAL_SQL_PASSWORD" `
  -p 1433:1433 `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

สร้าง database schema:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BackendApiDb;User Id=sa;Password=$($env:LOCAL_SQL_PASSWORD);TrustServerCertificate=True;"
$env:Jwt__SigningKey="replace-with-local-development-signing-key-at-least-32-bytes"
dotnet tool restore
dotnet tool run dotnet-ef database update --project Backend.Api --startup-project Backend.Api
```

รัน API:

```powershell
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BackendApiDb;User Id=sa;Password=$($env:LOCAL_SQL_PASSWORD);TrustServerCertificate=True;"
$env:Jwt__SigningKey="replace-with-local-development-signing-key-at-least-32-bytes"
$env:Database__SeedOnStartup="true"
dotnet run --project Backend.Api
```

## Endpoint สำคัญ

```text
GET  /health/live
GET  /health/ready
POST /api/auth/register
POST /api/auth/login
POST /api/auth/verify-email
POST /api/auth/resend-email-verification
POST /api/auth/forgot-password
POST /api/auth/reset-password
POST /api/auth/refresh
POST /api/auth/revoke
GET  /api/auth/me
GET  /api/auth/sessions
DELETE /api/auth/sessions/{familyId}
DELETE /api/auth/sessions
GET  /api/admin/users
GET  /api/admin/users/{id}
PATCH /api/admin/users/{id}/role
PATCH /api/admin/users/{id}/status
```

## Production hardening baseline

- normalized email + unique index
- email verification with hashed one-time tokens
- reset password with hashed one-time tokens and refresh token revoke
- audit log for auth/security events and admin role/status changes
- email sender abstraction with development log sender and SMTP sender
- SQL Server transient retry with `EnableRetryOnFailure`
- refresh token rotation/revoke/reuse detection โดยเก็บเฉพาะ token hash พร้อม token family
- session/device management สำหรับดู active refresh token families, revoke session เดียว และ revoke session ทั้งหมด พร้อม audit log
- account lockout เมื่อ login ผิดซ้ำ
- `RowVersion` สำหรับ optimistic concurrency
- production query indexes สำหรับ admin user list, audit log lookup และ token lookup
- CORS config ที่ production ต้องตั้งค่า origin ชัดเจน
- rate limiting สำหรับ auth endpoints
- security headers พื้นฐานพร้อม integration tests
- liveness/readiness health checks
- Docker runtime image แบบ multi-stage และรันด้วย non-root user

ไฟล์ `Backend.Api/Backend.Api.http` มีตัวอย่าง request สำหรับทดสอบจาก Visual Studio Code หรือ Visual Studio

## หมายเหตุเรื่อง production

ไฟล์ `appsettings.json` ไม่เก็บ connection string หรือ JWT signing key แล้ว ให้ส่งค่าลับผ่าน environment variables, user secrets, `.env` ที่ไม่ commit หรือ secret manager ของ platform ก่อนใช้จริงต้องเปลี่ยน `Jwt__SigningKey`, password ของ SQL Server และวิธีจัดการ secret ให้เหมาะกับระบบ deploy จริง
