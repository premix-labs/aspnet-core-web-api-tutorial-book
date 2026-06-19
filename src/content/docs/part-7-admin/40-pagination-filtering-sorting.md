---
title: 40 - Pagination, Filtering และ Sorting
description: จัดการรายการผู้ใช้จำนวนมากใน admin API อย่างควบคุมได้
---

Admin API ที่แสดงรายการผู้ใช้ไม่ควรคืนข้อมูลทั้งหมดในครั้งเดียว เพราะเมื่อข้อมูลโตขึ้น API จะช้า ใช้ memory มาก และ frontend จัดการยาก

บทนี้จะปรับ `GET /api/admin/users` ให้รองรับ pagination, filtering และ sorting

## Query ที่ต้องรองรับ

```text
GET /api/admin/users?page=1&pageSize=20&search=admin&role=Admin&isActive=true&sortBy=createdAt&sortDirection=desc
```

## สร้าง PagedResponse

สร้างโฟลเดอร์

```text
Dtos/Common/
```

สร้างไฟล์

```text
Dtos/Common/PagedResponse.cs
```

เพิ่ม code นี้

```csharp
namespace Backend.Api.Dtos.Common;

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages => (int)Math.Ceiling(TotalItems / (double)PageSize);
}
```

## สร้าง AdminUserQuery

สร้างไฟล์

```text
Dtos/Admin/AdminUserQuery.cs
```

เพิ่ม code นี้

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Admin;

public class AdminUserQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 20;

    [StringLength(256)]
    public string? Search { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public string SortBy { get; set; } = "createdAt";

    public string SortDirection { get; set; } = "desc";
}
```

จำกัด `PageSize` ไม่เกิน `100` เพื่อป้องกัน client ขอข้อมูลเยอะเกินไปใน request เดียว

## เพิ่ม QueryUsersAsync ใน Repository

เปิด `IUserRepository.cs` แล้วเพิ่ม method

```csharp
Task<PagedResponse<User>> QueryUsersAsync(AdminUserQuery query);
```

เพิ่ม using ที่จำเป็น

```csharp
using Backend.Api.Dtos.Admin;
using Backend.Api.Dtos.Common;
```

เปิด `UserRepository.cs` แล้วเพิ่ม implementation

```csharp
public async Task<PagedResponse<User>> QueryUsersAsync(AdminUserQuery query)
{
    var users = db.Users.AsNoTracking().AsQueryable();

    if (!string.IsNullOrWhiteSpace(query.Search))
    {
        var keyword = query.Search.Trim().ToUpperInvariant();

        users = users.Where(user =>
            user.Email.ToUpper().Contains(keyword));
    }

    if (!string.IsNullOrWhiteSpace(query.Role))
    {
        var role = Roles.IsValid(query.Role)
            ? Roles.Normalize(query.Role)
            : query.Role;

        users = users.Where(user => user.Role == role);
    }

    if (query.IsActive.HasValue)
    {
        users = users.Where(user => user.IsActive == query.IsActive.Value);
    }

    users = (query.SortBy.ToLowerInvariant(), query.SortDirection.ToLowerInvariant()) switch
    {
        ("email", "asc") => users.OrderBy(user => user.Email),
        ("email", "desc") => users.OrderByDescending(user => user.Email),
        ("role", "asc") => users.OrderBy(user => user.Role),
        ("role", "desc") => users.OrderByDescending(user => user.Role),
        ("createdat", "asc") => users.OrderBy(user => user.CreatedAtUtc),
        _ => users.OrderByDescending(user => user.CreatedAtUtc)
    };

    var totalItems = await users.CountAsync();

    var items = await users
        .Skip((query.Page - 1) * query.PageSize)
        .Take(query.PageSize)
        .ToListAsync();

    return new PagedResponse<User>(
        items,
        query.Page,
        query.PageSize,
        totalItems);
}
```

## ปรับ AdminUserService

เพิ่ม using

```csharp
using Backend.Api.Dtos.Common;
```

เปลี่ยน `GetUsersAsync` ให้รับ query

```csharp
public async Task<PagedResponse<AdminUserResponse>> GetUsersAsync(
    AdminUserQuery query)
{
    var result = await userRepository.QueryUsersAsync(query);

    return new PagedResponse<AdminUserResponse>(
        result.Items.Select(ToResponse).ToList(),
        result.Page,
        result.PageSize,
        result.TotalItems);
}
```

## ปรับ AdminUsersController

เปลี่ยน action `GetUsers`

```csharp
[HttpGet]
public async Task<IActionResult> GetUsers([FromQuery] AdminUserQuery query)
{
    var users = await adminUserService.GetUsersAsync(query);

    return Ok(users);
}
```

`[FromQuery]` บอกให้ ASP.NET Core bind ค่าจาก query string เช่น `page`, `pageSize`, `search`

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

## Test cases

เพิ่ม request เหล่านี้ใน `Backend.Api.http`

```http
### Admin users page 1
GET {{baseUrl}}/api/admin/users?page=1&pageSize=20
Authorization: Bearer {{adminToken}}
Accept: application/json

### Filter by role
GET {{baseUrl}}/api/admin/users?role=Admin
Authorization: Bearer {{adminToken}}
Accept: application/json

### Search by email
GET {{baseUrl}}/api/admin/users?search=admin
Authorization: Bearer {{adminToken}}
Accept: application/json

### Sort by email ascending
GET {{baseUrl}}/api/admin/users?sortBy=email&sortDirection=asc
Authorization: Bearer {{adminToken}}
Accept: application/json
```

## Checkpoint

เมื่อจบภาคนี้ คุณควรทำได้ครบตามนี้

- มี `PagedResponse<T>`
- มี `AdminUserQuery`
- `GET /api/admin/users` รองรับ `page` และ `pageSize`
- filter ด้วย `search`, `role`, `isActive` ได้
- sort ด้วย `email`, `role`, `createdAt` ได้
- response มี metadata `totalItems` และ `totalPages`
- admin endpoint ยังถูกป้องกันด้วย role
