---
title: 12 - Dependency Injection
description: เข้าใจการ inject service เข้า Controller ด้วย built-in DI container
---

Dependency Injection หรือ DI คือวิธีส่ง object ที่ class ต้องใช้เข้ามาจากภายนอก แทนการสร้าง object เองใน class นั้น

ASP.NET Core มี DI container ในตัว ทำให้เราลงทะเบียน service ใน `Program.cs` แล้ว inject เข้า Controller, Service หรือ class อื่นได้

## ปัญหาของการ new เอง

ตัวอย่างที่ไม่ควรทำใน Controller:

```csharp
public class UsersController : ControllerBase
{
    private readonly UserService _userService = new();
}
```

ปัญหาคือ Controller ผูกกับ `UserService` โดยตรง เปลี่ยน implementation ยาก test ยาก และเมื่อ `UserService` ต้องใช้ repository ก็ต้อง new ต่อกันเป็นทอด ๆ

DI แก้ปัญหานี้โดยให้ ASP.NET Core เป็นคนสร้าง object และส่ง dependency เข้ามาให้

## สร้าง Model

สร้างไฟล์

```text
Models/User.cs
```

```csharp
namespace Backend.Api.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
}
```

## สร้าง Repository interface

สร้างไฟล์

```text
Repositories/IUserRepository.cs
```

```csharp
using Backend.Api.Models;

namespace Backend.Api.Repositories;

public interface IUserRepository
{
    IReadOnlyList<User> GetAll();

    User? GetById(int id);

    User Add(string email);

    User? Update(int id, string email);

    bool Delete(int id);
}
```

Interface บอกว่า repository ต้องทำอะไรได้ โดยยังไม่บอกว่าข้างในเก็บข้อมูลแบบไหน

## สร้าง Repository implementation

สร้างไฟล์

```text
Repositories/InMemoryUserRepository.cs
```

```csharp
using Backend.Api.Models;

namespace Backend.Api.Repositories;

public class InMemoryUserRepository : IUserRepository
{
    private static readonly List<User> Users =
    [
        new() { Id = 1, Email = "admin@example.com" },
        new() { Id = 2, Email = "user@example.com" }
    ];

    public IReadOnlyList<User> GetAll()
    {
        return Users;
    }

    public User? GetById(int id)
    {
        return Users.FirstOrDefault(user => user.Id == id);
    }

    public User Add(string email)
    {
        var nextId = Users.Count == 0 ? 1 : Users.Max(user => user.Id) + 1;

        var user = new User
        {
            Id = nextId,
            Email = email
        };

        Users.Add(user);

        return user;
    }

    public User? Update(int id, string email)
    {
        var user = GetById(id);

        if (user is null)
        {
            return null;
        }

        user.Email = email;

        return user;
    }

    public bool Delete(int id)
    {
        var user = GetById(id);

        if (user is null)
        {
            return false;
        }

        Users.Remove(user);

        return true;
    }
}
```

## สร้าง Service interface

สร้างไฟล์

```text
Services/IUserService.cs
```

```csharp
using Backend.Api.Models;

namespace Backend.Api.Services;

public interface IUserService
{
    IReadOnlyList<User> GetUsers();

    User? GetUserById(int id);

    User CreateUser(string email);

    User? UpdateUser(int id, string email);

    bool DeleteUser(int id);
}
```

## สร้าง Service implementation

สร้างไฟล์

```text
Services/UserService.cs
```

```csharp
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public IReadOnlyList<User> GetUsers()
    {
        return userRepository.GetAll();
    }

    public User? GetUserById(int id)
    {
        return userRepository.GetById(id);
    }

    public User CreateUser(string email)
    {
        return userRepository.Add(email);
    }

    public User? UpdateUser(int id, string email)
    {
        return userRepository.Update(id, email);
    }

    public bool DeleteUser(int id)
    {
        return userRepository.Delete(id);
    }
}
```

ตอนนี้ service ยังดูเหมือน pass-through ไป repository แต่ในบทหลัง ๆ service จะเริ่มมี validation, password hashing, JWT, self-protection และ audit log

## ลงทะเบียน Service ใน Program.cs

เปิด `Program.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Repositories;
using Backend.Api.Services;
```

