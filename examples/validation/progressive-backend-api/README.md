# Progressive Validation Project

โปรเจกต์นี้ใช้ตรวจว่าหนังสือทำตามได้จริงจากโปรเจกต์ว่าง โดยค่อย ๆ ต่อ code ตามลำดับบท และใช้เป็น validation project สำหรับเทียบกับ final project

สถานะล่าสุด: ตรวจถึงบทที่ 58 แล้ว และปรับ end-state ให้ใช้ production model เดียวกับ final project

## สิ่งที่ตรวจแล้ว

- สร้าง ASP.NET Core Web API ด้วย `--use-controllers`
- ลบ WeatherForecast template
- สร้าง `UsersController`
- ทำ CRUD endpoint ด้วย in-memory repository
- แยก `Model`, `Repository`, `Service`, `DTO`
- ลงทะเบียน Dependency Injection
- ทำ manual mapping จาก `User` เป็น `UserResponse`
- เปลี่ยนจาก in-memory repository เป็น EF Core repository
- เพิ่ม `AppDbContext`, migration และ SQL Server connection string
- เพิ่ม `DataSeeder` สำหรับ user ตัวอย่าง
- เพิ่ม Data Annotations และ custom validation สำหรับ request DTO
- เพิ่ม global exception handler และ `ProblemDetails`
- เพิ่ม error response ที่มี `code` และ `traceId`
- เพิ่ม register/login, password hashing และ JWT
- เพิ่ม email verification และ reset password พร้อม token hash, resend endpoint, revoke refresh token หลัง reset และ test email sender
- เพิ่ม `GET /api/auth/me`
- ป้องกัน `UsersController` ด้วย `[Authorize]`
- เพิ่ม admin role และ `AdminUsersController`
- เพิ่ม admin user list, change role/status, self-protection และ audit log
- เพิ่ม pagination/filtering/sorting สำหรับ admin users
- เพิ่ม structured logging, configuration/env, appsettings แยก environment และ OpenAPI metadata
- เพิ่ม xUnit unit test และ WebApplicationFactory integration test
- เพิ่ม Dockerfile, Docker Compose และ publish/Docker smoke test
- ปรับ Dockerfile ให้ใช้ multi-stage runtime image และรัน container ด้วย non-root user
- เพิ่ม production hardening: normalized email, unique index, RowVersion, refresh token rotation/revoke/reuse detection, session/device management, account lockout, audit log สำหรับ auth/security events, SQL Server transient retry, CORS, rate limiting, security headers, health checks และ integration tests สำหรับ hardening
- เพิ่ม production query indexes สำหรับ admin user list, audit log lookup และ token lookup พร้อม metadata tests
- ปรับ `Guid Id`, `DateTimeOffset`, `PATCH` admin endpoint และ query `search` ให้ตรงกับ final project
- ทดสอบ CRUD runtime กับ SQL Server ผ่าน

## คำสั่งตรวจ

```powershell
cd examples/validation/progressive-backend-api
dotnet restore
dotnet tool restore
dotnet build
dotnet test
```

คำสั่งตรวจ production-ready:

```powershell
dotnet build -c Release
dotnet test -c Release
dotnet publish Backend.Api\Backend.Api.csproj -c Release -o Backend.Api\publish
docker build -t backend-api:validation .\Backend.Api
copy .env.example .env
docker compose up --build -d
docker compose down -v
```

คำสั่ง migration:

```powershell
cd examples/validation/progressive-backend-api/Backend.Api
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BackendApiValidationDb;User Id=sa;Password=Replace_With_Strong_Local_Password_123!;TrustServerCertificate=True;"
$env:Jwt__SigningKey="replace-with-local-development-signing-key-at-least-32-bytes"
dotnet tool run dotnet-ef database update
```

## Endpoint สำคัญ

```text
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}

POST   /api/auth/register
POST   /api/auth/login
POST   /api/auth/verify-email
POST   /api/auth/resend-email-verification
POST   /api/auth/forgot-password
POST   /api/auth/reset-password
POST   /api/auth/refresh
POST   /api/auth/revoke
GET    /api/auth/me
GET    /api/auth/sessions
DELETE /api/auth/sessions/{familyId}
DELETE /api/auth/sessions

GET    /api/admin/users
GET    /api/admin/users/{id}
PATCH  /api/admin/users/{id}/role
PATCH  /api/admin/users/{id}/status
```

หมายเหตุ: `/api/users` เป็น endpoint CRUD จากบทต้น ๆ แต่ใน end-state นี้ถูกจำกัดให้ใช้ได้เฉพาะ `Admin` เพื่อไม่ให้ user ปกติอ่านหรือแก้ไขข้อมูลผู้ใช้อื่น

## Smoke Test ล่าสุด

```text
SeededCount                                    2
GET /api/users                                 200
POST /api/users                                201
POST /api/users email ซ้ำ                     409
POST /api/users email ผิดรูปแบบ               400
POST /api/users domain ต้องห้าม               400
GET /api/users/{id}                            200
GET /api/users/{id} ไม่พบ                     404
PUT /api/users/{id}                            200
PUT /api/users/{id} email ซ้ำ                 409
DELETE /api/users/{id}                         204
GET /api/users/{id} หลังลบ                    404
POST /api/auth/login                           200
POST /api/auth/verify-email                    200
POST /api/auth/resend-email-verification       200
POST /api/auth/forgot-password                 204
POST /api/auth/reset-password                  204
GET /api/auth/me ไม่มี token                   401
GET /api/auth/me พร้อม token                   200
GET /api/users ไม่มี token                     401
GET /api/users พร้อม user token                403
GET /api/users พร้อม admin token               200
POST /api/auth/login password ผิด              401
POST /api/auth/login inactive                  403
POST /api/auth/register                        200
GET /api/admin/users/ping ไม่มี token          401
GET /api/admin/users/ping user token           403
GET /api/admin/users/ping admin token          200
GET /api/admin/users                           200
PATCH /api/admin/users/{id}/role               200
PATCH /api/admin/users/{id}/status             200
AuditLogs หลังเปลี่ยน role/status             2 records
dotnet test                                    49 passed
published app /api/auth/me                     401
Docker image /api/auth/me                      401
Docker Compose /api/auth/me                    401
```

หมายเหตุ: ชื่อโฟลเดอร์ยังเป็น `progressive-backend-api` เพราะเริ่มสร้างตอนตรวจบท 1-15 แต่โปรเจกต์เดียวกันนี้ใช้ตรวจต่อเนื่องจนถึง end-state ล่าสุดของหนังสือ
