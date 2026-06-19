---
title: 42 - Configuration และ Environment Variables
description: จัดการค่าตั้งต้นและ secret โดยไม่ hard-code ใน source code
---

Configuration คือค่าที่เปลี่ยนได้ตาม environment เช่น connection string, JWT issuer, JWT signing key และ logging level

ASP.NET Core อ่าน configuration ได้จากหลายแหล่ง เช่น `appsettings.json`, environment variables, command-line arguments และ secret store

## ค่าใดควรเป็น configuration

- database connection string
- JWT issuer
- JWT audience
- JWT signing key
- token expiration
- logging level
- feature flags
- URL ของ service ภายนอก

หลักคิดง่าย ๆ คือถ้าค่านั้นอาจต่างกันระหว่างเครื่อง dev, staging และ production ให้ใส่ใน configuration

## ค่าใดไม่ควร commit ลง Git

- production database password
- JWT signing key จริง
- API key ของ service ภายนอก
- credential ของ cloud provider
- private certificate หรือ private key

ไฟล์ตัวอย่างใน repository ควรมีเฉพาะค่าที่ไม่ใช่ secret หรือ placeholder ส่วนค่า development ที่เป็น secret ให้เก็บใน user secrets หรือ `.env` ที่ไม่ commit

## ใช้ Environment Variable

บน Windows PowerShell ตั้งค่า environment variable ชั่วคราวได้แบบนี้

```powershell
$env:LOCAL_SQL_PASSWORD="Replace_With_Strong_Local_Password_123!"
$env:Jwt__SigningKey="replace-with-local-development-signing-key-at-least-32-bytes"
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BackendApiDb;User Id=sa;Password=$($env:LOCAL_SQL_PASSWORD);TrustServerCertificate=True;"
```

เครื่องหมาย `__` ใช้แทน `:` ใน configuration path

ดังนั้น `Jwt__SigningKey` จะ map ไปที่

```text
Jwt:SigningKey
```

และ `ConnectionStrings__DefaultConnection` จะ map ไปที่

```text
ConnectionStrings:DefaultConnection
```

## ใช้ User Secrets สำหรับ local development

สำหรับค่า secret ในเครื่อง dev สามารถใช้ user secrets ได้

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:SigningKey" "replace-with-local-development-signing-key-at-least-32-bytes"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=BackendApiDb;User Id=sa;Password=Replace_With_Strong_Local_Password_123!;TrustServerCertificate=True;"
```

User secrets ไม่ได้ encrypt secret แต่ช่วยไม่ให้ secret ถูก commit เข้า repository

## Validate configuration ตอน start

ในบท JWT เราเช็ก signing key ด้วย code แล้ว

```csharp
if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey) ||
    jwtOptions.SigningKey.Length < 32)
{
    throw new InvalidOperationException("Jwt signing key must be at least 32 characters.");
}
```

แนวคิดสำคัญคือถ้า config สำคัญหาย application ควร fail ตอน start ไม่ใช่รอไป error ตอนมี request จริง

## ตัวอย่าง appsettings.json ที่ปลอดภัยกว่า

```json
{
  "Jwt": {
    "Issuer": "Backend.Api",
    "Audience": "Backend.ApiClient",
    "ExpirationMinutes": 60
  }
}
```

สังเกตว่าไม่มี `SigningKey` และไม่มี connection string ในไฟล์นี้ ค่าเหล่านี้ให้มาจาก user secrets, environment variable หรือ secret store แทน

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- เข้าใจว่า `__` ใน environment variable map เป็น `:`
- ย้าย `Jwt:SigningKey` ออกจาก source code ได้
- ใช้ `dotnet user-secrets` สำหรับ local secret ได้
- application fail เร็วถ้า config สำคัญหาย
