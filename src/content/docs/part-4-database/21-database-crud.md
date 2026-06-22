---
title: 21 - ทำ CRUD กับฐานข้อมูลจริง
description: เปลี่ยน CRUD API จาก in-memory list ไปใช้ EF Core และ repository
---

ก่อนหน้านี้เราใช้ list ใน memory เพื่อเรียน Controller และ REST API แต่ข้อมูลแบบนั้นจะหายทุกครั้งที่ restart application

บทนี้จะเปลี่ยนไปใช้ SQL Server ผ่าน EF Core โดยแยกการเข้าถึงข้อมูลไว้ใน repository

## ก่อนเริ่มบทนี้

ให้ตรวจว่าคุณทำบทก่อนหน้าครบแล้ว:

- มี `Models/User.cs`
- มี `Data/AppDbContext.cs`
- `Program.cs` ลงทะเบียน `AddDbContext`
- สร้าง migration และรัน `dotnet tool run dotnet-ef database update` แล้ว
- โครงสร้างจากภาค Architecture มี `Controllers`, `Services`, `Repositories` และ DTO พื้นฐานแล้ว

ถ้าจะรันคำสั่ง ให้เปิด terminal ที่ root ของโปรเจกต์ `Backend.Api` เว้นแต่บทจะบอกว่าให้เปิดจากโฟลเดอร์อื่น

## คำศัพท์ในบทนี้

`Repository` คือ class ที่รับผิดชอบการอ่าน/เขียนข้อมูลกับ database แทนที่จะให้ Controller หรือ Service เขียน EF Core query เองทุกที่

`async/await` ใช้เพราะการคุย database เป็นงาน I/O ที่อาจใช้เวลา การเขียนแบบ async ช่วยให้ server ไม่ต้อง block thread ระหว่างรอ database ตอบกลับ

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Repositories/IUserRepository.cs
Repositories/UserRepository.cs
Dtos/Users/UserResponse.cs
Services/IUserService.cs
Services/UserService.cs
Controllers/UsersController.cs
Program.cs
```

เป้าหมายของบทนี้คือ flow ยังเป็น `Controller -> Service -> Repository` เหมือนเดิม แต่ repository เปลี่ยนจาก in-memory list ไปใช้ EF Core และ SQL Server

## ปรับ Repository Interface

จากภาค Architecture เรามีโฟลเดอร์ `Repositories` และ interface สำหรับ in-memory repository อยู่แล้ว บทนี้จะเปลี่ยน interface เดิมให้เป็นแบบ async เพื่อใช้กับ EF Core

เปิดไฟล์

```text
Repositories/IUserRepository.cs
```

แทนที่ code เดิมด้วย code นี้

```csharp
using Backend.Api.Models;

namespace Backend.Api.Repositories;

public interface IUserRepository
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(User user);
}
```

## สร้าง UserRepository

สร้างไฟล์

```text
Repositories/UserRepository.cs
```

เพิ่ม code นี้

```csharp
using Microsoft.EntityFrameworkCore;
using Backend.Api.Data;
using Backend.Api.Models;

namespace Backend.Api.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<List<User>> GetAllAsync()
    {
        return db.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .ToListAsync();
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return db.Users.FirstOrDefaultAsync(user => user.Id == id);
    }

    public Task<User?> GetByEmailAsync(string email)
    {
        return db.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public async Task<User> CreateAsync(User user)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        user.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(User user)
    {
        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return true;
    }
}
```

## ทำไม GetAllAsync ใช้ AsNoTracking

`AsNoTracking()` บอก EF Core ว่าเราอ่านข้อมูลอย่างเดียว ไม่ได้จะแก้ไข entity ชุดนี้

สำหรับ query ที่ใช้แสดงผล `AsNoTracking()` ช่วยลดงาน tracking ภายใน EF Core และทำให้ intent ชัดเจนขึ้น

## ลงทะเบียน Repository

เปิด `Program.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Repositories;
```

ลงทะเบียน repository ใน DI container

```csharp
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

ถ้ายังมี registration เดิมแบบนี้อยู่ ให้เปลี่ยนเป็น `UserRepository` ไม่ใช่ลงทะเบียนซ้ำ

```csharp
builder.Services.AddScoped<IUserRepository, InMemoryUserRepository>();
```

