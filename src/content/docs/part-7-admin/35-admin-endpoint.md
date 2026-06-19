---
title: 35 - สร้าง Admin Endpoint
description: ใช้ role-based authorization เพื่อจำกัด endpoint เฉพาะ Admin
---

Admin endpoint คือ endpoint ที่ผู้ใช้ทั่วไปไม่ควรเข้าถึง เช่นดูรายชื่อผู้ใช้ เปลี่ยน role หรือปิดบัญชีผู้ใช้

ในบทนี้เราจะสร้าง controller สำหรับ admin และป้องกันด้วย `[Authorize(Roles = Roles.Admin)]`

## สร้าง AdminUsersController

สร้างไฟล์

```text
Controllers/AdminUsersController.cs
```

เพิ่ม code นี้

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Backend.Api.Constants;

namespace Backend.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[ApiController]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { message = "Admin endpoint is working" });
    }
}
```

เพราะ `Roles.Admin` เป็น `const string` จึงใช้ใน attribute ได้

## ลำดับ middleware ต้องถูกต้อง

ตรวจ `Program.cs` ว่ามีลำดับนี้

```csharp
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

ถ้า `UseAuthentication()` อยู่หลัง `UseAuthorization()` ระบบ authorization จะทำงานผิด

## ทดสอบแบบไม่ส่ง token

```http
GET {{baseUrl}}/api/admin/users/ping
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `401 Unauthorized`

## ทดสอบด้วย user token

login ด้วย `demo-user@example.com` แล้วเอา token มาเรียก admin endpoint

```http
GET {{baseUrl}}/api/admin/users/ping
Authorization: Bearer {{userToken}}
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `403 Forbidden`

เพราะ token ถูกต้อง แต่ role เป็น `User` ไม่ใช่ `Admin`

## ทดสอบด้วย admin token

login ด้วย `admin@example.com` แล้วเอา token มาเรียก endpoint เดิม

```http
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
