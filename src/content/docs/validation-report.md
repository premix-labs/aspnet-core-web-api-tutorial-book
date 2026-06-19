---
title: รายงานตรวจทำตามหนังสือ
description: บันทึกผลการทดลองสร้างโปรเจกต์ใหม่และทำตามหนังสือทีละบท
---

หน้านี้ใช้บันทึกผลการทดลองทำตามหนังสือจากศูนย์ เพื่อหาว่าบทไหนขาดคำสั่ง ขาดไฟล์ตัวอย่าง หรือมีคำอธิบายที่อาจทำให้มือใหม่สับสน

## สถานะการตรวจ

- สร้าง repo หนังสือ: เสร็จแล้ว
- สร้างโครงสารบัญ 58 บท: เสร็จแล้ว
- ตรวจ build เว็บไซต์หนังสือ: ผ่านแล้ว
- แก้คำสั่งสร้างโปรเจกต์ให้ใช้ Controller template: เสร็จแล้ว
- ขยายเนื้อหาภาค 1 พื้นฐาน: เสร็จรอบแรก
- ขยายเนื้อหาภาค 2 Controller/REST API: เสร็จรอบแรก
- ขยายเนื้อหาภาค 3 architecture: เสร็จรอบแรก
- ขยายเนื้อหาภาค 4 ฐานข้อมูล: เสร็จรอบแรก
- ขยายเนื้อหาภาค 5 validation/error handling: เสร็จรอบแรก
- ขยายเนื้อหาภาค 6 authentication/JWT: เสร็จรอบแรก
- ขยายเนื้อหาภาค 7 admin/authorization: เสร็จรอบแรก
- ขยายเนื้อหาภาค 8 production ready: เสร็จรอบแรก
- สร้าง final project จริง: เสร็จแล้ว
- ตรวจ final project ด้วย build/test/Docker: ผ่านแล้ว
- ทดลองทำตามบท 1-15 จากโปรเจกต์ว่าง: ผ่านแล้ว
- ทดลองทำตามบท 16-22 จากโปรเจกต์ validation: ผ่านแล้ว
- ทดลองทำตามบท 23-27 จากโปรเจกต์ validation: ผ่านแล้ว
- ทดลองทำตามบท 28-33 จากโปรเจกต์ validation: ผ่านแล้ว
- ทดลองทำตามบท 34-40 จากโปรเจกต์ validation: ผ่านแล้ว
- ทดลองทำตามบท 41-50 จากโปรเจกต์ validation: ผ่านแล้ว
- ทดลองทำตามทีละบทจากโปรเจกต์ว่างเต็มระบบ 1-50: ผ่านแล้วด้วย validation project เดียวที่ต่อจากบท 1 ถึง 50

## ผลตรวจบท 1-15 ล่าสุด

โปรเจกต์ที่ใช้ตรวจ:

```text
examples/validation/progressive-backend-api/Backend.Api
```

ขั้นตอนที่ตรวจแล้ว:

```text
บท 03: dotnet new webapi -n Backend.Api --use-controllers
บท 06: สร้าง UsersController
บท 07: เพิ่ม CRUD endpoint ด้วย in-memory list
บท 10: ใช้ Backend.Api.http ทดสอบ request
บท 11-12: แยก Model, Repository, Service และลงทะเบียน DI
บท 13-14: แยก DTO และทำ manual mapping
บท 15: ตรวจ response format และ status code ของ CRUD flow
```

คำสั่งที่ผ่าน:

```powershell
dotnet build
```

ผล runtime smoke test:

```text
GET /api/users               200
POST /api/users              201
GET /api/users/{id}          200
PUT /api/users/{id}          200
DELETE /api/users/{id}       204
GET /api/users/{id} หลังลบ   404
```

ประเด็นที่พบในรอบนี้:

```text
บท: 10 - ทดสอบ API ด้วย REST Client หรือ Postman
ปัญหา: หนังสือใช้ชื่อไฟล์ requests.http แต่ template ของ ASP.NET Core สร้างไฟล์ Backend.Api.http มาให้
การแก้ไข: ปรับบท 10 ให้ใช้ Backend.Api.http เป็นหลัก และบอกว่า requests.http เป็นชื่อทางเลือกได้
สถานะ: แก้แล้ว
```

## ผลตรวจบท 16-22 ล่าสุด

โปรเจกต์ที่ใช้ตรวจ:

```text
examples/validation/progressive-backend-api/Backend.Api
```

ขั้นตอนที่ตรวจแล้ว:

```text
บท 16: ติดตั้ง EF Core SQL Server และ EF Core Design
บท 16: ติดตั้ง dotnet-ef เป็น local tool version 10.0.9
บท 17: ปรับ User entity ให้มี PasswordHash, Role, IsActive และเวลา created/updated
บท 18: สร้าง AppDbContext และ mapping table Users
บท 19: เพิ่ม DefaultConnection ใน appsettings.json
บท 20: สร้าง migration InitialCreate และ update database
บท 21: เปลี่ยน repository จาก in-memory เป็น EF Core async CRUD
บท 22: เพิ่ม DataSeeder และ seed user ตัวอย่าง 2 รายการ
```

คำสั่งที่ผ่าน:

```powershell
dotnet build
dotnet tool run dotnet-ef --version
dotnet tool run dotnet-ef migrations add InitialCreate
dotnet tool run dotnet-ef database update
```

ผล runtime smoke test กับ SQL Server:

