---
title: 14 - Mapping ระหว่าง Entity กับ DTO
description: แปลงข้อมูลระหว่าง object ภายในระบบกับ object ที่ใช้ใน API
---

Mapping คือการแปลงข้อมูลจาก object รูปแบบหนึ่งไปเป็นอีกรูปแบบหนึ่ง เช่นแปลง `User` model เป็น `UserResponse`

หลังจากเราแยก DTO แล้ว ต้องมีจุดที่แปลงข้อมูลภายในระบบให้เป็นข้อมูลที่ API ส่งออกได้อย่างปลอดภัย

## ตัวอย่าง Model

ตอนนี้ model ของเรายังเรียบง่าย

```csharp
namespace Backend.Api.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;
}
```

ในระบบจริง `User` จะมี field มากกว่านี้ เช่น `PasswordHash`, `Role`, `IsActive` และ `CreatedAt`

## ตัวอย่าง DTO

```csharp
namespace Backend.Api.Dtos.Users;

public record UserResponse(int Id, string Email);
```

`UserResponse` คือสิ่งที่ API ส่งกลับไปหา client ไม่จำเป็นต้องเหมือน model ทุก field

## Mapping แบบ manual

วิธีที่เข้าใจง่ายที่สุดคือเขียน mapping เอง

```csharp
private static UserResponse ToResponse(User user)
{
    return new UserResponse(
        user.Id,
        user.Email);
}
```

ข้อดีของ manual mapping:

- อ่านง่าย
- เห็นชัดว่าส่ง field ไหนออก API
- debug ง่าย
- ไม่มี magic ซ่อนอยู่
- เหมาะกับมือใหม่และโปรเจกต์ที่ยังไม่ซับซ้อนมาก

## เพิ่ม mapping ใน UserService

แก้ `IUserService` ให้ return DTO แทน model

```csharp
using Backend.Api.Dtos.Users;

namespace Backend.Api.Services;

public interface IUserService
{
    IReadOnlyList<UserResponse> GetUsers();

    UserResponse? GetUserById(int id);

    UserResponse CreateUser(CreateUserRequest request);

    UserResponse? UpdateUser(int id, UpdateUserRequest request);

    bool DeleteUser(int id);
}
```

จากนั้นแก้ `UserService`

```csharp
using Backend.Api.Dtos.Users;
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class UserService(IUserRepository userRepository) : IUserService
{
    public IReadOnlyList<UserResponse> GetUsers()
    {
        return userRepository.GetAll()
            .Select(ToResponse)
            .ToList();
    }

    public UserResponse? GetUserById(int id)
    {
        var user = userRepository.GetById(id);

        return user is null ? null : ToResponse(user);
    }

    public UserResponse CreateUser(CreateUserRequest request)
    {
        var user = userRepository.Add(request.Email);

        return ToResponse(user);
    }

    public UserResponse? UpdateUser(int id, UpdateUserRequest request)
    {
        var user = userRepository.Update(id, request.Email);

        return user is null ? null : ToResponse(user);
    }

    public bool DeleteUser(int id)
    {
        return userRepository.Delete(id);
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.Email);
    }
}
```

ตอนนี้ Controller ไม่ต้องรู้จัก `User` model โดยตรงแล้ว Controller คุยกับ DTO ผ่าน service

## แก้ Controller ให้ส่ง DTO เข้า Service

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
        var user = userService.CreateUser(request);

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, UpdateUserRequest request)
    {
        var user = userService.UpdateUser(id, request);

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

## Mapping ควรอยู่ที่ไหน

ในโปรเจกต์เล็ก mapping อยู่ใน service ได้

ในโปรเจกต์ที่ใหญ่ขึ้น อาจแยกเป็น extension method หรือ mapper class เช่น

```text
Mappings/UserMappings.cs
```

แต่สำหรับหนังสือนี้จะเริ่มจาก manual mapping ใน service เพื่อให้ flow ชัด

## ควรใช้ AutoMapper ไหม

library mapping เช่น AutoMapper มีประโยชน์ในบางโปรเจกต์ แต่สำหรับมือใหม่ควรเริ่มจาก manual mapping ก่อน

เหตุผลคือ manual mapping ทำให้เห็นชัดว่า field ไหนถูกส่งออก และช่วยให้เข้าใจ boundary ระหว่าง model กับ DTO

เมื่อเข้าใจพื้นฐานแล้ว คุณค่อยตัดสินใจได้ว่าทีมของคุณควรใช้ library หรือเขียน mapping เอง

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรตอบได้ว่า

- Mapping ช่วยป้องกันข้อมูลรั่วได้อย่างไร
- ทำไม `PasswordHash` ไม่ควรอยู่ใน response DTO
- Manual mapping มีข้อดีอะไรสำหรับมือใหม่
- Controller ควรรู้จัก entity ภายในระบบโดยตรงหรือไม่
- Mapping ควรอยู่ที่ไหนในโปรเจกต์ขนาดเล็ก
