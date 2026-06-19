---
title: 30 - สร้าง Login API
description: ตรวจ email และ password แล้วเตรียมคืน token เมื่อ login สำเร็จ
---

Login API มีหน้าที่รับ email และ password ค้นผู้ใช้ ตรวจ password และคืน access token เมื่อข้อมูลถูกต้อง

ในบทนี้เราจะทำ flow login และ error handling ให้ถูกก่อน ส่วนการสร้าง JWT token จริงจะอยู่บทถัดไป

## เพิ่ม UnauthorizedException

สร้างไฟล์

```text
Exceptions/UnauthorizedException.cs
```

เพิ่ม code นี้

```csharp
using Microsoft.AspNetCore.Http;

namespace Backend.Api.Exceptions;

public class UnauthorizedException(string message, string code)
    : ApiException(message, code, StatusCodes.Status401Unauthorized);
```

## เพิ่ม ForbiddenException

สร้างไฟล์

```text
Exceptions/ForbiddenException.cs
```

เพิ่ม code นี้

```csharp
using Microsoft.AspNetCore.Http;

namespace Backend.Api.Exceptions;

public class ForbiddenException(string message, string code)
    : ApiException(message, code, StatusCodes.Status403Forbidden);
```

`UnauthorizedException` ใช้เมื่อ login ไม่ผ่านหรือ token ไม่ถูกต้อง

`ForbiddenException` ใช้เมื่อรู้แล้วว่าผู้ใช้คือใคร แต่ไม่มีสิทธิ์ หรือบัญชีถูกปิดใช้งาน

## เพิ่ม LoginAsync ใน AuthService

เปิด `AuthService.cs` แล้วเพิ่ม method นี้

```csharp
public async Task<LoginResponse> LoginAsync(LoginRequest request)
{
    var user = await userRepository.GetByEmailAsync(request.Email);

    if (user is null)
    {
        throw new UnauthorizedException(
            "Invalid email or password",
            "INVALID_CREDENTIALS");
    }

    var verificationResult = passwordHasher.VerifyHashedPassword(
        user,
        user.PasswordHash,
        request.Password);

    if (verificationResult == PasswordVerificationResult.Failed)
    {
        throw new UnauthorizedException(
            "Invalid email or password",
            "INVALID_CREDENTIALS");
    }

    if (!user.IsActive)
    {
        throw new ForbiddenException(
            "User account is inactive",
            "USER_INACTIVE");
    }

    return new LoginResponse
    {
        AccessToken = "temporary-token-created-in-next-chapter",
        TokenType = "Bearer",
        ExpiresIn = 0
    };
}
```

ตอนนี้ method นี้ยังคืน temporary token เพื่อให้โปรเจกต์ compile และทดสอบ flow login ได้ก่อน ในบทถัดไปเราจะแทนส่วนนี้ด้วย JWT token จริง

## ทำไม email หรือ password ผิดต้องตอบข้อความเดียวกัน

ถ้า email ไม่พบแล้วตอบว่า `Email not found` แต่ password ผิดตอบว่า `Password is incorrect` ผู้โจมตีจะใช้ API ตรวจได้ว่า email ไหนมีอยู่ในระบบ

ดังนั้นทั้งสองกรณีควรตอบข้อความกลาง ๆ ว่า `Invalid email or password`

## เพิ่ม endpoint login

เปิด `AuthController.cs` แล้วเพิ่ม action นี้

```csharp
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    var response = await authService.LoginAsync(request);

    return Ok(response);
}
```

## ทดสอบ login flow เบื้องต้น

หลังจบบทนี้ login ที่ email/password ถูกต้องจะได้ temporary token กลับมา

```http
POST https://localhost:7001/api/auth/login
Content-Type: application/json

{
  "email": "demo-user@example.com",
  "password": "User1234!"
}
```

ผลลัพธ์ที่คาดหวังชั่วคราวคือ

```json
{
  "accessToken": "temporary-token-created-in-next-chapter",
  "tokenType": "Bearer",
  "expiresIn": 0
}
```

บทถัดไปจะเปลี่ยน response นี้ให้เป็น JWT จริง

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `UnauthorizedException`
- มี `ForbiddenException`
- `AuthService.LoginAsync` ค้น user จาก email
- ตรวจ password ด้วย `VerifyHashedPassword`
- email หรือ password ผิดตอบ `INVALID_CREDENTIALS`
- บัญชี inactive ตอบ `USER_INACTIVE`
- มี `POST /api/auth/login` ใน `AuthController`