```text
SeededCount                      2
GET /api/users                   200
POST /api/users                  201
POST /api/users email ซ้ำ        409
GET /api/users/{id}              200
PUT /api/users/{id}              200
DELETE /api/users/{id}           204
GET /api/users/{id} หลังลบ       404
```

ประเด็นที่พบในรอบนี้:

```text
บท: 16 และ 20 - dotnet-ef
ปัญหา: เครื่องตรวจมี global dotnet ef version 9.0.8 แต่โปรเจกต์ใช้ EF Core 10.0.9 ทำให้เสี่ยงเจอ warning หรือ error ตอนทำ migration
การแก้ไข: ปรับหนังสือให้ใช้ local tool manifest และรันผ่าน dotnet tool run dotnet-ef เพื่อให้ version ตรงกับโปรเจกต์
สถานะ: แก้แล้ว
```

## ผลตรวจบท 23-27 ล่าสุด

โปรเจกต์ที่ใช้ตรวจ:

```text
examples/validation/progressive-backend-api/Backend.Api
```

ขั้นตอนที่ตรวจแล้ว:

```text
บท 23: เพิ่ม Data Annotations ใน CreateUserRequest และ UpdateUserRequest
บท 24: เพิ่ม custom validation ด้วย IValidatableObject
บท 25: เพิ่ม ApiException, NotFoundException, ConflictException และ GlobalExceptionHandler
บท 26: ปรับ ValidationProblemDetails และ ProblemDetails ให้มี code และ traceId
บท 27: ตรวจ status code ของ validation, not found, conflict, create, delete
```

คำสั่งที่ผ่าน:

```powershell
dotnet build
dotnet tool run dotnet-ef database update
```

ผล runtime smoke test กับ SQL Server:

```text
POST /api/users email ผิดรูปแบบ       400 VALIDATION_FAILED traceId=true
POST /api/users domain ที่ห้ามใช้      400 VALIDATION_FAILED traceId=true
GET /api/users/999999                 404 USER_NOT_FOUND traceId=true
POST /api/users email ซ้ำ             409 EMAIL_ALREADY_EXISTS traceId=true
POST /api/users email ใหม่            201
PUT /api/users/{id} email ซ้ำ         409 EMAIL_ALREADY_EXISTS
DELETE /api/users/{id}                204
DELETE /api/users/{id} หลังลบ         404 USER_NOT_FOUND
```

ประเด็นที่พบในรอบนี้:

```text
บท: 21, 23 และ 24 - ลำดับ Service/Repository
ปัญหา: เอกสารบางช่วงพา Controller กลับไปเรียก repository โดยตรง หรือบอกให้สร้าง DTO/UserService ซ้ำ ทั้งที่ภาค Architecture สอน Service layer ไปแล้ว
การแก้ไข: ปรับบท 21 ให้ UserService เรียก repository และ Controller เรียก IUserService ต่อเนื่อง รวมถึงปรับบท 23-24 ให้เป็นการตรวจ/ปรับไฟล์เดิมแทนการสร้างซ้ำ
สถานะ: แก้แล้ว
```

```text
บท: 25 - GlobalExceptionHandler
ปัญหา: ตัวอย่างใช้ problemDetailsService.WriteAsync ซึ่งไม่ตรงกับ API ที่ build ผ่านจริงในโปรเจกต์นี้
การแก้ไข: เปลี่ยนเป็น problemDetailsService.TryWriteAsync
สถานะ: แก้แล้ว
```

```text
บท: 26 - traceId ใน validation error
ปัญหา: AddProblemDetails เพิ่ม traceId ให้ business error แต่ validation error ที่สร้างจาก InvalidModelStateResponseFactory ยังไม่มี traceId
การแก้ไข: เพิ่ม traceId เข้า ValidationProblemDetails ใน InvalidModelStateResponseFactory โดยตรง
สถานะ: แก้แล้ว
```

## ผลตรวจบท 28-33 ล่าสุด

โปรเจกต์ที่ใช้ตรวจ:

```text
examples/validation/progressive-backend-api/Backend.Api
```

ขั้นตอนที่ตรวจแล้ว:

```text
บท 28: เพิ่ม DTO สำหรับ Register, Login, LoginResponse และ CurrentUserResponse
บท 29: เพิ่ม PasswordHasher, AuthService.RegisterAsync, AuthController และปรับ DataSeeder
บท 30: เพิ่ม UnauthorizedException, ForbiddenException และ AuthService.LoginAsync
บท 31: เพิ่ม JwtOptions, JwtTokenService, JwtBearer authentication และ middleware
บท 32: เพิ่ม CurrentUserService และ GET /api/auth/me
บท 33: ป้องกัน UsersController ด้วย [Authorize]
```

คำสั่งที่ผ่าน:

```powershell
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.9
dotnet build
dotnet tool run dotnet-ef database update
```

ผล runtime smoke test กับ SQL Server:

```text
POST /api/auth/login demo-user          200 accessToken + refreshToken
GET /api/auth/me ไม่ส่ง token           401
GET /api/auth/me พร้อม token            200 demo-user@example.com role=User
GET /api/users ไม่ส่ง token             401
GET /api/users พร้อม token              200
POST /api/auth/login password ผิด       401 INVALID_CREDENTIALS
POST /api/auth/login inactive user      403 USER_INACTIVE
POST /api/auth/register                 200 accessToken + refreshToken และไม่ส่ง passwordHash
POST /api/auth/register email ซ้ำ       409 EMAIL_ALREADY_EXISTS
```

