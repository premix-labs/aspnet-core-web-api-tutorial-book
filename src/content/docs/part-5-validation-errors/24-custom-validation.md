---
title: 24 - Custom Validation
description: เพิ่ม validation rule เฉพาะระบบด้วย IValidatableObject และแยก business validation ออกจาก Controller
---

Data Annotations พื้นฐานครอบคลุม rule ทั่วไป เช่น required, email และความยาว แต่ระบบจริงมักมี rule เฉพาะ เช่นห้ามใช้ email domain บางแบบ หรือ email ต้องไม่ซ้ำใน database

บทนี้จะแยก validation เป็นสองระดับ

- Input validation ตรวจรูปแบบ request ด้วย DTO
- Business validation ตรวจ rule ของระบบ เช่น email ซ้ำ

## เพิ่ม custom rule ใน DTO

สมมติระบบของเราไม่ต้องการรับ email domain สำหรับทดสอบ เช่น `example.invalid`

เปิด `CreateUserRequest.cs` แล้วให้ class implement `IValidatableObject`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Users;

public class CreateUserRequest : IValidatableObject
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Email.EndsWith("@example.invalid", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Email domain is not allowed.",
                [nameof(Email)]);
        }
    }
}
```

ทำแบบเดียวกันใน `UpdateUserRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Users;

public class UpdateUserRequest : IValidatableObject
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Email.EndsWith("@example.invalid", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Email domain is not allowed.",
                [nameof(Email)]);
        }
    }
}
```

`IValidatableObject` เหมาะกับ rule ที่ต้องดูค่าหลาย property หรือ rule ที่ attribute สำเร็จรูปไม่มีให้

## ทดสอบ custom validation

ส่ง request นี้

```http
POST https://localhost:7001/api/users
Content-Type: application/json

{
  "email": "user@example.invalid"
}
```

ผลลัพธ์ที่คาดหวังคือ `400 Bad Request`

## Business validation ไม่ควรอยู่ใน DTO

Rule บางอย่างต้อง query database เช่น email ซ้ำหรือไม่

ไม่ควรใส่ database query ลงใน DTO เพราะ DTO ควรเป็น object สำหรับรับส่งข้อมูล ไม่ควรรู้รายละเอียด repository หรือ database

ในบทนี้เราจะตรวจให้แน่ใจว่า business validation อยู่ใน `UserService` ไม่ใช่อยู่ใน DTO และไม่กระจายอยู่ใน Controller

## ตรวจ UserService

เปิดไฟล์ `Services/UserService.cs` แล้วตรวจว่า service เป็นคนจัดการ business rule เช่น email ซ้ำ

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

ตอนนี้ `CreateUserAsync` ยังใช้ `InvalidOperationException` ชั่วคราวเมื่อ email ซ้ำ ในบทถัดไปเราจะเปลี่ยนเป็น custom exception ที่ global handler แปลงเป็น response ให้

## ตรวจการลงทะเบียน UserService

เปิด `Program.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Services;
```

ลงทะเบียน service

```csharp
builder.Services.AddScoped<IUserService, UserService>();
```

## ปรับ Controller ให้เรียก UserService

constructor ของ `UsersController` ควรเรียกผ่าน interface

```csharp
public class UsersController(IUserService userService) : ControllerBase
```

ตัวอย่าง action หลังปรับ

```csharp
[HttpGet]
public async Task<IActionResult> GetUsers()
{
    var users = await userService.GetUsersAsync();

    return Ok(users);
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
```

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- `CreateUserRequest` และ `UpdateUserRequest` มี custom validation rule
- ส่ง email domain ที่ห้ามใช้แล้วได้ `400 Bad Request`
- มี `UserService`
- Controller เริ่มบางลงและไม่ query repository เอง
- เข้าใจว่า DTO validation กับ business validation เป็นคนละระดับกัน
