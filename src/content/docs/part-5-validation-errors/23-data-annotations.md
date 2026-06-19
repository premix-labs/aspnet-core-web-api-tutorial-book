---
title: 23 - Validation ด้วย Data Annotations
description: ตรวจ request DTO ด้วย Required, EmailAddress, StringLength และ automatic 400 response
---

Data Annotations คือ attribute ที่ใช้กำหนด validation rule ให้ request DTO เช่น field นี้ห้ามว่าง ต้องเป็น email หรือความยาวต้องไม่เกินค่าที่กำหนด

ใน ASP.NET Core Web API ถ้า Controller มี `[ApiController]` และ model validation ไม่ผ่าน ระบบจะตอบ `400 Bad Request` ให้อัตโนมัติ เราจึงไม่ต้องเขียน `if (!ModelState.IsValid)` ในทุก action

## ตรวจและปรับ DTO

ถ้าทำตามภาค Architecture มาแล้ว โปรเจกต์ควรมีโฟลเดอร์และไฟล์ DTO เหล่านี้อยู่แล้ว

```text
Dtos/Users/CreateUserRequest.cs
Dtos/Users/UpdateUserRequest.cs
Dtos/Users/UserResponse.cs
```

ถ้ายังไม่มี ให้สร้างโฟลเดอร์นี้

```text
Dtos/Users/
```

ในบทนี้เราจะเปลี่ยน request DTO จาก record สั้น ๆ ให้เป็น class ที่ใส่ validation attribute ได้ชัดเจนขึ้น ส่วน `UserResponse` ให้ตรวจว่ามี field ที่ต้องส่งออกครบ

ถ้ายังมี `CreateUserRequest` หรือ `UpdateUserRequest` เป็น record แบบนี้ ให้แทนที่ด้วย code ในหัวข้อถัดไป

```csharp
public record CreateUserRequest(string Email);
public record UpdateUserRequest(string Email);
```

## CreateUserRequest

เพิ่ม code นี้ใน `Dtos/Users/CreateUserRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Users;

public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}
```

## UpdateUserRequest

เพิ่ม code นี้ใน `Dtos/Users/UpdateUserRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Users;

public class UpdateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}
```

## UserResponse

ตรวจว่า `Dtos/Users/UserResponse.cs` มีข้อมูลที่ต้องส่งออกครบ และไม่มี `PasswordHash`

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

`UserResponse` ไม่มี `PasswordHash` เพราะ response ที่ส่งออกไปหา client ไม่ควรเปิดเผยข้อมูลภายในที่เกี่ยวกับรหัสผ่าน

ถ้าคุณชอบ class มากกว่า record สามารถใช้ class ได้เช่นกัน แต่ให้ field ตรงกันและต้องไม่ส่ง `PasswordHash`

## ตรวจ UsersController

ถ้าทำตามบท 21 แล้ว `UsersController` ควรรับ request DTO จาก `Backend.Api.Dtos.Users` อยู่แล้ว

```csharp
using Backend.Api.Dtos.Users;
```

ตัวอย่าง action ที่ถูกต้องคือ Controller รับ `CreateUserRequest` แล้วส่งต่อให้ service

```csharp
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

ไม่ต้องเพิ่ม `if (!ModelState.IsValid)` เพราะ `[ApiController]` จะตรวจให้อัตโนมัติ

## ทดสอบ validation

รัน API

```powershell
dotnet run
```

ส่ง request ที่ email ผิดรูปแบบ

```http
POST https://localhost:7001/api/users
Content-Type: application/json

{
  "email": "not-an-email"
}
```

ผลลัพธ์ที่คาดหวังคือ `400 Bad Request` พร้อมรายละเอียด validation error

ลองส่ง body ว่าง

```http
POST https://localhost:7001/api/users
Content-Type: application/json

{}
```

ควรได้ `400 Bad Request` เช่นกัน เพราะ `Email` ถูกกำหนด `[Required]`

## ทำไมไม่ต้องเช็ก ModelState เอง

เมื่อใช้ `[ApiController]` ASP.NET Core จะตรวจ model state ก่อน action method ทำงาน ถ้า validation ไม่ผ่าน action จะไม่ถูกเรียก และ framework จะตอบ `400 Bad Request` อัตโนมัติ

ดังนั้น code แบบนี้ไม่จำเป็นใน Controller ของเรา

```csharp
if (!ModelState.IsValid)
{
    return BadRequest(ModelState);
}
```

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `CreateUserRequest`, `UpdateUserRequest`, `UserResponse`
- request DTO ใช้ `[Required]`, `[EmailAddress]`, `[StringLength]`
- `UsersController` ไม่ใช้ anonymous response กระจัดกระจาย
- ส่ง email ผิดรูปแบบแล้วได้ `400 Bad Request`
- response ไม่ส่ง `PasswordHash` ออกไป