จากนั้นลงทะเบียน service ก่อน `var app = builder.Build();`

```csharp
builder.Services.AddScoped<IUserRepository, InMemoryUserRepository>();
builder.Services.AddScoped<IUserService, UserService>();
```

ความหมายคือ ถ้ามี class ไหนขอ `IUserRepository` ให้สร้าง `InMemoryUserRepository` ให้

ถ้ามี class ไหนขอ `IUserService` ให้สร้าง `UserService` ให้

## Inject เข้า Controller

แก้ `UsersController` ให้รับ `IUserService`

ก่อนแก้ ให้ลบ static list เดิมใน Controller ออก เพราะตอนนี้ข้อมูลจะถูกจัดการผ่าน `InMemoryUserRepository` แทน Controller แล้ว

ในบทนี้เรายังเก็บ request record ไว้ในไฟล์ Controller ก่อน เพื่อไม่เพิ่มหลายแนวคิดพร้อมกันเกินไป บทถัดไปจะแยก DTO ไปไว้ในโฟลเดอร์ของตัวเอง

```csharp
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Services;

namespace Backend.Api.Controllers;

public record CreateUserRequest(string Email);

public record UpdateUserRequest(string Email);

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(userService.GetUsers());
    }

    [HttpGet("{id:int}")]
    public IActionResult GetUserById(int id)
    {
        var user = userService.GetUserById(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        var user = userService.CreateUser(request.Email);

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, UpdateUserRequest request)
    {
        var user = userService.UpdateUser(id, request.Email);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        var deleted = userService.DeleteUser(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
```

ตัวอย่างนี้ใช้ primary constructor ของ C# เพื่อรับ dependency

ASP.NET Core จะสร้าง `UserService` และส่งเข้ามาให้ Controller อัตโนมัติ

หลังแก้แล้ว flow ของ `POST /api/users` จะเปลี่ยนจาก Controller เพิ่มข้อมูลเข้า list เอง เป็น:

```text
UsersController
  -> IUserService
  -> IUserRepository
  -> InMemoryUserRepository
```

ผลลัพธ์ของ endpoint ยังเหมือนเดิม แต่ code แต่ละชั้นมีหน้าที่ชัดขึ้น

## Service Lifetime ที่เจอบ่อย

```text
AddTransient  สร้าง instance ใหม่ทุกครั้งที่ถูกขอ
AddScoped     สร้าง instance หนึ่งครั้งต่อ HTTP request
AddSingleton  สร้าง instance เดียวทั้ง application
```

สำหรับ Web API ทั่วไป โดยเฉพาะ service ที่เกี่ยวกับ database มักใช้ `AddScoped`

เหตุผลคือ EF Core `DbContext` ปกติใช้แบบ scoped ต่อ request ดังนั้น service และ repository ที่ใช้ database ก็ควรเป็น scoped ด้วย

## เลือก lifetime แบบไหนดี

ใช้ `AddScoped` เป็นค่าเริ่มต้นสำหรับ application service และ repository

ใช้ `AddSingleton` กับ object ที่ไม่มี state ต่อ request เช่น configuration reader บางชนิด หรือ service ที่ thread-safe จริง

ใช้ `AddTransient` กับ object เบา ๆ ที่ไม่มี state และไม่ต้องแชร์ใน request เดียวกัน

ถ้าไม่แน่ใจใน Web API ทั่วไป ให้เริ่มจาก `AddScoped`

## Error ที่เจอบ่อย

ถ้าลืมลงทะเบียน service คุณจะเจอ error ประมาณนี้ตอนเรียก endpoint

```text
Unable to resolve service for type 'Backend.Api.Services.IUserService'
```

แปลว่า Controller ขอ `IUserService` แต่ DI container ไม่รู้ว่าจะสร้างอะไรให้

ให้กลับไปเช็ก `Program.cs` ว่าลงทะเบียนครบหรือไม่

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรตอบได้ว่า

- DI ช่วยแก้ปัญหาอะไร
- ทำไม Controller ไม่ควร `new UserService()` เอง
- interface ช่วยให้เปลี่ยน implementation ได้อย่างไร
- `AddScoped` ต่างจาก `AddSingleton` อย่างไร
- ถ้าเจอ error `Unable to resolve service` ควรตรวจที่ไหน
