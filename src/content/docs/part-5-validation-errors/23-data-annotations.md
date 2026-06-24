---
title: 23 - Validation ด้วย Data Annotations
description: ตรวจ request DTO ด้วย Required, EmailAddress, StringLength และ automatic 400 response
---

Data Annotations คือ attribute ที่ใช้กำหนด validation rule ให้ request DTO เช่น field นี้ห้ามว่าง ต้องเป็น email หรือความยาวต้องไม่เกินค่าที่กำหนด

ใน ASP.NET Core Web API ถ้า Controller มี `[ApiController]` และ model validation ไม่ผ่าน ระบบจะตอบ `400 Bad Request` ให้อัตโนมัติ เราจึงไม่ต้องเขียน `if (!ModelState.IsValid)` ในทุก action

## วิธีเรียนบทนี้

บทนี้ยังไม่สร้างระบบ error handler เอง ให้โฟกัสแค่ input validation ที่ขอบ API ก่อน:

1. ตรวจว่า DTO อยู่ถูกที่
2. เปลี่ยน request DTO จาก `record` เป็น `class`
3. ใส่ validation attribute ทีละตัว
4. build ตรวจ code
5. ยิง request ที่ผิดเพื่อดู `400 Bad Request`

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `[Required]` | field นี้ต้องมีค่า |
| `[EmailAddress]` | field นี้ต้องมีรูปแบบ email |
| `[StringLength(256)]` | field นี้ยาวได้ไม่เกิน 256 ตัวอักษร |
| `[ApiController]` | เปิด automatic model validation ให้ Controller |
| `ValidationProblemDetails` | response มาตรฐานเมื่อ validation ไม่ผ่าน |
| `System.ComponentModel.DataAnnotations` | namespace ของ validation attributes |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Dtos/Users/CreateUserRequest.cs
Dtos/Users/UpdateUserRequest.cs
Dtos/Users/UserResponse.cs
Controllers/UsersController.cs
```

## ตรวจโฟลเดอร์ DTO

ถ้าทำตามภาค Architecture มาแล้ว โปรเจกต์ควรมีไฟล์เหล่านี้อยู่แล้ว:

```text
Dtos/Users/CreateUserRequest.cs
Dtos/Users/UpdateUserRequest.cs
Dtos/Users/UserResponse.cs
```

ถ้ายังไม่มี ให้สร้างจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force -Path Dtos/Users
New-Item -ItemType File -Path Dtos/Users/CreateUserRequest.cs
New-Item -ItemType File -Path Dtos/Users/UpdateUserRequest.cs
New-Item -ItemType File -Path Dtos/Users/UserResponse.cs
```

macOS/Linux Bash:

```bash
mkdir -p Dtos/Users
touch Dtos/Users/CreateUserRequest.cs
touch Dtos/Users/UpdateUserRequest.cs
touch Dtos/Users/UserResponse.cs
```

## ทำไมเปลี่ยน record เป็น class

ในบทก่อน request DTO อาจยังเป็น `record` สั้น ๆ:

```csharp
public record CreateUserRequest(string Email);
public record UpdateUserRequest(string Email);
```

แบบนี้ใช้ได้ในช่วงเริ่มต้น แต่เมื่อเพิ่ม validation attribute หลายตัว การใช้ `class` ทำให้อ่านง่ายและเพิ่ม rule ต่อได้ตรงไปตรงมากว่า

## ขั้นที่ 1: แก้ CreateUserRequest

เปิดไฟล์:

```text
Dtos/Users/CreateUserRequest.cs
```

เพิ่ม using และ namespace:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Users;
```

จากนั้นเพิ่ม class:

```csharp
public class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}
```

อ่าน attribute เหนือ property จากบนลงล่าง:

- `[Required]` กันกรณีไม่ส่ง email มา
- `[EmailAddress]` กันรูปแบบที่ไม่ใช่ email
- `[StringLength(256)]` จำกัดความยาวให้สอดคล้องกับ database schema

## ขั้นที่ 2: แก้ UpdateUserRequest

เปิดไฟล์:

```text
Dtos/Users/UpdateUserRequest.cs
```

ใช้ rule เดียวกับตอนสร้าง user:

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

ตอนนี้ `CreateUserRequest` และ `UpdateUserRequest` ยังเหมือนกัน แต่แยกไว้เพราะอนาคต rule ตอนสร้างกับตอนแก้อาจต่างกัน เช่นตอน register ต้องมี password แต่ตอน update email ไม่ต้องมี password

## ขั้นที่ 3: ตรวจ UserResponse

เปิดไฟล์:

```text
Dtos/Users/UserResponse.cs
```

ตรวจว่ามีข้อมูลที่ต้องส่งออกครบและไม่มี `PasswordHash`

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

## ขั้นที่ 4: ตรวจ UsersController

เปิดไฟล์:

```text
Controllers/UsersController.cs
```

ตรวจว่ามี using นี้:

```csharp
using Backend.Api.Dtos.Users;
```

action ควรรับ DTO เป็น parameter เช่น:

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    // The request has already passed automatic model validation here.
    var user = await userService.CreateUserAsync(request);

    return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
}
```

ถ้า Controller ของคุณยังมี `try-catch` สำหรับ email ซ้ำจากบทก่อน ให้เก็บไว้ก่อนได้ เพราะบทนี้โฟกัส validation ก่อน action ทำงาน ส่วนบทที่ 25 จะย้าย error handling ไปไว้ที่ global handler

ไม่ต้องเพิ่ม `if (!ModelState.IsValid)` เพราะ `[ApiController]` จะตรวจ validation ให้อัตโนมัติก่อน action ทำงาน

## ตรวจ build

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet build
```

ถ้า build error ว่าไม่รู้จัก `[Required]` หรือ `[EmailAddress]` ให้ตรวจว่ามี using นี้ใน DTO:

```csharp
using System.ComponentModel.DataAnnotations;
```

## ทดสอบ validation

รัน API:

```powershell
dotnet run
```

ใช้ `baseUrl` ตาม port จริงของเครื่อง ตัวอย่าง:

```http
@baseUrl = http://localhost:5156

### Invalid email
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "not-an-email"
}
```

ผลลัพธ์ที่คาดหวังคือ `400 Bad Request` พร้อมรายละเอียด validation error

ลองส่ง body ว่าง:

```http
### Missing email
POST {{baseUrl}}/api/users
Content-Type: application/json

{}
```

ควรได้ `400 Bad Request` เช่นกัน เพราะ `Email` ถูกกำหนด `[Required]`

## ทำไมไม่ต้องเช็ก ModelState เอง

เมื่อใช้ `[ApiController]` ASP.NET Core จะตรวจ model state ก่อน action method ทำงาน ถ้า validation ไม่ผ่าน action จะไม่ถูกเรียก และ framework จะตอบ `400 Bad Request` อัตโนมัติ

ดังนั้น code แบบนี้ไม่จำเป็นใน Controller ของเรา:

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
- `UsersController` รับ request DTO จาก `Backend.Api.Dtos.Users`
- ส่ง email ผิดรูปแบบแล้วได้ `400 Bad Request`
- response ไม่ส่ง `PasswordHash` ออกไป
