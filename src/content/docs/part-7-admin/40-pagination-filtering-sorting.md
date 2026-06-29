---
title: 40 - Pagination, Filtering และ Sorting
description: จัดการรายการผู้ใช้จำนวนมากใน admin API อย่างควบคุมได้
---

Admin API ที่แสดงรายการผู้ใช้ไม่ควรคืนข้อมูลทั้งหมดในครั้งเดียว เพราะเมื่อข้อมูลโตขึ้น API จะช้า ใช้ memory มาก และ frontend จัดการยาก

บทนี้จะปรับ `GET /api/admin/users` ให้รองรับ pagination, filtering และ sorting

## วิธีเรียนบทนี้

บทนี้มีโค้ดหลายจุด ให้ทำตามลำดับนี้:

1. สร้าง response กลางชื่อ `PagedResponse<T>`
2. สร้าง query object ชื่อ `AdminUserQuery`
3. เพิ่ม method query ใน repository
4. ปรับ service ให้แปลง `User` เป็น `AdminUserResponse`
5. ปรับ controller ให้รับค่าจาก query string
6. ทดสอบด้วยไฟล์ `.http`

อย่าข้ามไปแก้ controller ก่อน repository เพราะ controller ต้องเรียก method ที่ยังไม่มี

## ก่อนเริ่มบทนี้

ให้ตรวจว่าภาค admin ก่อนหน้านี้ทำงานครบแล้ว:

- `GET /api/admin/users` คืนรายการผู้ใช้ได้
- Admin เปลี่ยน role/status ได้
- self-protection ทำงาน
- มี audit log สำหรับ admin action
- `AdminUserService` เป็นจุดกลางที่ controller เรียกใช้งาน

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `PagedResponse<T>` | response กลางสำหรับข้อมูลแบบแบ่งหน้า |
| `[FromQuery]` | บอก ASP.NET Core ให้ bind ค่าจาก query string |
| `IQueryable<T>` | query ที่ยังไม่ยิง database จนกว่าจะ `ToListAsync` หรือ `CountAsync` |
| `AsNoTracking()` | อ่านข้อมูลโดยไม่ให้ EF Core track entity เหมาะกับ read-only query |
| `Skip()` | ข้ามข้อมูลก่อนหน้าตามเลขหน้า |
| `Take()` | จำกัดจำนวนข้อมูลในหน้านั้น |
| `CountAsync()` | นับจำนวนทั้งหมดหลัง filter แต่ก่อนแบ่งหน้า |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Dtos/Common/PagedResponse.cs
Dtos/Admin/AdminUserQuery.cs
Repositories/IUserRepository.cs
Repositories/UserRepository.cs
Services/AdminUserService.cs
Controllers/AdminUsersController.cs
Backend.Api.http
```

หลังจบบทนี้ response ของ `GET /api/admin/users` จะเปลี่ยนจาก array ตรง ๆ เป็น object ที่มี `items`, `page`, `pageSize`, `totalItems` และ `totalPages`

## Query ที่ต้องรองรับ

```text
GET /api/admin/users?page=1&pageSize=20&search=admin&role=Admin&isActive=true&sortBy=createdAtUtc&sortDirection=desc
```

ความหมายของ query string:

- `page` คือหน้าที่ต้องการ เริ่มจาก `1`
- `pageSize` คือจำนวนรายการต่อหน้า
- `search` คือคำค้นจาก email
- `role` คือ role ที่ต้องการกรอง เช่น `Admin` หรือ `User`
- `isActive` คือสถานะบัญชี
- `sortBy` คือ field ที่ใช้เรียง
- `sortDirection` คือ `asc` หรือ `desc`

## ขั้นที่ 1: สร้าง PagedResponse

รันจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force -Path Dtos/Common
New-Item -ItemType File -Force -Path Dtos/Common/PagedResponse.cs
```

macOS/Linux Bash:

```bash
mkdir -p Dtos/Common
touch Dtos/Common/PagedResponse.cs
```

เปิดไฟล์:

```text
Dtos/Common/PagedResponse.cs
```

เพิ่ม code:

```csharp
namespace Backend.Api.Dtos.Common;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages =>
        (int)Math.Ceiling(TotalItems / (double)PageSize);
}
```

`T` คือ generic type ทำให้ response นี้ใช้ได้กับหลายชนิดข้อมูล เช่น `PagedResponse<AdminUserResponse>` หรือ `PagedResponse<User>`

## ขั้นที่ 2: สร้าง AdminUserQuery

รันจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType File -Force -Path Dtos/Admin/AdminUserQuery.cs
```

macOS/Linux Bash:

```bash
touch Dtos/Admin/AdminUserQuery.cs
```

เปิดไฟล์:

```text
Dtos/Admin/AdminUserQuery.cs
```

เริ่มด้วย using และ class:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Admin;

public class AdminUserQuery
{
}
```

เพิ่ม `Page` และ `PageSize`:

