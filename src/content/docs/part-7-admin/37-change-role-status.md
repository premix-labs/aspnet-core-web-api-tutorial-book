---
title: 37 - เปลี่ยน Role หรือสถานะผู้ใช้
description: สร้าง endpoint ให้ Admin เปลี่ยน role และเปิดปิดบัญชีผู้ใช้
---

Admin มักต้องจัดการผู้ใช้ เช่นเปลี่ยน role จาก `User` เป็น `Admin` หรือปิดบัญชีที่ไม่ควรใช้งานต่อ

บทนี้จะเพิ่ม endpoint สองตัว

```text
PUT /api/admin/users/{id}/role
PUT /api/admin/users/{id}/status
```

## สร้าง UpdateUserRoleRequest

สร้างไฟล์

```text
Dtos/Admin/UpdateUserRoleRequest.cs
```

เพิ่ม code นี้

```csharp
using System.ComponentModel.DataAnnotations;
using Backend.Api.Constants;

namespace Backend.Api.Dtos.Admin;

public class UpdateUserRoleRequest : IValidatableObject
{
    [Required]
    public string Role { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Roles.IsValid(Role))
        {
            yield return new ValidationResult(
                "Role is invalid.",
                [nameof(Role)]);
        }
    }
}
```

ใช้ `IValidatableObject` เพราะ role ที่อนุญาตมาจากระบบของเรา ไม่ใช่ validation attribute สำเร็จรูป

## สร้าง UpdateUserStatusRequest

สร้างไฟล์

```text
Dtos/Admin/UpdateUserStatusRequest.cs
```

เพิ่ม code นี้

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Admin;

public class UpdateUserStatusRequest
{
    [Required]
    public bool? IsActive { get; set; }
}
```

ใช้ `bool?` เพราะถ้าใช้ `bool` ธรรมดา ค่า default จะเป็น `false` ทำให้แยกไม่ได้ว่า client ตั้งใจส่ง `false` หรือไม่ได้ส่ง field นี้มา

## เพิ่ม method ใน AdminUserService

เปิด `AdminUserService.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Exceptions;
```

เพิ่ม method เปลี่ยน role

```csharp
public async Task<AdminUserResponse> UpdateRoleAsync(
    int id,
    UpdateUserRoleRequest request)
{
    var user = await userRepository.GetByIdAsync(id);

    if (user is null)
    {
        throw new NotFoundException("User not found", "USER_NOT_FOUND");
    }

    user.Role = Roles.Normalize(request.Role);

    await userRepository.UpdateAsync(user);

    return ToResponse(user);
}
```

ถึงแม้ `Roles.IsValid()` จะตรวจแบบไม่สนตัวพิมพ์เล็กใหญ่ แต่ตอนบันทึกควรใช้ `Roles.Normalize()` เพื่อเก็บค่าเป็น `Admin` หรือ `User` ตามที่ระบบกำหนดเสมอ

เพิ่ม method เปลี่ยนสถานะ

```csharp
public async Task<AdminUserResponse> UpdateStatusAsync(
    int id,
    UpdateUserStatusRequest request)
{
    var user = await userRepository.GetByIdAsync(id);

    if (user is null)
    {
        throw new NotFoundException("User not found", "USER_NOT_FOUND");
    }

    user.IsActive = request.IsActive!.Value;

    await userRepository.UpdateAsync(user);

    return ToResponse(user);
}
```

เครื่องหมาย `!` หลัง `IsActive` ใช้บอก compiler ว่า validation ผ่านแล้วจึงไม่เป็น null

## เพิ่ม endpoint ใน AdminUsersController

เปิด `AdminUsersController.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Dtos.Admin;
```

เพิ่ม action เปลี่ยน role

```csharp
[HttpPut("{id:int}/role")]
public async Task<IActionResult> UpdateRole(
    int id,
    UpdateUserRoleRequest request)
{
    var user = await adminUserService.UpdateRoleAsync(id, request);

    return Ok(user);
}
```

เพิ่ม action เปลี่ยนสถานะ

```csharp
[HttpPut("{id:int}/status")]
public async Task<IActionResult> UpdateStatus(
    int id,
    UpdateUserStatusRequest request)
{
    var user = await adminUserService.UpdateStatusAsync(id, request);

    return Ok(user);
}
```

## ทดสอบเปลี่ยน role

```http
PUT {{baseUrl}}/api/admin/users/2/role
Authorization: Bearer {{adminToken}}
Content-Type: application/json

{
  "role": "Admin"
}
```

ถ้าส่ง role ที่ไม่อยู่ใน `Roles.All`

```json
{
  "role": "SuperAdmin"
}
```

ควรได้ `400 Bad Request` จาก validation

## ทดสอบปิดบัญชีผู้ใช้

```http
PUT {{baseUrl}}/api/admin/users/2/status
Authorization: Bearer {{adminToken}}
Content-Type: application/json

{
  "isActive": false
}
```

## สิ่งที่ยังไม่สมบูรณ์

ตอนนี้ admin ยังสามารถปิดบัญชีตัวเองหรือลด role ตัวเองได้ ซึ่งอันตราย

บทถัดไปจะเพิ่ม self-protection rule เพื่อป้องกันกรณีนี้

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `UpdateUserRoleRequest`
- มี `UpdateUserStatusRequest`
- Admin เปลี่ยน role ผู้ใช้ได้
- Admin เปิดปิดบัญชีผู้ใช้ได้
- role ที่ไม่ถูกต้องตอบ `400 Bad Request`
- เข้าใจว่าต้องเพิ่ม self-protection ก่อนถือว่าพร้อมใช้งานจริง