ประเด็นที่พบในรอบนี้:

```text
บท: 29 - DataSeeder หลังเปลี่ยนเป็น password hashing
ปัญหา: ถ้าทำต่อจากบท 22 ฐานข้อมูลมี seed เดิมที่ PasswordHash เป็น pending-auth แล้ว DataSeeder แบบ return เมื่อพบ user จะไม่อัปเกรด hash ทำให้ demo-user login ไม่ผ่าน
การแก้ไข: ปรับ DataSeeder ให้ ensure user ตาม email และอัปเกรด hash เดิมที่ยังเป็น pending-auth
สถานะ: แก้แล้ว
```

```text
บท: 29 - UserResponse
ปัญหา: ตัวอย่าง AuthService ใช้ object initializer กับ UserResponse แต่บทก่อนหน้าสอน UserResponse เป็น record
การแก้ไข: เปลี่ยนตัวอย่างให้สร้าง UserResponse ด้วย record constructor
สถานะ: แก้แล้ว
```

```text
บท: 31 - JwtBearer package
ปัญหา: คำสั่งติดตั้ง package ไม่ล็อก version ทั้งที่โปรเจกต์ target net10.0 และ validation ใช้ package 10.0.9
การแก้ไข: ปรับคำสั่งเป็น dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.9
สถานะ: แก้แล้ว
```

```text
บท: 27, 33 และ 40 - ชื่อไฟล์ HTTP request
ปัญหา: บางบทบอกให้เพิ่ม request ลง requests.http แต่ project template และ validation project ใช้ Backend.Api.http
การแก้ไข: ปรับจุดที่เป็นคำสั่งใช้งานจริงให้ใช้ Backend.Api.http
สถานะ: แก้แล้ว
```

## ผลตรวจบท 34-40 ล่าสุด

โปรเจกต์ที่ใช้ตรวจ:

```text
examples/validation/progressive-backend-api/Backend.Api
```

ขั้นตอนที่ตรวจแล้ว:

```text
บท 34: เพิ่ม Roles constants และ seed admin@example.com
บท 35: เพิ่ม AdminUsersController และ role-based authorization
บท 36: เพิ่ม AdminUserResponse และ AdminUserService สำหรับ list users
บท 37: เพิ่ม endpoint เปลี่ยน role/status
บท 38: เพิ่ม self-protection และ last active admin rule
บท 39: เพิ่ม AuditLog, AuditLogService และ migration AddAuditLogs
บท 40: เพิ่ม pagination/filtering/sorting สำหรับ GET /api/admin/users
```

คำสั่งที่ผ่าน:

```powershell
dotnet build
dotnet tool run dotnet-ef migrations add AddAuditLogs
dotnet tool run dotnet-ef database update
```

ผล runtime smoke test กับ SQL Server:

```text
POST /api/auth/login admin@example.com       200 และได้ admin token
POST /api/auth/login demo-user@example.com   200 และได้ user token
GET /api/admin/users/ping ไม่ส่ง token       401
GET /api/admin/users/ping ด้วย user token    403
GET /api/admin/users/ping ด้วย admin token   200
GET /api/admin/users?page=1&pageSize=20      200 totalItems=3
GET /api/admin/users?role=Admin              200
GET /api/admin/users?search=demo             200
PATCH /api/admin/users/{id}/role invalid role 400 VALIDATION_FAILED
PATCH /api/admin/users/{id}/role             200
PATCH /api/admin/users/{adminId}/role self   403 ADMIN_SELF_DEMOTE_NOT_ALLOWED
PATCH /api/admin/users/{adminId}/status self 403 ADMIN_SELF_DEACTIVATE_NOT_ALLOWED
PATCH /api/admin/users/{id}/status           200
AuditLogs หลังเปลี่ยน role/status           2 records
```

ประเด็นที่พบในรอบนี้:

```text
บท: 34 - Seed admin หลังมี seed เดิม
ปัญหา: ถ้าใช้ AnyAsync แล้ว return เมื่อมี user เดิม ระบบจะไม่สร้าง admin@example.com ในฐานข้อมูลที่ทำต่อจากบทก่อนหน้า
การแก้ไข: ปรับบท 34 ให้ใช้ EnsureUserAsync เพื่อเพิ่ม admin และอัปเกรด seed เดิมแทนการ reset database
สถานะ: แก้แล้ว
```

```text
บท: 34, 37, 38 และ 40 - Role normalization
ปัญหา: Roles.IsValid ตรวจแบบไม่สนตัวพิมพ์เล็กใหญ่ แต่ถ้าบันทึกค่าที่ client ส่งมาตรง ๆ เช่น admin จะเก็บ role ไม่ตรงกับค่า canonical Admin
การแก้ไข: เพิ่ม Roles.Normalize และใช้ตอน update role กับ filter role
สถานะ: แก้แล้ว
```

## ผลตรวจบท 41-50 ล่าสุด

โปรเจกต์ที่ใช้ตรวจ:

```text
examples/validation/progressive-backend-api
```

ขั้นตอนที่ตรวจแล้ว:

