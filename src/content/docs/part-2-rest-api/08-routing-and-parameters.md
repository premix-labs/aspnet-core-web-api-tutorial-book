---
title: 08 - Routing และ Route Parameter
description: เข้าใจการจับคู่ URL กับ action method ใน Controller
---

Routing คือกลไกที่ ASP.NET Core ใช้ตัดสินว่า request หนึ่งควรถูกส่งไปที่ controller และ action method ใด

ถ้าเข้าใจ routing คุณจะแก้ปัญหา endpoint ไม่ถูกเรียก, route ซ้ำ, parameter ไม่เข้า action และ 404 จาก path ผิดได้ง่ายขึ้น

## Route ระดับ Controller

ใน `UsersController` เรากำหนด route หลักแบบนี้

```csharp
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
}
```

เมื่อ controller ชื่อ `UsersController` ค่า `[controller]` จะกลายเป็น `users` เพราะ ASP.NET Core ตัดคำว่า `Controller` ออก

ดังนั้น route หลักคือ

```text
/api/users
```

## Route ระดับ Action

action แต่ละตัวสามารถเพิ่ม route ย่อยได้

```csharp
[HttpGet("{id:int}")]
public IActionResult GetUserById(int id)
{
    // ...
}
```

route เต็มของ action นี้คือ

```text
GET /api/users/{id}
```

ถ้า request เป็น `GET /api/users/1` ค่า `1` จะถูก bind เข้า parameter `id`

## Route Parameter

Route parameter คือค่าที่อยู่ใน path เช่น `{id}`

```csharp
[HttpGet("{id}")]
public IActionResult GetUserById(int id)
```

URL:

```text
/api/users/1
```

ค่า:

```text
id = 1
```

ใช้ route parameter เมื่อค่านั้นเป็นส่วนหนึ่งของ resource identity เช่น user id, order id หรือ product id

## Route Constraint

ส่วน `{id:int}` หมายถึง parameter ชื่อ `id` ต้องเป็น integer เท่านั้น

```csharp
[HttpGet("{id:int}")]
public IActionResult GetUserById(int id)
```

URL นี้จะ match:

```text
/api/users/1
```

แต่ URL นี้จะไม่ match กับ action เดิม:

```text
/api/users/abc
```

การใช้ route constraint ช่วยลด request ที่ผิดรูปแบบตั้งแต่ชั้น routing

Constraint ที่เจอบ่อย:

```text
{id:int}
{id:guid}
{slug:alpha}
{value:min(1)}
```

ใน final project เราจะใช้ `{id:guid}` เพราะ user id เป็น `Guid`

## Query String

Query string เหมาะกับข้อมูลที่ใช้ค้นหา กรอง แบ่งหน้า หรือเรียงลำดับ

ตัวอย่าง URL:

```text
GET /api/users?keyword=admin&page=1&pageSize=10
```

ตัวอย่าง action:

```csharp
[HttpGet]
public IActionResult GetUsers(string? keyword, int page = 1, int pageSize = 10)
{
    var query = Users.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(keyword))
    {
        query = query.Where(user =>
            user.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    var result = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return Ok(result);
}
```

ในภาค Admin เราจะทำ pagination, filtering และ sorting แบบจริงจังกว่านี้ด้วย DTO ชื่อ `AdminUserQuery`

## Route Parameter กับ Query String ต่างกันอย่างไร

ใช้ route parameter เมื่อค่าเป็นตัวระบุ resource โดยตรง

```text
GET /api/users/1
```

ใช้ query string เมื่อค่าเป็นตัวเลือกในการค้นหา

```text
GET /api/users?role=Admin&page=1&pageSize=10
```

อย่าออกแบบแบบนี้ถ้า `id` เป็นตัวระบุ resource:

```text
GET /api/users?id=1
```

เขียนแบบนี้จะสื่อความหมายชัดกว่า:

```text
GET /api/users/1
```

## Route ซ้ำคือปัญหา

ถ้ามี action สองตัวที่ใช้ method และ route เดียวกัน ASP.NET Core จะไม่รู้ว่าควรเรียก action ไหน

ตัวอย่างที่ควรหลีกเลี่ยง:

```csharp
[HttpGet("{value}")]
public IActionResult GetById(int value) => Ok();

[HttpGet("{value}")]
public IActionResult GetByEmail(string value) => Ok();
```

ควรทำ route ให้ชัด:

```csharp
[HttpGet("{id:int}")]
public IActionResult GetById(int id) => Ok();

[HttpGet("by-email/{email}")]
public IActionResult GetByEmail(string email) => Ok();
```

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรเข้าใจว่า

- `[Route("api/[controller]")]` ทำงานอย่างไร
- `{id:int}` มีประโยชน์อะไร
- route parameter ต่างจาก query string อย่างไร
- ควรใช้ query string กับ search, filter และ pagination เมื่อไหร่
- route ซ้ำทำให้เกิดปัญหาอะไร
