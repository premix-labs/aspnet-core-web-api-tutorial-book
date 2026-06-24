---
title: 35 - สร้าง Admin Endpoint
description: ใช้ role-based authorization เพื่อจำกัด endpoint เฉพาะ Admin
---

Admin endpoint คือ endpoint ที่ผู้ใช้ทั่วไปไม่ควรเข้าถึง เช่นดูรายชื่อผู้ใช้ เปลี่ยน role หรือปิดบัญชีผู้ใช้

ในบทนี้เราจะสร้าง controller สำหรับ admin และป้องกันด้วย `[Authorize(Roles = Roles.Admin)]`

## วิธีเรียนบทนี้

บทนี้ยังไม่ทำ user management จริง ให้สร้าง endpoint เล็ก ๆ ชื่อ `ping` เพื่อพิสูจน์ก่อนว่า role-based authorization ทำงาน:

1. สร้าง `AdminUsersController`
2. ใส่ `[Authorize(Roles = Roles.Admin)]`
3. ตรวจ middleware order
4. ทดสอบ 3 กรณี: ไม่มี token, user token, admin token

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `[Authorize]` | บังคับ authentication/authorization |
| `Roles.Admin` | role ที่อนุญาตให้เข้า endpoint |
| `401 Unauthorized` | ไม่มี token หรือ token ไม่ถูกต้อง |
| `403 Forbidden` | token ถูกต้องแต่ role ไม่พอ |
| `200 OK` | admin token ผ่านและเรียก endpoint ได้ |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Controllers/AdminUsersController.cs
Program.cs
Backend.Api.http
```

## ขั้นที่ 1: สร้าง AdminUsersController

รันจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType File -Path Controllers/AdminUsersController.cs
```

macOS/Linux Bash:

```bash
touch Controllers/AdminUsersController.cs
```

เปิดไฟล์:

```text
Controllers/AdminUsersController.cs
```

เพิ่ม using และ namespace:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Constants;

namespace Backend.Api.Controllers;
```

## ขั้นที่ 2: เพิ่ม controller และ route

เพิ่ม class นี้:

```csharp
[Authorize(Roles = Roles.Admin)]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
}
```

`Roles.Admin` เป็น `const string` จึงใช้ใน attribute ได้

## ขั้นที่ 3: เพิ่ม ping endpoint

เพิ่ม action นี้ใน controller:

```csharp
[HttpGet("ping")]
public IActionResult Ping()
{
    return Ok(new { message = "Admin endpoint is working" });
}
```

endpoint นี้ใช้ทดสอบ authorization ก่อนเพิ่ม logic จริงในบทถัดไป

## ขั้นที่ 4: ตรวจลำดับ middleware

เปิด `Program.cs` แล้วตรวจว่ามีลำดับนี้:

```csharp
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

ถ้า `UseAuthentication()` อยู่หลัง `UseAuthorization()` ระบบ authorization จะทำงานผิด

## ทดสอบแบบไม่ส่ง token

```http
@baseUrl = http://localhost:5156

### Admin ping without token
GET {{baseUrl}}/api/admin/users/ping
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `401 Unauthorized`

## ทดสอบด้วย user token

login ด้วย `demo-user@example.com` แล้วเอา token มาเรียก admin endpoint

```http
@userToken = paste-user-token-here

### Admin ping with user token
GET {{baseUrl}}/api/admin/users/ping
Authorization: Bearer {{userToken}}
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `403 Forbidden`

เพราะ token ถูกต้อง แต่ role เป็น `User` ไม่ใช่ `Admin`

## ทดสอบด้วย admin token

login ด้วย `admin@example.com` แล้วเอา token มาเรียก endpoint เดิม

```http
@adminToken = paste-admin-token-here

### Admin ping with admin token
GET {{baseUrl}}/api/admin/users/ping
Authorization: Bearer {{adminToken}}
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `200 OK`

```json
{
  "message": "Admin endpoint is working"
}
```

## 401 กับ 403 ต่างกันตรงนี้

`401 Unauthorized` คือยังยืนยันตัวตนไม่สำเร็จ เช่นไม่ส่ง token หรือ token ผิด

`403 Forbidden` คือยืนยันตัวตนสำเร็จแล้ว แต่สิทธิ์ไม่พอ เช่น user ธรรมดาเข้า admin endpoint

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `AdminUsersController`
- controller ถูกป้องกันด้วย `[Authorize(Roles = Roles.Admin)]`
- ไม่ส่ง token แล้วได้ `401`
- ส่ง user token แล้วได้ `403`
- ส่ง admin token แล้วได้ `200`
