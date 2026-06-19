---
title: 29 - Hash Password
description: ใช้ PasswordHasher<TUser> เพื่อ hash และ verify password โดยไม่เก็บ plain text
---

ระบบจริงห้ามเก็บ password ดิบลง database ต้องเก็บเฉพาะ password hash เท่านั้น

ในบทนี้เราจะใช้ `PasswordHasher<TUser>` ที่มากับ ASP.NET Core Identity core services โดยไม่ต้องใช้ระบบ ASP.NET Core Identity เต็มชุด

## หลักการสำคัญ

- ไม่เก็บ plain text password
- ไม่ส่ง password hash กลับไปใน response
- ไม่เปรียบเทียบ password ด้วย string ธรรมดา
- ใช้ password hasher ที่ออกแบบมาสำหรับ password โดยเฉพาะ
- hash เดิมอาจไม่เหมือนกันแม้ password เดียวกัน เพราะมี salt

## ลงทะเบียน PasswordHasher

เปิด `Program.cs` แล้วเพิ่ม using

```csharp
using Microsoft.AspNetCore.Identity;
using Backend.Api.Models;
```

จากนั้นลงทะเบียน service

```csharp
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```

## ทำไม hash password เดิมแล้วได้ค่าไม่เหมือนเดิม

Password hasher ที่ดีจะใช้ salt ทำให้ password เดียวกันถูก hash แล้วได้ค่าต่างกันได้ นี่เป็นเรื่องปกติและเป็นสิ่งที่ต้องการ

ดังนั้นห้ามตรวจ password ด้วยวิธีนี้

```csharp
var hash = passwordHasher.HashPassword(user, inputPassword);

if (hash == user.PasswordHash)
{
    // วิธีนี้ผิด
}
```

ให้ใช้ `VerifyHashedPassword` แทน

## สร้าง AuthService

สร้างไฟล์

```text
Services/AuthService.cs
```

เพิ่ม code เริ่มต้นนี้

```csharp
using Microsoft.AspNetCore.Identity;
using Backend.Api.Dtos.Auth;
using Backend.Api.Dtos.Users;
using Backend.Api.Exceptions;
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher<User> passwordHasher)
{
    public async Task<UserResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new ConflictException("Email already exists", "EMAIL_ALREADY_EXISTS");
        }

        var user = new User
        {
            Email = request.Email,
            Role = "User",
            IsActive = true
        };

        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        var createdUser = await userRepository.CreateAsync(user);

        return ToResponse(createdUser);
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }
}
```

จุดสำคัญคือสร้าง `User` ก่อน แล้วค่อยเรียก `HashPassword(user, request.Password)` เพื่อให้ hasher ได้รับ user object ที่เกี่ยวข้อง

## ลงทะเบียน AuthService

เปิด `Program.cs` แล้วเพิ่ม

```csharp
builder.Services.AddScoped<AuthService>();
```

## สร้าง AuthController

สร้างไฟล์

```text
Controllers/AuthController.cs
```

เพิ่ม code นี้

```csharp
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Dtos.Auth;
using Backend.Api.Services;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var user = await authService.RegisterAsync(request);

        return StatusCode(StatusCodes.Status201Created, user);
    }
}
```

## ปรับ DataSeeder

ในบทฐานข้อมูลเราเคยใช้ `PasswordHash = "pending-auth"` สำหรับข้อมูลทดสอบ หลังมี password hasher แล้ว ไม่ควร seed user ด้วยค่า placeholder อีก

ให้แก้ `DataSeeder` ให้รับ `IPasswordHasher<User>` และรองรับกรณีที่มีข้อมูล seed เดิมจากบท 22 อยู่แล้ว

ถ้าคุณทำตามหนังสือมาต่อเนื่อง ฐานข้อมูลอาจมี `demo-user@example.com` และ `inactive-user@example.com` ที่ยังมี `PasswordHash = "pending-auth"` อยู่ ดังนั้น seeder ต้องอัปเกรด hash เดิมด้วย ไม่ใช่เจอ user แล้ว `return` ทันที

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Models;

namespace Backend.Api.Data;

public class DataSeeder(AppDbContext db, IPasswordHasher<User> passwordHasher)
{
    public async Task SeedAsync()
    {
        await EnsureUserAsync(
            "demo-user@example.com",
            "User",
            isActive: true);

        await EnsureUserAsync(
            "inactive-user@example.com",
            "User",
            isActive: false);

        await db.SaveChangesAsync();
    }

    private async Task EnsureUserAsync(string email, string role, bool isActive)
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

            user.PasswordHash = passwordHasher.HashPassword(user, "User1234!");
            db.Users.Add(user);

            return;
        }

        if (user.PasswordHash == "pending-auth")
        {
            user.PasswordHash = passwordHasher.HashPassword(user, "User1234!");
        }

        user.Role = role;
        user.IsActive = isActive;
    }
}
```

บัญชีเหล่านี้ใช้สำหรับทดสอบ login ได้

```text
Email: demo-user@example.com
Password: User1234!

Email: inactive-user@example.com
Password: User1234!
```

## ทดสอบ register

รัน API

```powershell
dotnet run
```

ส่ง request

```http
POST https://localhost:7001/api/auth/register
Content-Type: application/json

{
  "email": "new-user@example.com",
  "password": "Passw0rd!"
}
```

ผลลัพธ์ที่คาดหวังคือ `201 Created` และ response ไม่มี password หรือ password hash

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- ลงทะเบียน `IPasswordHasher<User>`
- มี `AuthService.RegisterAsync`
- `RegisterAsync` hash password ก่อนบันทึก database
- มี `AuthController` พร้อม `POST /api/auth/register`
- `DataSeeder` ไม่สร้าง seed ใหม่ด้วย `PasswordHash = "pending-auth"` แล้ว
- `DataSeeder` อัปเกรด seed เดิมที่ยังเป็น `pending-auth` ให้ login ได้
