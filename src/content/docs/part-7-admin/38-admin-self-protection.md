---
title: 38 - ป้องกันไม่ให้ Admin ลบตัวเองผิดพลาด
description: เพิ่ม business rule เพื่อป้องกัน Admin ทำ action อันตรายกับบัญชีตัวเอง
---

ระบบ admin ต้องมี guardrail สำหรับ action อันตราย เช่น Admin ปิดบัญชีตัวเอง หรือลด role ตัวเองจนไม่มีใครดูแลระบบได้

ในบทนี้เราจะเพิ่ม rule สำคัญก่อนทำ audit log

## Rule ที่ต้องมี

- Admin ห้าม deactivate บัญชีตัวเอง
- Admin ห้ามลด role ตัวเอง
- ระบบไม่ควรเหลือ active admin เป็นศูนย์

## เพิ่ม method นับ active admin ใน Repository

เปิด `IUserRepository.cs` แล้วเพิ่ม method

```csharp
Task<int> CountActiveAdminsAsync();
```

เปิด `UserRepository.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Constants;
```

เพิ่ม implementation

```csharp
public Task<int> CountActiveAdminsAsync()
{
    return db.Users.CountAsync(user =>
        user.Role == Roles.Admin &&
        user.IsActive);
}
```

## Inject CurrentUserService เข้า AdminUserService

เปิด `AdminUserService.cs` แล้วแก้ constructor

```csharp
public class AdminUserService(
    IUserRepository userRepository,
    CurrentUserService currentUserService)
```

เพิ่ม using

```csharp
using Backend.Api.Constants;
```

## เพิ่ม helper ตรวจ current user

เพิ่ม method นี้ใน `AdminUserService`

```csharp
private int GetCurrentAdminId()
{
    if (currentUserService.UserId is null)
    {
        throw new UnauthorizedException("Invalid token", "INVALID_TOKEN");
    }

    return currentUserService.UserId.Value;
}
```

## ปรับ UpdateRoleAsync

แก้ `UpdateRoleAsync` ให้ป้องกันการลด role ตัวเอง

```csharp
public async Task<AdminUserResponse> UpdateRoleAsync(
    int id,
    UpdateUserRoleRequest request)
{
    var currentAdminId = GetCurrentAdminId();
    var user = await userRepository.GetByIdAsync(id);

    if (user is null)
    {
        throw new NotFoundException("User not found", "USER_NOT_FOUND");
    }

    var nextRole = Roles.Normalize(request.Role);

    if (user.Id == currentAdminId && nextRole != Roles.Admin)
    {
        throw new ForbiddenException(
            "Admin cannot demote own account",
            "ADMIN_SELF_DEMOTE_NOT_ALLOWED");
    }

    var activeAdminCount = await userRepository.CountActiveAdminsAsync();

    if (user.Role == Roles.Admin &&
        nextRole != Roles.Admin &&
        user.IsActive &&
        activeAdminCount <= 1)
    {
        throw new ForbiddenException(
            "Cannot remove the last active admin",
            "LAST_ACTIVE_ADMIN_REQUIRED");
    }

    user.Role = nextRole;

    await userRepository.UpdateAsync(user);

    return ToResponse(user);
}
```

## ปรับ UpdateStatusAsync

แก้ `UpdateStatusAsync` ให้ป้องกัน admin ปิดบัญชีตัวเอง และป้องกันการปิด admin คนสุดท้าย

```csharp
public async Task<AdminUserResponse> UpdateStatusAsync(
    int id,
    UpdateUserStatusRequest request)
{
    var currentAdminId = GetCurrentAdminId();
    var user = await userRepository.GetByIdAsync(id);

    if (user is null)
    {
        throw new NotFoundException("User not found", "USER_NOT_FOUND");
    }

    var nextIsActive = request.IsActive!.Value;

    if (user.Id == currentAdminId && !nextIsActive)
    {
        throw new ForbiddenException(
            "Admin cannot deactivate own account",
            "ADMIN_SELF_DEACTIVATE_NOT_ALLOWED");
    }

    var activeAdminCount = await userRepository.CountActiveAdminsAsync();

    if (user.Role == Roles.Admin &&
        user.IsActive &&
        !nextIsActive &&
        activeAdminCount <= 1)
    {
        throw new ForbiddenException(
            "Cannot deactivate the last active admin",
            "LAST_ACTIVE_ADMIN_REQUIRED");
    }

    user.IsActive = nextIsActive;

    await userRepository.UpdateAsync(user);

    return ToResponse(user);
}
```

## ทดสอบ self-protection

login ด้วย admin แล้วลองปิดบัญชีตัวเอง

```http
PUT {{baseUrl}}/api/admin/users/1/status
Authorization: Bearer {{adminToken}}
Content-Type: application/json

{
  "isActive": false
}
```

ผลลัพธ์ที่คาดหวังคือ `403 Forbidden`

```json
{
  "title": "Admin cannot deactivate own account",
  "status": 403,
  "code": "ADMIN_SELF_DEACTIVATE_NOT_ALLOWED"
}
```

ลองลด role ตัวเอง

```http
PUT {{baseUrl}}/api/admin/users/1/role
Authorization: Bearer {{adminToken}}
Content-Type: application/json

{
  "role": "User"
}
```

ควรได้ `403 Forbidden` เช่นกัน

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- `IUserRepository` มี `CountActiveAdminsAsync`
- `AdminUserService` ใช้ `CurrentUserService`
- Admin ปิดบัญชีตัวเองไม่ได้
- Admin ลด role ตัวเองไม่ได้
- ระบบไม่เหลือ active admin เป็นศูนย์