```csharp
[Range(1, int.MaxValue)]
public int Page { get; set; } = 1;

[Range(1, 100)]
public int PageSize { get; set; } = 20;
```

`[Range]` คือ validation attribute ที่ช่วยกันค่าผิด เช่น `page=0` หรือ `pageSize=5000`

เพิ่ม filter:

```csharp
[StringLength(256)]
public string? Search { get; set; }

public string? Role { get; set; }

public bool? IsActive { get; set; }
```

`bool?` แปลว่า nullable bool ถ้า client ไม่ส่ง `isActive` เข้ามา เราจะไม่กรองด้วยสถานะ

เพิ่ม sorting:

```csharp
public string SortBy { get; set; } = "createdAtUtc";

public string SortDirection { get; set; } = "desc";
```

## ขั้นที่ 3: เพิ่ม method ใน IUserRepository

เปิด `Repositories/IUserRepository.cs`

เพิ่ม using:

```csharp
using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;
```

เพิ่ม method ใน interface:

```csharp
Task<PagedResponse<User>> QueryUsersAsync(AdminUserQuery query);
```

Repository คืน `User` เพราะ repository ยังอยู่ชั้น data access ส่วนการแปลงเป็น DTO จะทำใน service

## ขั้นที่ 4: เริ่ม QueryUsersAsync ใน UserRepository

เปิด `Repositories/UserRepository.cs`

เพิ่ม using:

```csharp
using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;
```

เริ่ม method ด้วย query พื้นฐาน:

```csharp
public async Task<PagedResponse<User>> QueryUsersAsync(AdminUserQuery query)
{
    var users = db.Users.AsNoTracking().AsQueryable();
}
```

`AsQueryable()` ทำให้เราค่อย ๆ ต่อเงื่อนไขได้ เช่น search ก่อน filter role แล้วค่อย sort

## ขั้นที่ 5: เพิ่ม search filter

ใน `QueryUsersAsync` เพิ่ม code นี้ต่อจาก `var users = ...`

```csharp
if (!string.IsNullOrWhiteSpace(query.Search))
{
    var keyword = query.Search.Trim();

    users = users.Where(user =>
        user.Email.Contains(keyword));
}
```

ใน progressive model ตอนนี้ยังไม่มี `NormalizedEmail` จึงค้นจาก `Email` ตรง ๆ ก่อน การย้ายไปใช้ `NormalizedEmail` และ index สำหรับค้นหาแบบ production จะอยู่ในภาค production hardening

## ขั้นที่ 6: เพิ่ม role และ status filter

เพิ่มต่อจาก search filter:

```csharp
if (!string.IsNullOrWhiteSpace(query.Role))
{
    var role = Roles.IsValid(query.Role)
        ? Roles.Normalize(query.Role)
        : query.Role;

    users = users.Where(user => user.Role == role);
}
```

`Roles.Normalize` ช่วยให้ `admin`, `ADMIN` และ `Admin` ถูกแปลงเป็นรูปแบบเดียวกัน

เพิ่ม status filter:

```csharp
if (query.IsActive.HasValue)
{
    users = users.Where(user =>
        user.IsActive == query.IsActive.Value);
}
```

ถ้าไม่ส่ง `isActive` มา `HasValue` จะเป็น `false` และ query จะไม่กรองสถานะ

## ขั้นที่ 7: เพิ่ม sorting

เพิ่มต่อจาก filter ทั้งหมด:

```csharp
users = (query.SortBy.ToLowerInvariant(),
    query.SortDirection.ToLowerInvariant()) switch
{
    ("email", "asc") => users.OrderBy(user => user.Email),
    ("email", "desc") => users.OrderByDescending(user => user.Email),
    ("role", "asc") => users.OrderBy(user => user.Role),
    ("role", "desc") => users.OrderByDescending(user => user.Role),
    ("createdatutc", "asc") => users.OrderBy(user => user.CreatedAtUtc),
    _ => users.OrderByDescending(user => user.CreatedAtUtc)
};
```

เราใช้ `switch` เพื่อจำกัด field ที่ sort ได้ ไม่เปิดให้ client ส่งชื่อ column อะไรก็ได้เข้ามา

ชื่อ `createdAtUtc` ต้องตรงกับ property ที่มีอยู่ใน `User` ตอนนี้ ถ้าใช้ `CreatedAt` จะ build ไม่ผ่าน เพราะ field นั้นจะถูกเพิ่มในภาค production hardening ภายหลัง

## ขั้นที่ 8: นับจำนวนและแบ่งหน้า

เพิ่มท้าย method:

```csharp
var totalItems = await users.CountAsync();

var items = await users
    .Skip((query.Page - 1) * query.PageSize)
    .Take(query.PageSize)
    .ToListAsync();
```

`CountAsync()` ต้องอยู่ก่อน `Skip()` และ `Take()` เพราะเราต้องการจำนวนทั้งหมดหลัง filter ไม่ใช่จำนวนเฉพาะหน้าปัจจุบัน

คืน response:

```csharp
return new PagedResponse<User>(
    items,
    query.Page,
    query.PageSize,
    totalItems);
```

ตอนนี้ `QueryUsersAsync` ควรมี flow เป็น:

```text
เริ่มจาก Users query
เพิ่ม search filter
เพิ่ม role filter
เพิ่ม status filter
เพิ่ม sorting
นับ totalItems
แบ่งหน้าด้วย Skip/Take
คืน PagedResponse<User>
```

## ขั้นที่ 9: ปรับ AdminUserService

เปิด `Services/AdminUserService.cs`

เพิ่ม using:

```csharp
using Backend.Api.Dtos.Common;
```

เปลี่ยน `GetUsersAsync` ให้รับ `AdminUserQuery`:

```csharp
public async Task<PagedResponse<AdminUserResponse>> GetUsersAsync(
    AdminUserQuery query)
{
    var result = await userRepository.QueryUsersAsync(query);
}
```

แปลง `User` เป็น `AdminUserResponse` แล้วคืน response:

```csharp
return new PagedResponse<AdminUserResponse>(
    result.Items.Select(ToResponse).ToList(),
    result.Page,
    result.PageSize,
    result.TotalItems);
```

Service เป็นชั้นที่เหมาะกับการแปลง entity เป็น DTO เพราะ controller ไม่ควรรู้รายละเอียดของ database entity

## ขั้นที่ 10: ปรับ AdminUsersController

เปิด `Controllers/AdminUsersController.cs`

ตรวจว่ามี using นี้:

```csharp
using Backend.Api.Dtos.Common;
```

เปลี่ยน action `GetUsers`:

```csharp
[HttpGet]
[ProducesResponseType(typeof(PagedResponse<AdminUserResponse>),
    StatusCodes.Status200OK)]
public async Task<IActionResult> GetUsers([FromQuery] AdminUserQuery query)
{
    var users = await adminUserService.GetUsersAsync(query);

    return Ok(users);
}
```

`[FromQuery]` บอก ASP.NET Core ให้ bind ค่าจาก query string เช่น `page=1&pageSize=20` เข้า object `AdminUserQuery`

## ขั้นที่ 11: ตรวจ build

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet build
```

ถ้า build fail ให้ตรวจ 3 จุดนี้ก่อน:

- `IUserRepository` มี method `QueryUsersAsync`
- `UserRepository` มี using `Backend.Api.Dtos.Admin` และ `Backend.Api.Dtos.Common`
- `AdminUserService.GetUsersAsync` คืน `PagedResponse<AdminUserResponse>`
- ถ้าเจอ error ว่า `User` ไม่มี `NormalizedEmail` หรือ `CreatedAt` ให้ตรวจว่าใช้ `Email` และ `CreatedAtUtc` ตามบทนี้ ไม่ใช่ code จากภาค production hardening

## ขั้นที่ 12: ทดสอบด้วย Backend.Api.http

เพิ่ม request เหล่านี้ใน `Backend.Api.http`

```http
@baseUrl = http://localhost:5156
@adminUsersPath = {{baseUrl}}/api/admin/users
@adminToken = paste-admin-token-here

### Admin users page 1
GET {{adminUsersPath}}?page=1&pageSize=20
Authorization: Bearer {{adminToken}}
Accept: application/json

### Filter admin users
GET {{adminUsersPath}}?role=Admin
Authorization: Bearer {{adminToken}}
Accept: application/json

### Search by email
GET {{adminUsersPath}}?search=admin
Authorization: Bearer {{adminToken}}
Accept: application/json

### Sort by created time
GET {{adminUsersPath}}?sortBy=createdAtUtc&sortDirection=desc
Authorization: Bearer {{adminToken}}
Accept: application/json
```

ถ้าใช้ HTTPS ให้เปลี่ยน `baseUrl` เป็น port จริงของเครื่องคุณ เช่น `https://localhost:7127`

## ตัวอย่าง response

```json
{
  "items": [
    {
      "id": 1,
      "email": "admin@example.com",
      "role": "Admin",
      "isActive": true,
      "createdAtUtc": "2026-06-16T03:00:00Z",
      "updatedAtUtc": null
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalItems": 1,
  "totalPages": 1
}
```

ตัวอย่างนี้ใช้ model ของ Part 7 คือ `id` ยังเป็นตัวเลข และเวลายังเป็น `createdAtUtc`/`updatedAtUtc` ส่วน response หลังผ่านภาค production hardening อาจมี field เพิ่ม เช่น `isEmailVerified`

## Checkpoint

ก่อนอ่านภาคต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `PagedResponse<T>`
- มี `AdminUserQuery`
- `GET /api/admin/users` รับ query string ผ่าน `[FromQuery]`
- filter ด้วย `search`, `role`, `isActive` ได้
- sort ด้วย `email`, `role`, `createdAtUtc` ได้
- response มี `items`, `page`, `pageSize`, `totalItems`, `totalPages`
- admin endpoint ยังถูกป้องกันด้วย role