ควรวางไว้ใกล้กับ service registration อื่น ๆ เช่นหลัง `AddDbContext`

## ปรับ UserResponse ให้ตรงกับ Entity ใหม่

บทก่อนหน้านี้ `UserResponse` มีแค่ `Id` และ `Email` แต่ตอนนี้ `User` entity มีข้อมูลเพิ่ม เช่น `Role`, `IsActive`, `CreatedAtUtc` และ `UpdatedAtUtc`

เปิดไฟล์

```text
Dtos/Users/UserResponse.cs
```

แทนที่ด้วย code นี้

```csharp
namespace Backend.Api.Dtos.Users;

public record UserResponse(
    int Id,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
```

เหตุผลที่ต้องปรับก่อนแก้ service คือ `UserService` ด้านล่างจะ map จาก `User` entity ไปเป็น `UserResponse` รูปแบบใหม่นี้ ถ้าไม่ปรับ code จะ compile ไม่ผ่าน

## ปรับ UserService ให้ใช้ Repository

ในภาค Architecture เราแยก flow เป็น Controller -> Service -> Repository แล้ว ดังนั้นบทนี้ไม่ควรให้ Controller ข้ามไปเรียก repository โดยตรง

ให้เปิด `Services/UserService.cs` แล้วปรับ service ให้เรียก `IUserRepository` แบบ async

```csharp
using Backend.Api.Dtos.Users;
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetUsersAsync()
    {
        var users = await userRepository.GetAllAsync();

        return users
            .Select(ToResponse)
            .ToList();
    }

    public async Task<UserResponse?> GetUserByIdAsync(int id)
    {
        var user = await userRepository.GetByIdAsync(id);

        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = "pending-auth",
            Role = "User",
            IsActive = true
        };

        var createdUser = await userRepository.CreateAsync(user);

        return ToResponse(createdUser);
    }

    public async Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return null;
        }

        user.Email = request.Email;

        await userRepository.UpdateAsync(user);

        return ToResponse(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await userRepository.GetByIdAsync(id);

        if (user is null)
        {
            return false;
        }

        return await userRepository.DeleteAsync(user);
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

ถ้า `IUserService` ยังเป็น synchronous method จากภาคก่อน ให้ปรับเป็น async ให้ตรงกับ service ด้านบน

```csharp
using Backend.Api.Dtos.Users;

namespace Backend.Api.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetUsersAsync();
    Task<UserResponse?> GetUserByIdAsync(int id);
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateUserAsync(int id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(int id);
}
```

## ปรับ Controller ให้เรียก UserService ต่อ

Controller ควรบางและไม่รู้รายละเอียด EF Core โดยตรง

```csharp
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Dtos.Users;
using Backend.Api.Services;

namespace Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        return Ok(await userService.GetUsersAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await userService.GetUserByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(CreateUserRequest request)
    {
        try
        {
            var user = await userService.CreateUserAsync(request);

            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new { message = exception.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, UpdateUserRequest request)
    {
        var user = await userService.UpdateUserAsync(id, request);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await userService.DeleteUserAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
```

## ทำไม PasswordHash ยังเป็น pending-auth

ตอนนี้เรายังอยู่ในภาค database และยังไม่ได้สอน hash password จึงใช้ค่า placeholder เพื่อให้ record ผ่าน schema ก่อน

ค่า `pending-auth` ใช้เพื่อการเรียนในบทนี้เท่านั้น ในภาค Authentication เราจะเปลี่ยน flow การสร้าง user ให้ hash password จริงก่อนบันทึกลง database

## ทดสอบ API

รัน API

```powershell
dotnet run
```

ทดสอบสร้าง user

```http
POST https://localhost:7001/api/users
Content-Type: application/json

{
  "email": "database-user@example.com"
}
```

จากนั้น restart application แล้วเรียก `GET /api/users` อีกครั้ง ข้อมูลควรยังอยู่ เพราะตอนนี้ถูกเก็บใน database แล้ว

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `IUserRepository`
- มี `UserRepository`
- `Program.cs` ลงทะเบียน repository แล้ว
- `UserService` ใช้ async/await กับ repository
- `UsersController` ยังเรียกผ่าน `IUserService`
- ข้อมูลไม่หายหลัง restart application