```text
บท 41: เพิ่ม structured logging ใน AuthService และ AdminUserService
บท 42: ย้าย Jwt:SigningKey ออกจาก appsettings.json และใช้ environment variables
บท 43: เพิ่ม appsettings.Development.json และ appsettings.Production.json
บท 44: เพิ่ม ProducesResponseType ให้ controller สำคัญ
บท 45: สร้าง solution และ xUnit test project
บท 46: เพิ่ม WebApplicationFactory integration test และปิด DataSeeding ตอน test
บท 47: เพิ่ม .dockerignore และ Dockerfile แบบ multi-stage build
บท 48: เพิ่ม docker-compose.yml สำหรับ API และ SQL Server
บท 49: ตรวจ restore/build/test/publish/run published app/Docker image
บท 50: ตรวจ checklist ก่อน deploy
```

คำสั่งที่ผ่าน:

```powershell
dotnet restore
dotnet build -c Release
dotnet test -c Release
dotnet publish Backend.Api\Backend.Api.csproj -c Release -o Backend.Api\publish
docker build -t backend-api:validation .\Backend.Api
docker compose -p backend-api-validation up --build -d
docker compose -p backend-api-validation down -v --remove-orphans
```

ผลตรวจ:

```text
Release build warnings/errors          0/0
Unit + integration tests               15 passed
Published app smoke /api/auth/me       401
Docker image smoke /api/auth/me        401
Docker Compose smoke /api/auth/me      401
```

ประเด็นที่พบในรอบนี้:

```text
บท: 46 - Integration test
ปัญหา: Test config ที่ใส่ผ่าน ConfigureAppConfiguration มาช้าเกินไป เพราะ Program.cs อ่าน JWT ตั้งแต่ช่วง startup แรก
การแก้ไข: เพิ่ม TestApiFactory ที่ตั้ง environment variables ก่อน app start และปิด DataSeeding ด้วย DataSeeding__Enabled=false
สถานะ: แก้แล้ว
```

```text
บท: 47 - Dockerfile
ปัญหา: docker run ตัวอย่างเดิมไม่ได้ส่ง ConnectionStrings__DefaultConnection และยังเปิด seeding ทำให้ container start ไม่ได้ถ้ายังไม่มี database/migration
การแก้ไข: เพิ่ม environment variables ที่จำเป็นและตั้ง DataSeeding__Enabled=false สำหรับ smoke test image
สถานะ: แก้แล้ว
```

```text
บท: 48 - Docker Compose
ปัญหา: build context เดิมไม่ตรงกับ Dockerfile ที่อยู่ใน API project และขั้นตอน migration ยังไม่ชัดเจน
การแก้ไข: ปรับ context เป็น ./Backend.Api, dockerfile เป็น Dockerfile และเพิ่มลำดับ docker compose up -d db -> dotnet-ef database update -> docker compose up api
สถานะ: แก้แล้ว
```

## ผลตรวจ final project ล่าสุด

โปรเจกต์ที่ตรวจ:

```text
examples/final-backend-api
```

คำสั่งที่ผ่าน:

```powershell
dotnet restore
dotnet tool restore
dotnet build
dotnet test
docker compose config
docker compose build
docker compose up -d
```

ผล test:

```text
Passed: 24
Failed: 0
```

ผล runtime:

```text
GET /health/live                    ผ่าน
GET /health/ready                   ผ่าน
POST /api/auth/login ด้วย admin      ผ่าน
GET /api/admin/users ด้วย admin JWT  ผ่าน
```

หมายเหตุ: ตอนแรกใช้ host port `8080` แล้วชนกับ service อื่นในเครื่อง จึงปรับ Docker Compose เป็น `18080:8080`

## ผลตรวจภาพรวมก่อน deploy

ตรวจรายการที่มักพลาดก่อนเผยแพร่แล้ว:

```text
Astro config สำหรับ GitHub Pages      ผ่าน
GitHub Actions workflow               ผ่าน
ข้อความสถานะงานค้าง                   ไม่พบ
คำสั่ง test project บท 45             ปรับเป็น path .csproj แล้ว
integration test config บท 46          ปรับให้ใช้ environment variables แล้ว
Dockerfile run command บท 47           เพิ่ม config ที่จำเป็นแล้ว
Docker Compose path บท 48              ปรับให้ตรง validation project แล้ว
secret ใน appsettings validation       ไม่พบ Jwt:SigningKey
```

ผลตรวจ encoding: ไฟล์ภาษาไทยเป็น Unicode ไทยจริง ปัญหา mojibake ที่เห็นใน terminal เป็นปัญหาการแสดงผลของ shell ไม่ใช่ไฟล์เสีย

## ประเด็นที่ตรวจพบและแก้แล้ว

```text
บท: 01-05 - Foundation
ปัญหา: ฉบับก่อนยังอธิบายพื้นฐานสั้นเกินไปสำหรับมือใหม่
การแก้ไข: เพิ่มภาพรวม API ในระบบจริง, request pipeline, เครื่องมือที่ต้องติดตั้ง, วิธีอ่าน output ของ dotnet run, โครงสร้างไฟล์ และ HTTP/REST/JSON ที่ใช้ต่อในบท Controller
สถานะ: แก้แล้ว
```

```text
บท: 06-10 - Controller และ REST API
ปัญหา: ฉบับก่อนมีตัวอย่าง CRUD แต่ยังไม่พอให้ผู้อ่านทำตามแบบไม่เดา
การแก้ไข: เพิ่ม UsersController แบบเต็ม, ลำดับทดสอบ CRUD, routing/query string, status code, CreatedAtAction และไฟล์ Backend.Api.http แบบเต็ม
สถานะ: แก้แล้ว
```

