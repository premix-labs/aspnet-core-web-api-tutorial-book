---
title: 36 - ดูรายการผู้ใช้สำหรับ Admin
description: สร้าง endpoint สำหรับให้ Admin ดูรายการผู้ใช้ในระบบ
---

บทนี้จะเปลี่ยน admin endpoint จาก `ping` เป็น endpoint ที่ใช้งานจริง คือดูรายชื่อผู้ใช้ในระบบ

ผู้ใช้ทั่วไปไม่ควรเห็นรายการผู้ใช้ทั้งหมด ดังนั้น endpoint นี้ยังต้องอยู่หลัง `[Authorize(Roles = Roles.Admin)]`

## สร้าง DTO สำหรับ Admin

สร้างโฟลเดอร์

```text
Dtos/Admin/
```

สร้างไฟล์

```text
Dtos/Admin/AdminUserResponse.cs
```

เพิ่ม code นี้

```csharp
namespace Backend.Api.Dtos.Admin;

public record AdminUserResponse(
    int Id,
    string Email,
    string Role,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
```

แม้เป็น admin response ก็ยังไม่ส่ง `PasswordHash`

## สร้าง AdminUserService

สร้างไฟล์

```text
Services/AdminUserService.cs
```

เพิ่ม code นี้

```csharp
using Backend.Api.Dtos.Admin;
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class AdminUserService(IUserRepository userRepository)
{
    public async Task<List<AdminUserResponse>> GetUsersAsync()
    {
        var users = await userRepository.GetAllAsync();

        return users.Select(ToResponse).ToList();
    }

    private static AdminUserResponse ToResponse(User user)
    {
        return new AdminUserResponse(
            user.Id,
            user.Email,
            user.Role,
            user.IsActive,
            user.CreatedAtUtc,
            user.UpdatedAtUtc);
    }
}
```

ตอนนี้ใช้ `GetAllAsync()` ก่อนเพื่อให้เข้าใจง่าย บทที่ 40 จะปรับเป็น pagination/filtering/sorting

## ลงทะเบียน AdminUserService

เปิด `Program.cs` แล้วเพิ่ม

```csharp
builder.Services.AddScoped<AdminUserService>();
```

## ปรับ AdminUsersController

แก้ constructor ของ `AdminUsersController`

```csharp
public class AdminUsersController(AdminUserService adminUserService) : ControllerBase
```

แทนที่ action `Ping()` ด้วย action นี้

```csharp
[HttpGet]
public async Task<IActionResult> GetUsers()
{
    var users = await adminUserService.GetUsersAsync();

    return Ok(users);
}
```

## ทดสอบด้วย admin token

```http
GET {{baseUrl}}/api/admin/users
Authorization: Bearer {{adminToken}}
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือรายการผู้ใช้

```json
[
  {
    "id": 1,
    "email": "admin@example.com",
    "role": "Admin",
    "isActive": true,
    "createdAtUtc": "2026-06-16T03:00:00Z",
    "updatedAtUtc": null
  }
]
```

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `AdminUserResponse`
- มี `AdminUserService`
- `AdminUsersController` เรียก service ไม่ query repository เอง
- admin token เรียก `GET /api/admin/users` ได้
- response ไม่มี `PasswordHash`
