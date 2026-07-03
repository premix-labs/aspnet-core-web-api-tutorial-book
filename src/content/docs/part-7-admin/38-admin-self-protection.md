---
title: 38 - ป้องกันไม่ให้ Admin ลบตัวเองผิดพลาด
description: เพิ่ม business rule เพื่อป้องกัน Admin ทำ action อันตรายกับบัญชีตัวเอง
---

ระบบ admin ต้องมี guardrail สำหรับ action อันตราย เช่น Admin ปิดบัญชีตัวเอง หรือลด role ตัวเองจนไม่มีใครดูแลระบบได้

ในบทนี้เราจะเพิ่ม rule สำคัญก่อนทำ audit log

## วิธีเรียนบทนี้

ให้เพิ่ม rule ทีละข้อ:

1. ให้ repository นับ active admin ได้
2. ให้ `AdminUserService` อ่าน current admin id
3. กัน admin ลด role ตัวเอง
4. กัน admin ปิดบัญชีตัวเอง
5. กันระบบเหลือ active admin เป็นศูนย์

## Rule ที่ต้องมี

- Admin ห้าม deactivate บัญชีตัวเอง
- Admin ห้ามลด role ตัวเอง
- ระบบไม่ควรเหลือ active admin เป็นศูนย์

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `CountActiveAdminsAsync()` | นับ admin ที่ยัง active อยู่ |
| `CurrentUserService` | อ่าน id ของ admin ที่กำลังเรียก endpoint |
| `ForbiddenException` | ใช้กับ action ที่รู้ผู้ใช้แล้วแต่ไม่อนุญาต |
| `ADMIN_SELF_DEMOTE_NOT_ALLOWED` | error code เมื่อ admin ลด role ตัวเอง |
| `ADMIN_SELF_DEACTIVATE_NOT_ALLOWED` | error code เมื่อ admin ปิดบัญชีตัวเอง |
| `LAST_ACTIVE_ADMIN_REQUIRED` | error code เมื่อ action จะทำให้ไม่มี admin active |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Repositories/IUserRepository.cs
Repositories/UserRepository.cs
Services/AdminUserService.cs
```

## ขั้นที่ 1: เพิ่ม method นับ active admin ใน Repository

เปิด `Repositories/IUserRepository.cs` แล้วเพิ่ม method:

```csharp
Task<int> CountActiveAdminsAsync();
```

เปิด `Repositories/UserRepository.cs` แล้วเพิ่ม using:

```csharp
using Backend.Api.Constants;
```

เพิ่ม implementation:

```csharp
public Task<int> CountActiveAdminsAsync()
{
    return db.Users.CountAsync(user =>
        user.Role == Roles.Admin &&
        user.IsActive);
}
```

method นี้ใช้ตรวจว่า action ที่กำลังทำจะทำให้ระบบไม่มี admin active เหลืออยู่หรือไม่

## ขั้นที่ 2: Inject CurrentUserService เข้า AdminUserService

เปิด `Services/AdminUserService.cs` แล้วแก้ constructor:

```csharp
public class AdminUserService(
    IUserRepository userRepository,
    CurrentUserService currentUserService)
```

ตรวจว่ามี using เหล่านี้:

```csharp
using Backend.Api.Constants;
using Backend.Api.Exceptions;
```

## ขั้นที่ 3: เพิ่ม helper อ่าน current admin id

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

เราต้องรู้ว่า admin ที่กำลังเรียก endpoint คือใคร เพื่อกันไม่ให้จัดการบัญชีตัวเองผิดพลาด

## ขั้นที่ 4: กัน self-demote ใน UpdateRoleAsync

ใน `UpdateRoleAsync` ให้เริ่ม method ด้วย current admin id:

```csharp
var currentAdminId = GetCurrentAdminId();
```

หลังหา user และ normalize role แล้ว เพิ่ม rule นี้:

```csharp
var nextRole = Roles.Normalize(request.Role);

if (user.Id == currentAdminId && nextRole != Roles.Admin)
{
    throw new ForbiddenException(
        "Admin cannot demote own account",
        "ADMIN_SELF_DEMOTE_NOT_ALLOWED");
}
```

ถ้า admin กำลังแก้บัญชีตัวเองและ role ใหม่ไม่ใช่ `Admin` ให้ปฏิเสธทันที

## ขั้นที่ 5: กัน last active admin ตอนเปลี่ยน role

เพิ่ม rule นี้ก่อนบันทึก role:

```csharp
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
```

rule นี้กันกรณี admin คนสุดท้ายถูกลด role ทำให้ระบบไม่มี admin active เหลือ

หลังผ่าน guard แล้วค่อยบันทึก:

```csharp
user.Role = nextRole;

await userRepository.UpdateAsync(user);

return ToResponse(user);
```

## ขั้นที่ 6: กัน self-deactivate ใน UpdateStatusAsync

ใน `UpdateStatusAsync` ให้เริ่มด้วย current admin id:

```csharp
var currentAdminId = GetCurrentAdminId();
```

หลังหา user แล้ว อ่าน status ใหม่:

```csharp
var nextIsActive = request.IsActive!.Value;
```

เพิ่ม rule กันปิดบัญชีตัวเอง:

```csharp
if (user.Id == currentAdminId && !nextIsActive)
{
    throw new ForbiddenException(
        "Admin cannot deactivate own account",
        "ADMIN_SELF_DEACTIVATE_NOT_ALLOWED");
}
```

## ขั้นที่ 7: กัน last active admin ตอนเปลี่ยน status

เพิ่ม rule นี้ก่อนบันทึก status:

```csharp
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
```

หลังผ่าน guard แล้วค่อยบันทึก:

```csharp
user.IsActive = nextIsActive;

await userRepository.UpdateAsync(user);

return ToResponse(user);
```

## ตรวจ build และทดสอบ

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet build
```

login ด้วย admin แล้วลองปิดบัญชีตัวเอง:

```http
@baseUrl = http://localhost:<http-port>
@adminUsersPath = {{baseUrl}}/api/v1/admin/users
@adminToken = paste-admin-token-here
@adminUserId = paste-admin-user-id-here

### Admin self deactivate
PUT {{adminUsersPath}}/{{adminUserId}}/status
Authorization: Bearer {{adminToken}}
Content-Type: application/json

{
  "isActive": false
}
```

`adminUserId` ต้องเป็น id ของบัญชี admin ที่ใช้ login อยู่ เช่น `admin@example.com` ถ้าใช้ id ของ user คนอื่น test นี้จะไม่ใช่ self-protection

ผลลัพธ์ที่คาดหวังคือ `403 Forbidden` พร้อม code:

```json
{
  "title": "Admin cannot deactivate own account",
  "status": 403,
  "code": "ADMIN_SELF_DEACTIVATE_NOT_ALLOWED"
}
```

ลองลด role ตัวเอง:

```http
### Admin self demote
PUT {{adminUsersPath}}/{{adminUserId}}/role
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
- test ใช้ `adminUserId` ของบัญชีที่ login จริง ไม่ใช่ id เดาสุ่ม
