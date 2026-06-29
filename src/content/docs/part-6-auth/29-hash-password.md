---
title: 29 - Hash Password
description: ใช้ PasswordHasher<TUser> เพื่อ hash และ verify password โดยไม่เก็บ plain text
---

ระบบจริงห้ามเก็บ password ดิบลง database ต้องเก็บเฉพาะ password hash เท่านั้น

ในบทนี้เราจะใช้ `PasswordHasher<TUser>` ที่มากับ ASP.NET Core Identity core services โดยไม่ต้องใช้ระบบ ASP.NET Core Identity เต็มชุด

## วิธีเรียนบทนี้

ให้ทำทีละชั้น:

1. ลงทะเบียน `PasswordHasher`
2. สร้าง `AuthService`
3. เพิ่ม `RegisterAsync`
4. สร้าง `AuthController`
5. ปรับ `DataSeeder` ให้เลิกใช้ `pending-auth`
6. ทดสอบ register

บทนี้ยังไม่ทำ login และยังไม่สร้าง JWT จริง

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `IPasswordHasher<User>` | interface สำหรับ hash และ verify password |
| `PasswordHasher<User>` | implementation จาก Microsoft.AspNetCore.Identity |
| `HashPassword(...)` | สร้าง password hash จาก password ดิบ |
| salt | random data ที่ทำให้ hash ของ password เดียวกันไม่จำเป็นต้องเหมือนกัน |
| `AuthService` | service สำหรับ use case ด้าน authentication |
| `AuthController` | controller ของ endpoint `/api/auth/...` |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Services/AuthService.cs
Controllers/AuthController.cs
Data/DataSeeder.cs
Program.cs
```

## หลักการสำคัญ

- ไม่เก็บ plain text password
- ไม่ส่ง password hash กลับไปใน response
- ไม่เปรียบเทียบ password ด้วย string ธรรมดา
- ใช้ password hasher ที่ออกแบบมาสำหรับ password โดยเฉพาะ
- hash เดิมอาจไม่เหมือนกันแม้ password เดียวกัน เพราะมี salt

## ขั้นที่ 1: ลงทะเบียน PasswordHasher

เปิด `Program.cs` แล้วเพิ่ม using:

```csharp
using Microsoft.AspNetCore.Identity;
using Backend.Api.Models;
```

ลงทะเบียน service ก่อน `builder.Build()`:

```csharp
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```

ความหมายคือ ถ้า class ไหนขอ `IPasswordHasher<User>` ให้ DI container สร้าง `PasswordHasher<User>` ให้

## ทำไม hash password เดิมแล้วได้ค่าไม่เหมือนเดิม

Password hasher ที่ดีจะใช้ salt ทำให้ password เดียวกันถูก hash แล้วได้ค่าต่างกันได้ นี่เป็นเรื่องปกติและเป็นสิ่งที่ต้องการ

ดังนั้นห้ามตรวจ password ด้วยวิธีนี้:

```csharp
var hash = passwordHasher.HashPassword(user, inputPassword);

if (hash == user.PasswordHash)
{
    // Wrong: do not compare password hashes this way.
}
```

ตอนตรวจ password ให้ใช้ `VerifyHashedPassword` ซึ่งจะสอนในบท Login

## ขั้นที่ 2: สร้าง AuthService

รันจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType File -Path Services/AuthService.cs
```

macOS/Linux Bash:

```bash
touch Services/AuthService.cs
```

เปิดไฟล์:

```text
Services/AuthService.cs
```

เริ่มจาก using และ class:

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
}
```

`AuthService` ต้องใช้ repository เพื่อสร้าง user และใช้ password hasher เพื่อ hash password ก่อนบันทึก

## ขั้นที่ 3: เพิ่ม RegisterAsync

เพิ่ม method นี้ใน class `AuthService`

```csharp
public async Task<UserResponse> RegisterAsync(RegisterRequest request)
{
    var existingUser = await userRepository.GetByEmailAsync(request.Email);

    if (existingUser is not null)
    {
        throw new ConflictException("Email already exists", "EMAIL_ALREADY_EXISTS");
    }
```

ต่อด้วยการสร้าง user:

```csharp
    var user = new User
    {
        Email = request.Email,
        Role = "User",
        IsActive = true
    };
```

จากนั้น hash password ก่อน save:

```csharp
    user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

    var createdUser = await userRepository.CreateAsync(user);

    return ToResponse(createdUser);
}
```

จุดสำคัญคือสร้าง `User` object ก่อน แล้วค่อยเรียก `HashPassword(user, request.Password)` เพื่อให้ hasher ได้รับ user object ที่เกี่ยวข้อง

## ขั้นที่ 4: เพิ่ม mapping method

เพิ่ม method นี้ไว้ท้าย class `AuthService`

```csharp
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
```

method นี้ไม่ส่ง `PasswordHash` ออกไปใน response

## ขั้นที่ 5: ลงทะเบียน AuthService

เปิด `Program.cs` แล้วเพิ่มก่อน `builder.Build()`:

```csharp
builder.Services.AddScoped<AuthService>();
```

## ขั้นที่ 6: สร้าง AuthController

รันจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType File -Path Controllers/AuthController.cs
```