```text
บท: 11-15 - Architecture
ปัญหา: ฉบับก่อนยังเป็นแนวคิดสั้น ๆ และยังไม่เชื่อมไป final project ชัดพอ
การแก้ไข: เพิ่ม Controller-Service-Repository flow, DI registration, interface/implementation, DTO แยกตาม use case, manual mapping และแนวทาง response format ด้วย ProblemDetails/PagedResponse
สถานะ: แก้แล้ว
```

```text
บท: 03 - สร้างโปรเจกต์แรก
ปัญหา: ถ้าไม่ระบุ --use-controllers ใน .NET รุ่นใหม่ project อาจไม่ตรงกับหนังสือที่สอน Controller
การแก้ไข: เปลี่ยนคำสั่งเป็น dotnet new webapi -n Backend.Api --use-controllers
สถานะ: แก้แล้ว
```

```text
บท: 22 - Seed ข้อมูลเริ่มต้น
ปัญหา: การ seed admin ที่ login ได้จริงต้องมี password hashing ซึ่งยังไม่ได้สอนจนกว่าจะถึงภาค Authentication
การแก้ไข: บทฐานข้อมูลใช้ seed สำหรับทดสอบ database ก่อน แล้วค่อยทำ admin seed จริงหลังบท password hashing
สถานะ: แก้แล้ว
```

```text
บท: 16 และ 20 - EF Core tools
ปัญหา: การใช้ dotnet ef แบบ global อาจใช้ tool คนละ major version กับ EF Core package ในโปรเจกต์
การแก้ไข: เปลี่ยนตัวอย่างให้ติดตั้ง dotnet-ef เป็น local tool และใช้คำสั่ง dotnet tool run dotnet-ef
สถานะ: แก้แล้ว
```

```text
บท: 21, 23 และ 24 - ความต่อเนื่องของ architecture
ปัญหา: ตัวอย่างบางช่วงไม่ต่อเนื่องกับ flow Controller -> Service -> Repository ที่สอนในภาค Architecture
การแก้ไข: ปรับเนื้อหาให้ Controller เรียก Service ต่อ และให้ Service เป็นคนคุยกับ Repository รวมถึงปรับคำอธิบาย DTO/Service ไม่ให้สร้างซ้ำ
สถานะ: แก้แล้ว
```

```text
บท: 25 และ 26 - Error response format
ปัญหา: ตัวอย่าง global handler ใช้ method ที่ไม่ตรงกับ build จริง และ validation error ยังไม่มี traceId
การแก้ไข: ใช้ TryWriteAsync และเพิ่ม traceId ใน InvalidModelStateResponseFactory
สถานะ: แก้แล้ว
```

```text
บท: 28-33 - Authentication/JWT
ปัญหา: มีช่องว่างเรื่อง seed เดิมที่ยังเป็น pending-auth, ตัวอย่าง UserResponse ไม่ตรงกับ record, package JwtBearer ไม่ล็อก version และชื่อไฟล์ HTTP request ไม่สม่ำเสมอ
การแก้ไข: ปรับ DataSeeder, ตัวอย่าง mapping, คำสั่งติดตั้ง package และชื่อไฟล์ Backend.Api.http แล้วตรวจ register/login/me/[Authorize] ผ่านจริง
สถานะ: แก้แล้ว
```

```text
บท: 34-40 - Admin API และ Authorization
ปัญหา: seed admin ต้องรองรับฐานข้อมูลที่มี seed เดิม และ role ที่ตรวจแบบ case-insensitive ต้อง normalize ก่อนบันทึก
การแก้ไข: ปรับบท 34 ให้ใช้ EnsureUserAsync, เพิ่ม Roles.Normalize และตรวจ admin endpoint/list/change role/status/audit log/pagination ผ่านจริง
สถานะ: แก้แล้ว
```

```text
บท: 23-27 - Validation และ Error Handling
ปัญหา: ฉบับร่างเดิมยังไม่มีแนวทางครบเรื่อง automatic 400, custom validation, exception กลาง และ ProblemDetails
การแก้ไข: เพิ่ม DTO validation, IValidatableObject, ApiException, GlobalExceptionHandler, ValidationProblemDetails และ status code guide
สถานะ: แก้แล้ว
```

```text
บท: 28-33 - Authentication ด้วย JWT
ปัญหา: ฉบับร่างเดิมยังไม่มี package, config, service, middleware และ flow อ่าน current user จาก token
การแก้ไข: เพิ่ม Auth DTO, PasswordHasher<User>, AuthService, JwtOptions, JwtTokenService, AddJwtBearer, CurrentUserService และตัวอย่าง [Authorize]
สถานะ: แก้แล้ว
```

```text
บท: 34-40 - Admin API และ Authorization
ปัญหา: ฉบับร่างเดิมยังไม่มี role constants, admin endpoint, self-protection, audit log และ pagination
การแก้ไข: เพิ่ม Roles constants, AdminUsersController, AdminUserService, change role/status, self-protection, AuditLog และ paged query
สถานะ: แก้แล้ว
```

```text
บท: 41-50 - Production Ready
ปัญหา: ฉบับร่างเดิมยังไม่มีรายละเอียดครบเรื่อง logging, config/env, OpenAPI, test, Docker และ deploy checklist
การแก้ไข: เพิ่ม structured logging, environment variables, appsettings, OpenAPI metadata, unit/integration test, Dockerfile, Docker Compose, publish และ checklist
สถานะ: แก้แล้ว
```

