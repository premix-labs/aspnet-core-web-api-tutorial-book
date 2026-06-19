---
title: 43 - appsettings หลาย environment
description: ใช้ appsettings.Development และ appsettings.Production ให้ถูกหน้าที่
---

ASP.NET Core รองรับ configuration แยกตาม environment เช่น Development, Staging และ Production

แนวทางนี้ช่วยให้เราแยกค่าที่ใช้ตอนพัฒนาออกจากค่าที่ใช้ตอน deploy จริง

## ไฟล์ที่ใช้บ่อย

```text
appsettings.json
appsettings.Development.json
appsettings.Production.json
```

`appsettings.json` เก็บค่ากลาง

`appsettings.Development.json` override ค่าตอน development

`appsettings.Production.json` ใช้ค่าที่เหมาะกับ production แต่ยังไม่ควรใส่ secret จริงลงไป

connection string, JWT signing key, SMTP password และ API key ไม่ควรอยู่ใน `appsettings.json`, `appsettings.Development.json` หรือ `appsettings.Production.json` ของ repository ให้ส่งผ่าน user secrets, environment variables หรือ secret manager แทน

## ตัวอย่าง appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "Jwt": {
    "Issuer": "Backend.Api",
    "Audience": "Backend.ApiClient",
    "ExpirationMinutes": 60
  }
}
```

Development อาจเปิด EF Core SQL command เป็น `Information` เพื่อ debug query

## ตัวอย่าง appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "Jwt": {
    "Issuer": "Backend.Api",
    "Audience": "Backend.ApiClient",
    "ExpirationMinutes": 30
  }
}
```

Production ลด token lifetime และลด log ที่ละเอียดเกินไป

## ตั้งค่า environment

ใน PowerShell ตั้งค่า environment ชั่วคราว

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run
```

ถ้ารันใน container ให้ตั้งผ่าน `docker-compose.yml` หรือ platform ที่ deploy

```yaml
environment:
  ASPNETCORE_ENVIRONMENT: Production
```

## ตรวจ environment ใน Program.cs

ตัวอย่างที่เราใช้กับ OpenAPI

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

ใน production ไม่ควรเปิดทุกอย่างเหมือน development โดยไม่คิด เช่น OpenAPI public, SQL verbose log หรือ developer exception page

## launchSettings.json ใช้เฉพาะ local

ไฟล์ `Properties/launchSettings.json` ใช้ช่วยรัน local ใน Visual Studio หรือ `dotnet run`

ไม่ควรพึ่ง `launchSettings.json` ใน production เพราะตอน deploy จริงระบบมักใช้ environment variables จาก host หรือ container แทน

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- เข้าใจหน้าที่ของ `appsettings.json`
- มี `appsettings.Development.json`
- มี `appsettings.Production.json`
- ไม่ใส่ secret production ลง Git
- ไม่ใส่ connection string หรือ JWT signing key จริงลง appsettings ที่ commit
- ใช้ `ASPNETCORE_ENVIRONMENT` เลือก environment ได้
