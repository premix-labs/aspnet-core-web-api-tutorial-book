---
title: 34 - ออกแบบ Role และ Permission
description: สร้าง role constants และปรับ seed data ให้มี admin สำหรับทดสอบ
---

Authentication ตอบคำถามว่า "คุณเป็นใคร" ส่วน Authorization ตอบคำถามว่า "คุณมีสิทธิ์ทำอะไร"

ในภาคที่แล้ว JWT ของเรามี claim ชื่อ `role` แล้ว ภาคนี้จะใช้ role นั้นกับ `[Authorize(Roles = ...)]`

## สร้าง Role Constants

การเขียน string role กระจายหลายไฟล์ เช่น `"Admin"` และ `"User"` ทำให้พิมพ์ผิดง่าย

ให้สร้างโฟลเดอร์

```text
Constants/
```

สร้างไฟล์

```text
Constants/Roles.cs
```

เพิ่ม code นี้

```csharp
namespace Backend.Api.Constants;

public static class Roles
{
    public const string User = "User";
    public const string Admin = "Admin";

    public static readonly string[] All = [User, Admin];

    public static bool IsValid(string role)
    {
        return All.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public static string Normalize(string role)
    {
        return All.FirstOrDefault(
            value => value.Equals(role, StringComparison.OrdinalIgnoreCase)) ?? role;
    }
}
```

ใช้ `const` เพื่อให้เอาไปใช้ใน attribute ได้ เช่น `[Authorize(Roles = Roles.Admin)]`

## ปรับ User Entity

เปิด `Models/User.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Constants;
```

เปลี่ยนค่า default ของ role

```csharp
public string Role { get; set; } = Roles.User;
```

## ปรับ AuthService

เปิด `AuthService.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Constants;
```

เปลี่ยนตอน register จาก

```csharp
Role = "User",
```

เป็น

```csharp
Role = Roles.User,
```

## ปรับ JwtTokenService

ใน `JwtTokenService` เรายังคงใส่ role claim จาก `user.Role`

```csharp
new("role", user.Role)
```

ค่าที่ใส่จะเป็น `Roles.User` หรือ `Roles.Admin` ตามข้อมูลใน database

## Seed Admin สำหรับทดสอบ

เปิด `Data/DataSeeder.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Constants;
```

ปรับ seed data ให้สร้างทั้ง admin และ user โดยใช้ helper ที่ ensure ผู้ใช้ตาม email

วิธีนี้รองรับกรณีที่คุณทำตามหนังสือต่อมาจากบทก่อนหน้าและ database มี seed เดิมอยู่แล้ว

```csharp
await EnsureUserAsync(
    "admin@example.com",
    "Admin1234!",
    Roles.Admin,
    isActive: true);

await EnsureUserAsync(
    "demo-user@example.com",
    "User1234!",
    Roles.User,
    isActive: true);

private async Task EnsureUserAsync(
    string email,
    string password,
    string role,
    bool isActive)
{
    var user = await db.Users.FirstOrDefaultAsync(user => user.Email == email);

    if (user is null)
    {
        user = new User
        {
            Email = email,
            Role = role,
            IsActive = isActive
        };

        user.PasswordHash = passwordHasher.HashPassword(user, password);
        db.Users.Add(user);

        return;
    }

    if (user.PasswordHash == "pending-auth")
    {
        user.PasswordHash = passwordHasher.HashPassword(user, password);
    }

    user.Role = role;
    user.IsActive = isActive;
}
```

บัญชีสำหรับทดสอบ admin

```text
Email: admin@example.com
Password: Admin1234!
```

บัญชีสำหรับทดสอบ user ทั่วไป

```text
Email: demo-user@example.com
Password: User1234!
```

## ข้อควรระวัง

รหัสผ่าน seed data ใช้เพื่อ local development เท่านั้น อย่าใช้รหัสผ่านตัวอย่างนี้ใน production

ถ้า database ของคุณมีข้อมูล seed เดิมอยู่แล้ว ให้ใช้รูปแบบ `EnsureUserAsync` ด้านบนแทนการ `return` เมื่อเจอข้อมูล เพราะระบบต้องเพิ่ม admin และอัปเกรด password hash ของ seed เดิมด้วย

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `Constants/Roles.cs`
- `User.Role` ใช้ `Roles.User` เป็นค่า default
- `AuthService.RegisterAsync` ใช้ `Roles.User`
- `DataSeeder` สร้าง admin และ user สำหรับทดสอบ
- login ด้วย `admin@example.com` แล้ว token มี role เป็น `Admin`