```text
บท: 51-58 - Production Hardening
ปัญหา: หลังเพิ่มภาค 9 ต้องตรวจว่าทำตามบทใหม่แล้ว code ใช้งานได้จริง ไม่ใช่มีเฉพาะ final project
การแก้ไข: ปรับ validation project ให้มี normalized email + unique index, RowVersion, refresh token rotation/revoke/reuse detection, account lockout, SQL Server transient retry, CORS config, rate limiting, security headers, liveness/readiness health checks และเพิ่ม migration ProductionHardening
ผลตรวจ: dotnet test ผ่าน 50 tests ใน validation/progressive project และ 45 tests ใน final project, เพิ่ม test สำหรับ duplicate email แบบต่างตัวพิมพ์, refresh token rotation, refresh token reuse detection, account lockout, email verification, resend verification email, forgot password, reset password, audit log, health checks, CORS preflight, security headers, session/device management, login protection, authorization/role policy, global exception handling, observability/correlation id และ EF idempotent migration script generate ผ่าน

งานปรับ consistency ล่าสุด:
ปรับ validation/progressive project ให้ end-state ใช้ Guid Id, DateTimeOffset, LoginResponse พร้อม refresh token, query search และ PATCH admin endpoint ให้ตรงกับ final project
เพิ่ม migration AlignProgressiveModelWithFinal สำหรับย้าย schema จาก int id ไป Guid id โดยทดสอบ database update กับ SQL Server ผ่านจริง

งาน email verification ล่าสุด:
เพิ่ม EmailVerificationToken, EmailVerifiedAt, IEmailSender, LoggingEmailSender, SmtpEmailSender, verify-email endpoint และ resend-email-verification endpoint ให้ทั้ง final project และ validation/progressive project
เพิ่ม migration EmailVerification พร้อม data backfill สำหรับ user เดิมที่ verified แล้ว และเพิ่ม integration tests สำหรับ register ส่ง verification email, verify token ถูกต้อง/ผิด และ resend verification email

งาน reset password ล่าสุด:
เพิ่ม PasswordResetToken, PasswordResetOptions, forgot-password endpoint และ reset-password endpoint ให้ทั้ง final project และ validation/progressive project
เพิ่ม migration PasswordReset, test reset token ผิด, test reset สำเร็จ, test login ด้วย password ใหม่ และ test refresh token เก่าถูก revoke หลัง reset password

งาน CORS/security headers ล่าสุด:
เพิ่ม security headers ให้ครอบคลุม CSP, COOP, cross-domain policy และ no-store cache policy
เพิ่ม integration tests สำหรับ liveness/readiness health checks, security headers และ CORS preflight ทั้ง final project และ validation/progressive project

งาน audit logging ล่าสุด:
เพิ่ม `AuditActions`, เพิ่ม `IpAddress` ใน `AuditLog`, บันทึก auth/security events เช่น register, login success/fail, account locked, refresh token rotate/revoke, email verification และ password reset
เพิ่ม migration `AddAuditLogIpAddress` และ integration tests ที่ตรวจว่า audit records ถูกสร้างจริงโดยไม่เก็บ token หรือ secret ลง `Detail`

งาน refresh token hardening ล่าสุด:
เพิ่ม `FamilyId` และ `RevocationReason` ใน `RefreshToken`, เพิ่ม reuse detection เมื่อ token ที่ถูก rotate แล้วถูกนำกลับมาใช้ซ้ำ, revoke active token ทั้ง family และบันทึก audit log `REFRESH_TOKEN_REUSE_DETECTED`
เพิ่ม migration `RefreshTokenReuseDetection` และขยาย integration test ให้ตรวจว่า token family ถูก revoke จริง

งาน session/device management ล่าสุด:
เพิ่ม `RefreshToken.UserAgent`, `AuthSessionResponse`, endpoints `GET /api/auth/sessions`, `DELETE /api/auth/sessions/{familyId}` และ `DELETE /api/auth/sessions` ให้ทั้ง final project และ validation/progressive project
เพิ่ม migration `AddRefreshTokenUserAgent`, ตรวจ EF pending model changes แล้วไม่พบ change ค้าง, และเพิ่ม integration tests สำหรับ user agent, list active sessions, revoke session เดียว, ป้องกัน revoke session ของ user อื่น, revoke all sessions, refresh หลัง revoke ต้องใช้ไม่ได้ และ audit log สำหรับ revoke session
ผลตรวจ: `dotnet test` ผ่าน 31 tests ใน final project และ 33 tests ใน validation/progressive project

งาน login protection review ล่าสุด:
ตรวจแล้วว่า final project และ validation/progressive project ใช้ `PasswordHasher<TUser>`, failed login count, account lockout, lockout expiry, generic invalid-credentials response และ audit log สำหรับ login failed/locked/succeeded
เพิ่ม validation ให้ `AccountLockoutOptions` ใน validation/progressive project และเพิ่ม integration tests สำหรับ invalid email ไม่ leak account existence, failed login audit log, successful login reset failed count และ account lock audit log

งาน email verification review ล่าสุด:
ตรวจแล้วว่า register สร้าง user ที่ยังไม่ verified, verification token เก็บเฉพาะ hash, token มี expiry, token ใช้ซ้ำไม่ได้, verify สำเร็จ mark token เป็น used, resend revoke token เดิม และ resend ของ email ที่ไม่มีอยู่หรือ verified แล้วตอบกลาง ๆ โดยไม่ส่ง email เพิ่ม
เพิ่ม integration tests สำหรับ raw token ไม่ถูกเก็บ, expired token, token reuse, resend revoke previous token, resend no-op สำหรับ missing/verified email และ audit log `EMAIL_VERIFIED`

งาน password reset review ล่าสุด:
ตรวจแล้วว่า forgot-password ตอบแบบไม่ leak email existence, user inactive ไม่ได้รับ reset email, reset token เก็บเฉพาะ hash, token มี expiry และ single-use, request ใหม่ revoke token active เดิม, reset สำเร็จ consume token, reset lockout state, revoke refresh token ทั้งหมด และเขียน audit log `PASSWORD_RESET_COMPLETED`
เพิ่ม validation ให้ `PasswordResetOptions` ใน validation/progressive project และเพิ่ม integration tests สำหรับ missing/inactive email no-op, raw reset token ไม่ถูกเก็บ, request ใหม่ revoke token เดิม, expired token, token reuse, revoke refresh token, reset lockout state และ audit log

ผลตรวจล่าสุดหลัง password reset review:
`dotnet test` ผ่าน 42 tests ใน final project และ 44 tests ใน validation/progressive project, `dotnet publish` final ผ่าน, `docker compose config` ผ่านทั้ง final และ validation/progressive, `npm run build` ผ่าน และ `dotnet ef migrations has-pending-model-changes` ไม่พบ pending model changes ทั้งสอง project

งาน authorization/role policy review ล่าสุด:
ตรวจแล้วว่า role constants และ JWT role claim ใช้กับ `[Authorize(Roles = Roles.Admin)]`, admin endpoints แยก 401 สำหรับ no token และ 403 สำหรับ user token, admin token เข้า admin list ได้ และ validation/progressive project จำกัด `UsersController` จากบท CRUD เดิมให้เป็น admin-only เพื่อป้องกัน horizontal privilege escalation
เพิ่ม integration tests สำหรับ admin endpoint no token/user token/admin token และ `/api/users` ใน validation/progressive project ที่ user token ต้องได้ `403` แต่ admin token เข้าได้

ผลตรวจล่าสุดหลัง authorization/role policy review:
`dotnet test` ผ่าน 44 tests ใน final project และ 49 tests ใน validation/progressive project, `dotnet publish` final ผ่าน, `docker compose config` ผ่านทั้ง final และ validation/progressive, `npm run build` ผ่าน และ `dotnet ef migrations has-pending-model-changes` ไม่พบ pending model changes ทั้งสอง project

งาน global exception handling review ล่าสุด:
ปรับ final project ให้ error response ใช้ `ProblemDetails`/`ValidationProblemDetails` พร้อม `code` และ `traceId` แบบเดียวกับ validation/progressive project, เพิ่ม stable error codes ให้ auth, token, admin และ domain exceptions สำคัญ และตรวจว่า response ไม่ส่ง `stackTrace`
เพิ่ม integration assertions สำหรับ validation error และ duplicate email conflict ทั้ง final และ validation/progressive project ให้ตรวจ `code`, `traceId` และไม่ leak stack trace

ผลตรวจล่าสุดหลัง global exception handling review:
`dotnet test` ผ่าน 44 tests ใน final project และ 49 tests ใน validation/progressive project, `dotnet publish` final ผ่าน, `docker compose config` ผ่านทั้ง final และ validation/progressive, `npm run build` ผ่าน และ `dotnet ef migrations has-pending-model-changes` ไม่พบ pending model changes ทั้งสอง project

งาน observability review ล่าสุด:
เพิ่ม `X-Correlation-Id` middleware ให้ทั้ง final project และ validation/progressive project โดยรับ correlation id จาก request header หรือใช้ `HttpContext.TraceIdentifier` เป็นค่า fallback, ส่ง id เดียวกันกลับใน response header และเปิด logging scope `CorrelationId` สำหรับ log ใน request เดียวกัน
เพิ่ม integration tests สำหรับ correlation id response header ทั้งสอง project และอัปเดต runbook ให้ระบุการใช้ `X-Correlation-Id` ตอน troubleshoot production incident

ผลตรวจล่าสุดหลัง observability review:
`dotnet test` ผ่าน 45 tests ใน final project และ 50 tests ใน validation/progressive project, `dotnet publish` final ผ่าน, `docker compose config` ผ่านทั้ง final และ validation/progressive, `npm run build` ผ่าน และ `dotnet ef migrations has-pending-model-changes` ไม่พบ pending model changes ทั้งสอง project

งาน configuration/secrets review ล่าสุด:
ถอด `ConnectionStrings:DefaultConnection` และ `Jwt:SigningKey` ออกจาก tracked appsettings ของ final project, ถอด connection string ออกจาก `appsettings.Development.json` ของ validation/progressive project และให้ runtime secret มาจาก environment variables, user secrets, `.env` ที่ไม่ commit หรือ secret manager แทน
ปรับ Docker Compose ทั้ง final และ validation/progressive ให้ใช้ `${MSSQL_SA_PASSWORD}` และ `${JWT_SIGNING_KEY}`, เพิ่ม `.env.example`, ปรับ README และบท 42, 43, 47, 48, 57 ให้สอน pattern เดียวกัน และเปลี่ยน final `CustomWebApplicationFactory` ให้ตั้ง environment variables ก่อน app startup หลังจากถอด secret ออกจาก appsettings
สแกนซ้ำแล้วไม่พบ password/signing key ตัวอย่างเดิมใน source/docs ที่ไม่ใช่ generated output

ผลตรวจล่าสุดหลัง configuration/secrets review:
`dotnet test` ผ่าน 45 tests ใน final project และ 50 tests ใน validation/progressive project, `dotnet publish` ผ่านทั้ง final และ validation/progressive, `docker compose config` ผ่านทั้งสองชุดเมื่อส่ง env ที่จำเป็น, `npm run build` ผ่าน และ `dotnet ef migrations has-pending-model-changes` ไม่พบ pending model changes ทั้งสอง project

งาน database/EF Core review ล่าสุด:
ตรวจ indexes, unique constraints, delete behavior, DateTimeOffset และ migrations ทั้ง final และ validation/progressive project แล้ว พบว่า token/email constraints สำคัญมีอยู่ แต่ audit log และ admin user query ยังขาด index สำหรับ production lookup
เพิ่ม index ให้ `Users.CreatedAt`, `Users.Role + IsActive + CreatedAt`, `AuditLogs.CreatedAt`, `AuditLogs.ActorUserId + CreatedAt` และ `AuditLogs.EntityName + EntityId + CreatedAt` ทั้งสอง project
เพิ่ม migration `AddProductionQueryIndexes` และเพิ่ม `DatabaseModelTests` เพื่อตรวจ metadata ว่า unique/index สำคัญยังอยู่ครบ

ผลตรวจล่าสุดหลัง database/EF Core review:
`dotnet test` ผ่าน 46 tests ใน final project และ 51 tests ใน validation/progressive project, `dotnet publish` ผ่านทั้ง final และ validation/progressive, `docker compose config` ผ่านทั้งสองชุดเมื่อส่ง env ที่จำเป็น, `npm run build` ผ่าน และ `dotnet ef migrations has-pending-model-changes` ไม่พบ pending model changes ทั้งสอง project

งาน Docker/deployment review ล่าสุด:
ตรวจ Dockerfile, `.dockerignore`, Docker Compose และบท deploy แล้ว พบว่า Dockerfile ยังรัน runtime เป็น root และ `.dockerignore` ยังไม่กัน `.env`/publish output ครบในทุก build context
ปรับ Dockerfile ทั้ง final และ validation/progressive ให้ใช้ multi-stage image เดิมแต่สั่ง `USER 1654` ใน runtime stage, ปรับ `.dockerignore` ให้กัน `.env`, `.env.*` และ publish output, และอัปเดตบท Dockerfile/checklist/README ให้ระบุ non-root container กับ build context hygiene

ผลตรวจล่าสุดหลัง Docker/deployment review:
`docker build` ผ่านทั้ง final และ validation/progressive และ inspect image ได้ `Config.User = 1654` ทั้งสอง image, `dotnet test` ผ่าน 46 tests ใน final project และ 51 tests ใน validation/progressive project, `docker compose config` ผ่านทั้งสองชุดเมื่อส่ง env ที่จำเป็น และ `npm run build` ผ่าน

งาน book reader experience review ล่าสุด:
สแกนบทใน `src/content/docs/part-*` แล้วปรับให้บทเนื้อหาทุกบทมี `Checkpoint`, เพิ่ม checkpoint ให้บท 51-58, และปรับบท 36-40 ให้เดินตาม progressive model ณ จุดนั้นของหนังสือ คือ `User.Id` ยังเป็น `int`, response ยังใช้ `CreatedAtUtc`/`UpdatedAtUtc`, admin search ยังใช้ `Email` ก่อนที่บท 51 จะยกระดับเป็น production user model พร้อม `Guid`, `DateTimeOffset`, `CreatedAt` และ `NormalizedEmail`

ผลตรวจล่าสุดหลัง book reader experience review:
สแกนแล้วไม่พบบทเนื้อหาที่ขาด checkpoint, Part 7 ไม่กระโดดไปใช้ field จาก end-state ก่อนบท 51 และ `npm run build` ผ่าน

งาน GitHub Pages deploy prep ล่าสุด:
ปรับ metadata ของเว็บและ package ให้ใช้ชื่อ repository เป้าหมาย `aspnet-core-web-api-tutorial-book`, ยืนยันว่า GitHub Actions workflow ตั้งค่า `SITE` และ `BASE_PATH` ตามชื่อ repository อัตโนมัติ และเพิ่มคำสั่งตรวจ GitHub Pages base path ใน README
ผลตรวจ: รัน `npm run build` โดยตั้ง `SITE=https://example.github.io` และ `BASE_PATH=/aspnet-core-web-api-tutorial-book` ผ่าน, ตรวจ output แล้ว canonical URL, stylesheet/script assets, sitemap และ search index อยู่ใต้ base path ของ repository ใหม่ถูกต้อง

สถานะ: แก้แล้ว
```

## งานตรวจรอบถัดไป

- ตั้งค่า GitHub Pages ใน repository เป็น Source: GitHub Actions
- push repository ชื่อ `aspnet-core-web-api-tutorial-book` ขึ้น branch `main` แล้วตรวจผล workflow `Deploy to GitHub Pages`
- เปิดเว็บจริงจาก URL ของ GitHub Pages แล้วตรวจหน้าแรก, sidebar, search และบท 1-58
