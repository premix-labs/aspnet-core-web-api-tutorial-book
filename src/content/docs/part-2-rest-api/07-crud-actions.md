---
title: 07 - ทำ GET, POST, PUT, DELETE
description: สร้าง CRUD API พื้นฐานด้วย Controller
---

บทนี้เราจะเพิ่ม endpoint CRUD ให้ `UsersController` โดยใช้ list ใน memory เพื่อให้เห็นหลักการก่อนเชื่อมต่อ database ในภาคถัดไป

CRUD ย่อมาจาก:

```text
Create  สร้างข้อมูล
Read    อ่านข้อมูล
Update  แก้ไขข้อมูล
Delete  ลบข้อมูล
```

## ใช้ข้อมูลใน memory ก่อน

ช่วงนี้ยังไม่เชื่อม database เราจะใช้ `static List<T>` เก็บข้อมูลชั่วคราว

ข้อควรรู้:

- ข้อมูลจะอยู่ระหว่างที่ application ยังรัน
- ถ้า restart server ข้อมูลจะกลับไปเป็นค่าเริ่มต้น
- วิธีนี้เหมาะสำหรับเรียน Controller แต่ไม่เหมาะกับ production
- ภาค database จะเปลี่ยนไปใช้ EF Core และ SQL Server

## แก้ UsersController เป็นไฟล์เต็ม

เปิดไฟล์นี้

```text
Controllers/UsersController.cs
```

แทนที่ code ด้วยไฟล์เต็มนี้

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

public record UserDto(int Id, string Email);

public record CreateUserRequest(string Email);

public record UpdateUserRequest(string Email);

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private static readonly List<UserDto> Users =
    [
        new(1, "admin@example.com"),
        new(2, "user@example.com")
    ];

    [HttpGet]
    public IActionResult GetUsers()
    {
        return Ok(Users);
    }

    [HttpGet("{id:int}")]
    public IActionResult GetUserById(int id)
    {
        var user = Users.FirstOrDefault(user => user.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpPost]
    public IActionResult CreateUser(CreateUserRequest request)
    {
        var nextId = Users.Count == 0 ? 1 : Users.Max(user => user.Id) + 1;
        var user = new UserDto(nextId, request.Email);

        Users.Add(user);

        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPut("{id:int}")]
    public IActionResult UpdateUser(int id, UpdateUserRequest request)
    {
        var index = Users.FindIndex(user => user.Id == id);

        if (index == -1)
        {
            return NotFound();
        }

        var updatedUser = new UserDto(id, request.Email);
        Users[index] = updatedUser;

        return Ok(updatedUser);
    }

    [HttpDelete("{id:int}")]
    public IActionResult DeleteUser(int id)
    {
        var user = Users.FirstOrDefault(user => user.Id == id);

        if (user is null)
        {
            return NotFound();
        }

        Users.Remove(user);

        return NoContent();
    }
}
```

## DTO ชั่วคราวในบทนี้

ในไฟล์นี้เราสร้าง record 3 ตัว:

```csharp
public record UserDto(int Id, string Email);
public record CreateUserRequest(string Email);
public record UpdateUserRequest(string Email);
```

`UserDto` ใช้เป็น response

`CreateUserRequest` ใช้รับข้อมูลตอนสร้าง user

`UpdateUserRequest` ใช้รับข้อมูลตอนแก้ไข user

ตอนนี้เราวางไว้ไฟล์เดียวกันเพื่อให้เรียนง่ายก่อน ในภาค Architecture เราจะแยก DTO ไปไว้โฟลเดอร์ `Dtos`

## GET รายการทั้งหมด

```csharp
[HttpGet]
public IActionResult GetUsers()
{
    return Ok(Users);
}
```

endpoint:

```text
GET /api/users
```

ผลลัพธ์ที่คาดหวังคือ `200 OK` พร้อม array ของ user

## GET รายการเดียวด้วย id

```csharp
[HttpGet("{id:int}")]
public IActionResult GetUserById(int id)
```

endpoint:

```text
GET /api/users/1
```

ถ้าไม่พบข้อมูลให้ตอบ `404 Not Found` แทนการตอบ `200 OK` พร้อมค่า `null`

## POST สร้างข้อมูล

```csharp
[HttpPost]
public IActionResult CreateUser(CreateUserRequest request)
```

endpoint:

```text
POST /api/users
```

body:

```json
{
  "email": "new-user@example.com"
}
```

เมื่อสร้างข้อมูลสำเร็จควรตอบ `201 Created`

`CreatedAtAction` ช่วยสร้าง response ที่บอกว่า resource ใหม่สามารถอ่านได้จาก action ไหน

## PUT แก้ไขข้อมูล

```csharp
[HttpPut("{id:int}")]
public IActionResult UpdateUser(int id, UpdateUserRequest request)
```

endpoint:

```text
PUT /api/users/1
```

body:

```json
{
  "email": "updated-user@example.com"
}
```

ถ้า id มีอยู่ จะตอบ `200 OK` พร้อมข้อมูลที่แก้แล้ว ถ้าไม่มีจะตอบ `404 Not Found`

## DELETE ลบข้อมูล

```csharp
[HttpDelete("{id:int}")]
public IActionResult DeleteUser(int id)
```

endpoint:

```text
DELETE /api/users/1
```

เมื่อลบสำเร็จนิยมตอบ `204 No Content` เพราะไม่จำเป็นต้องส่ง body กลับ

## ลำดับการทดสอบที่แนะนำ

ให้รัน API แล้วทดสอบตามลำดับนี้

```text
GET    /api/users
GET    /api/users/1
POST   /api/users
GET    /api/users
PUT    /api/users/1
GET    /api/users/1
DELETE /api/users/1
GET    /api/users/1
```

การทดสอบตามลำดับนี้ทำให้เห็นว่าข้อมูลใน memory เปลี่ยนจริง

## Checkpoint

เมื่อจบบทนี้ คุณควรมี endpoint เหล่านี้

```text
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
```

และควรอธิบายได้ว่า `200`, `201`, `204` และ `404` ถูกใช้ในกรณีใด