macOS/Linux Bash:

```bash
touch Controllers/AuthController.cs
```

เปิดไฟล์:

```text
Controllers/AuthController.cs
```

เพิ่ม code นี้:

```csharp
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Dtos.Auth;
using Backend.Api.Services;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/auth")]
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

เราใช้ `[Route("api/auth")]` ตรง ๆ เพื่อให้ contract ของ auth ชัดเจนและไม่ผูกกับชื่อ class ของ controller

ถ้าใช้ `[Route("api/[controller]")]` กับ `AuthController` ASP.NET Core จะตัดคำว่า `Controller` ออกและได้ route เป็น `/api/auth` เช่นกัน แต่ในบท auth เราเลือกเขียน route คงที่เพื่อให้อ่าน endpoint ได้ทันที

## ขั้นที่ 7: ปรับ DataSeeder

ในบทฐานข้อมูลเราเคยใช้ `PasswordHash = "pending-auth"` สำหรับข้อมูลทดสอบ หลังมี password hasher แล้ว ไม่ควร seed user ด้วยค่า placeholder อีก

เปิดไฟล์:

```text
Data/DataSeeder.cs
```

เพิ่ม using:

```csharp
using Microsoft.AspNetCore.Identity;
```

ปรับ constructor ให้รับ password hasher:

```csharp
public class DataSeeder(AppDbContext db, IPasswordHasher<User> passwordHasher)
{
}
```

แทนที่จะ `return` ทันทีเมื่อมี user อยู่แล้ว ให้สร้าง helper ชื่อ `EnsureUserAsync` เพื่อสร้างหรืออัปเกรด user ทีละคน

## ขั้นที่ 8: เพิ่ม EnsureUserAsync

เพิ่ม method นี้ใน `DataSeeder`

```csharp
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
```

method นี้รองรับทั้งกรณี database ว่าง และกรณีมี seed เดิมที่ยังเป็น `pending-auth`

## ขั้นที่ 9: เรียก EnsureUserAsync ใน SeedAsync

ปรับ `SeedAsync` ให้เรียก helper แล้วค่อย save:

```csharp
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
```

บัญชีเหล่านี้ใช้สำหรับทดสอบ login ได้:

```text
Email: demo-user@example.com
Password: User1234!

Email: inactive-user@example.com
Password: User1234!
```

## ตรวจ build

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet build
```

ถ้า build error ที่ `DataSeeder` ให้ตรวจว่ายังมี using เหล่านี้:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Models;
```

## ทดสอบ register

รัน API:

```powershell
dotnet run
```

ใช้ `baseUrl` ตาม port จริง:

```http
@baseUrl = http://localhost:5156
@authPath = /api/auth

### Register
POST {{baseUrl}}{{authPath}}/register
Content-Type: application/json

{
  "email": "new-user@example.com",
  "password": "Passw0rd!"
}
```

ผลลัพธ์ที่คาดหวังคือ `201 Created` และ response ไม่มี password หรือ password hash

ถ้าส่ง email เดิมซ้ำ ควรได้ `409 Conflict` พร้อม `code` เป็น `EMAIL_ALREADY_EXISTS`

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- ลงทะเบียน `IPasswordHasher<User>`
- มี `AuthService.RegisterAsync`
- `RegisterAsync` hash password ก่อนบันทึก database
- มี `AuthController` พร้อม `POST /api/auth/register`
- `DataSeeder` ไม่สร้าง seed ใหม่ด้วย `PasswordHash = "pending-auth"` แล้ว
- `DataSeeder` อัปเกรด seed เดิมที่ยังเป็น `pending-auth` ให้ login ได้
