---
title: 28 - ออกแบบ Register และ Login
description: วาง contract ของ Auth API และสร้าง DTO สำหรับ register, login และ current user
---

ก่อนเขียน code ระบบ authentication ต้องกำหนด contract ให้ชัดว่า client จะเรียก endpoint อะไร ส่งข้อมูลอะไร และ API จะตอบอะไรกลับไป

ในหนังสือนี้เราจะแยก endpoint auth ออกจาก user management ชัดเจน

## ก่อนเริ่มบทนี้

ให้ตรวจว่าตอนนี้โปรเจกต์มี CRUD user ที่ใช้ database แล้ว และ `User` entity มี field สำหรับ auth ขั้นต้น เช่น `Email`, `PasswordHash`, `Role` และ `IsActive`

บทนี้ยังไม่ hash password และยังไม่สร้าง JWT จริง เราจะเริ่มจาก contract และ DTO ก่อน เพื่อให้บทถัดไปต่อยอดได้เป็นขั้นตอน

## คำศัพท์ในบทนี้

`Contract` คือข้อตกลงระหว่าง client กับ API ว่า endpoint รับ request รูปแบบไหน และตอบ response รูปแบบไหน

`DTO` หรือ Data Transfer Object คือ class ที่ใช้รับ/ส่งข้อมูลผ่าน API โดยแยกจาก entity ใน database เพื่อไม่ให้ response หลุด field ที่ไม่ควรส่ง เช่น `PasswordHash`

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Dtos/Auth/RegisterRequest.cs
Dtos/Auth/LoginRequest.cs
Dtos/Auth/LoginResponse.cs
Dtos/Auth/CurrentUserResponse.cs
```

หลังจบบทนี้ยังไม่มี endpoint auth ที่ทำงานจริงครบ เราแค่เตรียม request/response model ให้พร้อมสำหรับบท hash password, login และ JWT

## Endpoint ที่ต้องมี

```text
POST /api/auth/register
POST /api/auth/login
GET  /api/auth/me
```

`register` ใช้สมัครสมาชิกใหม่

`login` ใช้ตรวจ email/password และออก access token

`me` ใช้อ่านข้อมูลผู้ใช้ที่ login อยู่จาก token

## ทำไมไม่ใช้ POST /api/users สำหรับ register

ก่อนหน้านี้เราใช้ `POST /api/users` เพื่อเรียน CRUD กับ database แต่หลังเข้าสู่ระบบ auth แล้ว การสร้าง user สาธารณะควรอยู่ที่ `POST /api/auth/register`

ส่วน `POST /api/users` จะค่อยถูกปรับให้เป็นงานของ admin ในภาคถัดไป

## สร้างโฟลเดอร์ DTO

สร้างโฟลเดอร์นี้

```text
Dtos/Auth/
```

จากนั้นสร้างไฟล์

```text
Dtos/Auth/RegisterRequest.cs
Dtos/Auth/LoginRequest.cs
Dtos/Auth/LoginResponse.cs
Dtos/Auth/CurrentUserResponse.cs
```

## RegisterRequest

เพิ่ม code นี้ใน `Dtos/Auth/RegisterRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;
}
```

## LoginRequest

เพิ่ม code นี้ใน `Dtos/Auth/LoginRequest.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Auth;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
```

ใน `LoginRequest` เราไม่จำเป็นต้องใช้ `[MinLength(8)]` เพราะถ้ารหัสผ่านสั้นเกินไปก็จะ login ไม่ผ่านอยู่แล้ว การบอก validation ละเอียดเกินไปใน login อาจช่วยผู้โจมตีเดา rule ได้ง่ายขึ้น

## LoginResponse

เพิ่ม code นี้ใน `Dtos/Auth/LoginResponse.cs`

```csharp
namespace Backend.Api.Dtos.Auth;

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public int ExpiresIn { get; set; }
}
```

`ExpiresIn` ใช้หน่วยวินาที เพื่อให้ client รู้ว่า token จะหมดอายุเมื่อไหร่

## CurrentUserResponse

เพิ่ม code นี้ใน `Dtos/Auth/CurrentUserResponse.cs`

```csharp
namespace Backend.Api.Dtos.Auth;

public class CurrentUserResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
```

## Contract ที่คาดหวัง

Register request

```json
{
  "email": "user@example.com",
  "password": "Passw0rd!"
}
```

Register response

```json
{
  "id": 3,
  "email": "user@example.com",
  "role": "User",
  "isActive": true,
  "createdAtUtc": "2026-06-16T03:00:00Z",
  "updatedAtUtc": null
}
```

Login response

```json
{
  "accessToken": "jwt-token-here",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มีโฟลเดอร์ `Dtos/Auth`
- มี `RegisterRequest`, `LoginRequest`, `LoginResponse`, `CurrentUserResponse`
- เข้าใจว่า `POST /api/auth/register` แทนการสมัครสมาชิก ไม่ใช่ `POST /api/users`
- ไม่ส่ง password หรือ password hash กลับไปใน response
