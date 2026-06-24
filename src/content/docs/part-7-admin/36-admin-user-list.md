---
title: 36 - ดูรายการผู้ใช้สำหรับ Admin
description: สร้าง endpoint สำหรับให้ Admin ดูรายการผู้ใช้ในระบบ
---

บทนี้จะเปลี่ยน admin endpoint จาก `ping` เป็น endpoint ที่ใช้งานจริง คือดูรายชื่อผู้ใช้ในระบบ

ผู้ใช้ทั่วไปไม่ควรเห็นรายการผู้ใช้ทั้งหมด ดังนั้น endpoint นี้ยังต้องอยู่หลัง `[Authorize(Roles = Roles.Admin)]`

## วิธีเรียนบทนี้

ให้ทำทีละชั้น:

1. สร้าง response DTO สำหรับ admin
2. สร้าง `AdminUserService`
3. ลงทะเบียน service
4. เปลี่ยน `AdminUsersController` ให้เรียก service
5. ทดสอบด้วย admin token

## ก่อนเริ่มบทนี้

ให้ตรวจว่าคุณทำบท admin ก่อนหน้าแล้ว:

- มี `Constants/Roles.cs`
- `DataSeeder` สร้าง admin user สำหรับทดสอบ
- มี `AdminUsersController` ที่ใช้ `[Authorize(Roles = Roles.Admin)]`
- admin token เรียก `GET /api/admin/users/ping` ได้
- `IUserRepository.GetAllAsync()` ใช้งานได้จากบท database CRUD

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `AdminUserResponse` | DTO สำหรับข้อมูล user ที่ admin เห็น |
| `AdminUserService` | service สำหรับ use case ของ admin |
| `GetAllAsync()` | repository method สำหรับอ่าน user ทั้งหมดชั่วคราว |
| `ToResponse(...)` | mapping จาก `User` entity เป็น admin DTO |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Dtos/Admin/AdminUserResponse.cs
Services/AdminUserService.cs
Controllers/AdminUsersController.cs
Program.cs
```

บทนี้ยังใช้ `User.Id` แบบ `int` และ timestamp แบบ `CreatedAtUtc`/`UpdatedAtUtc` ตาม progressive model ปัจจุบัน การย้ายเป็น `Guid` และ `CreatedAt` จะอยู่ในภาค production hardening

## ขั้นที่ 1: สร้าง DTO สำหรับ Admin

รันจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force -Path Dtos/Admin
New-Item -ItemType File -Path Dtos/Admin/AdminUserResponse.cs
```

macOS/Linux Bash:

```bash
mkdir -p Dtos/Admin
touch Dtos/Admin/AdminUserResponse.cs
```

เปิดไฟล์:

```text
Dtos/Admin/AdminUserResponse.cs
```

เพิ่ม code นี้:

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

## ขั้นที่ 2: สร้าง AdminUserService

รันจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType File -Path Services/AdminUserService.cs
```

macOS/Linux Bash:

```bash
touch Services/AdminUserService.cs
```

เปิดไฟล์:

```text
Services/AdminUserService.cs
```

เริ่มจาก using และ class:

```csharp
using Backend.Api.Dtos.Admin;
using Backend.Api.Models;
using Backend.Api.Repositories;

namespace Backend.Api.Services;

public class AdminUserService(IUserRepository userRepository)
{
}
```

## ขั้นที่ 3: เพิ่ม GetUsersAsync

เพิ่ม method นี้ใน class:

```csharp
public async Task<List<AdminUserResponse>> GetUsersAsync()
{
    var users = await userRepository.GetAllAsync();

    return users.Select(ToResponse).ToList();
}
```

ตอนนี้ใช้ `GetAllAsync()` ก่อนเพื่อให้เข้าใจง่าย บทที่ 40 จะปรับเป็น pagination/filtering/sorting

## ขั้นที่ 4: เพิ่ม mapping method

เพิ่ม method นี้ไว้ท้าย class:

```csharp
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
```

mapping method เป็นจุดที่ควบคุมว่า admin response ส่ง field ไหนออก API

## ขั้นที่ 5: ลงทะเบียน AdminUserService

เปิด `Program.cs` แล้วเพิ่มก่อน `builder.Build()`:

```csharp
builder.Services.AddScoped<AdminUserService>();
```

## ขั้นที่ 6: ปรับ AdminUsersController

แก้ constructor ของ `AdminUsersController`:

```csharp
public class AdminUsersController(AdminUserService adminUserService) : ControllerBase
```

แทนที่ action `Ping()` ด้วย action นี้:

```csharp
[HttpGet]
public async Task<IActionResult> GetUsers()
{
    var users = await adminUserService.GetUsersAsync();

    return Ok(users);
}
```

Controller ยังไม่ query repository เอง หน้าที่ของมันคือรับ request แล้วเรียก service

## ตรวจ build และทดสอบ

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet build
dotnet run
```

ทดสอบด้วย admin token:

```http
@baseUrl = http://localhost:5156
@adminToken = paste-admin-token-here

### Admin user list
GET {{baseUrl}}/api/admin/users
Authorization: Bearer {{adminToken}}
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือรายการผู้ใช้:

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
