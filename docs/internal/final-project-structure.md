---
title: โครงสร้าง Final Project
description: ภาพรวมไฟล์และโฟลเดอร์ของโปรเจกต์สุดท้าย
---

# โครงสร้าง Final Project

Final project อยู่ที่โฟลเดอร์:

```text
examples/final-backend-api
```

โครงสร้างจริงของโปรเจกต์ตัวอย่างเป็นแบบ solution มี API project และ test project แยกกัน

```text
final-backend-api/
  .dockerignore
  .env.example
  Backend.Api/
    Constants/
    Controllers/
    Data/
    Dtos/
      Admin/
      Auth/
      Common/
      Users/
    Exceptions/
    Migrations/
    Models/
    Options/
    Repositories/
    Services/
    Program.cs
    appsettings.json
    appsettings.Development.json
    Dockerfile
    Backend.Api.http
  Backend.Api.Tests/
    AuditLogIntegrationTests.cs
    AuthIntegrationTests.cs
    CustomWebApplicationFactory.cs
    DatabaseModelTests.cs
    ProductionHardeningIntegrationTests.cs
    RolesTests.cs
    TestEmailSender.cs
  docker-compose.yml
  dotnet-tools.json
  README.md
  Backend.Api.slnx
```

โครงสร้างนี้แยกหน้าที่หลักออกจากกันชัดเจน:

- `Controllers` รับ HTTP request และส่ง response
- `Services` เก็บ business logic
- `Repositories` ติดต่อฐานข้อมูลผ่าน EF Core
- `Dtos` กำหนด request/response contract
- `Exceptions` รวม exception และ global error handler
- `Migrations` เก็บ schema version ของฐานข้อมูล
- `Options` เก็บ configuration classes เช่น JWT, lockout, email และ CORS options
- `Backend.Api.Tests` เก็บ unit test และ integration test
- `.env.example` บอกชื่อ environment variables ที่ต้องมีโดยไม่เก็บ secret จริง
- `.dockerignore` กันไฟล์ที่ไม่ควรเข้า Docker build context เช่น `.env`, `bin`, `obj` และ publish output

คำสั่งตรวจโปรเจกต์ตัวอย่าง:

```powershell
cd examples/final-backend-api
dotnet restore
dotnet tool restore
dotnet build
dotnet test
docker compose config
docker compose build
```

ถ้ารันด้วย Docker Compose API จะเปิดที่ `http://localhost:18080`
