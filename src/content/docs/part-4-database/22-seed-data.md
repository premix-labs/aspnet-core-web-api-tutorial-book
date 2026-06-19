---
title: 22 - Seed ข้อมูลเริ่มต้น
description: เพิ่มข้อมูลเริ่มต้นสำหรับทดสอบฐานข้อมูลโดยไม่ต้องกรอกเองทุกครั้ง
---

Seed data คือข้อมูลเริ่มต้นที่ application สร้างให้เอง เช่น user สำหรับทดสอบ หรือค่าพื้นฐานที่ระบบต้องมี

ในบทนี้เราจะ seed user ตัวอย่างสำหรับทดสอบ CRUD และฐานข้อมูล ส่วนการ seed admin ที่ login ได้จริงจะทำหลังจากเรียน password hashing ในภาค Authentication

## ทำไมยังไม่ seed admin login จริง

Admin ที่ login ได้จริงต้องมี `PasswordHash` ที่ถูกสร้างด้วย password hasher

ถ้าเรา seed admin ตอนนี้ด้วย password ปลอมหรือ hash ที่ไม่ถูกต้อง หนังสือจะพาผู้อ่านไปสู่ code ที่แก้ยากในภายหลัง

ดังนั้นบทนี้จะ seed เฉพาะข้อมูลทดสอบ database ก่อน แล้วค่อยสร้าง admin seed ที่ถูกต้องในภาค Auth

## สร้าง DataSeeder

สร้างไฟล์

```text
Data/DataSeeder.cs
```

เพิ่ม code นี้

```csharp
using Microsoft.EntityFrameworkCore;
using Backend.Api.Models;

namespace Backend.Api.Data;

public class DataSeeder(AppDbContext db)
{
    public async Task SeedAsync()
    {
        var hasUsers = await db.Users.AnyAsync();

        if (hasUsers)
        {
            return;
        }

        db.Users.AddRange(
            new User
            {
                Email = "demo-user@example.com",
                PasswordHash = "pending-auth",
                Role = "User",
                IsActive = true
            },
            new User
            {
                Email = "inactive-user@example.com",
                PasswordHash = "pending-auth",
                Role = "User",
                IsActive = false
            });

        await db.SaveChangesAsync();
    }
}
```

## ลงทะเบียน DataSeeder

เปิด `Program.cs` แล้วเพิ่ม

```csharp
builder.Services.AddScoped<DataSeeder>();
```

## เรียกใช้ DataSeeder ตอน application start

หลังสร้าง `app` แล้ว ก่อน `app.Run()` ให้เพิ่ม code นี้

```csharp
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}
```

ตำแหน่งโดยรวมจะประมาณนี้

```csharp
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

## ทำไมต้อง CreateScope

`AppDbContext` ถูกลงทะเบียนเป็น scoped service หมายถึงต้องถูกใช้งานภายใน scope

ตอน application start ยังไม่มี HTTP request scope ให้ใช้ เราจึงสร้าง scope เองด้วย `app.Services.CreateScope()`

## ระวังเรื่อง migration

DataSeeder ต้องใช้ table ที่มีอยู่แล้ว ดังนั้นให้รัน migration ก่อน

```powershell
dotnet tool run dotnet-ef database update
```

ถ้า table ยังไม่ถูกสร้าง seeder จะ error ตอน query `db.Users.AnyAsync()`

## ทดสอบ seed data

รัน application

```powershell
dotnet run
```

เปิด endpoint นี้

```text
GET /api/users
```

ควรเห็น user ตัวอย่างสองรายการ

ถ้ารัน application ซ้ำ user ไม่ควรถูกเพิ่มซ้ำ เพราะ seeder ตรวจ `AnyAsync()` ก่อนแล้ว

## Checkpoint

ก่อนจบภาคฐานข้อมูล ให้ตรวจว่าทำได้ครบตามนี้

- มี `DataSeeder`
- ลงทะเบียน `DataSeeder` ใน `Program.cs`
- เรียก `SeedAsync()` ตอน application start
- seed data ไม่ถูกสร้างซ้ำทุกครั้งที่รัน
- เข้าใจว่า admin seed จริงจะทำหลังจากมี password hasher
