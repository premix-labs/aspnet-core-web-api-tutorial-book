---
title: 08 - Routing และ Route Parameter
description: เข้าใจการจับคู่ URL กับ action method ใน Controller
---

Routing คือกลไกที่ ASP.NET Core ใช้ตัดสินว่า request หนึ่งควรถูกส่งไปที่ controller และ action method ใด

ถ้าเข้าใจ routing คุณจะแก้ปัญหา endpoint ไม่ถูกเรียก, route ซ้ำ, parameter ไม่เข้า action และ 404 จาก path ผิดได้ง่ายขึ้น

## วิธีเรียนบทนี้

บทนี้เน้นอ่าน route ให้เป็น ให้เทียบ route ในหนังสือกับ request ที่คุณยิงจริงใน `.http`

ทุกครั้งที่เห็น `{id:int}` ให้ถามตัวเองว่า:

- ค่านี้มาจาก URL ส่วนไหน
- action รับ parameter ชื่อเดียวกันหรือไม่
- ถ้าส่งค่าที่ไม่ใช่ตัวเลข route นี้ควรถูกเรียกหรือไม่

## Route ระดับ Controller

ใน `UsersController` เรากำหนด route หลักแบบนี้

```csharp
// [controller] is replaced by the controller name without the "Controller" suffix.
[Route("api/v1/[controller]")]
public class UsersController : ControllerBase
{
}
```

เมื่อ controller ชื่อ `UsersController` ค่า `[controller]` จะกลายเป็น `users` เพราะ ASP.NET Core ตัดคำว่า `Controller` ออก

ดังนั้น route หลักคือ

```text
/api/v1/users
```

## Route ระดับ Action

action แต่ละตัวสามารถเพิ่ม route ย่อยได้

```csharp
// {id:int} means this route only matches when id is an integer.
[HttpGet("{id:int}")]
public IActionResult GetUserById(int id)
{
    // ...
}
```

route เต็มของ action นี้คือ

```text
GET /api/v1/users/{id}
```

ถ้า request เป็น `GET /api/v1/users/1` ค่า `1` จะถูก bind เข้า parameter `id`

## Route Parameter

Route parameter คือค่าที่อยู่ใน path เช่น `{id}`

```csharp
// id comes from the URL path, for example /api/v1/users/1.
[HttpGet("{id}")]
public IActionResult GetUserById(int id)
```

URL:

```text
/api/v1/users/1
```

ค่า:

```text
id = 1
```

ใช้ route parameter เมื่อค่านั้นเป็นส่วนหนึ่งของ resource identity เช่น user id, order id หรือ product id

## Route Constraint

ส่วน `{id:int}` หมายถึง parameter ชื่อ `id` ต้องเป็น integer เท่านั้น

```csharp
// The :int constraint prevents non-numeric ids from matching this action.
[HttpGet("{id:int}")]
public IActionResult GetUserById(int id)
```

URL นี้จะ match:

```text
/api/v1/users/1
```

แต่ URL นี้จะไม่ match กับ action เดิม:

```text
/api/v1/users/abc
```

การใช้ route constraint ช่วยลด request ที่ผิดรูปแบบตั้งแต่ชั้น routing

Constraint ที่เจอบ่อย:

```text
{id:int}
{id:guid}
{slug:alpha}
{value:min(1)}
```

ในภาค production hardening เราจะใช้ `{id:guid}` เพราะ user id จะถูกย้ายเป็น `Guid`

## Query String

Query string เหมาะกับข้อมูลที่ใช้ค้นหา กรอง แบ่งหน้า หรือเรียงลำดับ

ตัวอย่าง URL:

```text
GET /api/v1/users?keyword=admin&page=1&pageSize=10
```

ก่อนดู code ให้รู้จัก method ที่ใช้ในตัวอย่างนี้:

| Method | ใช้ทำอะไร |
| --- | --- |
| `AsEnumerable()` | ทำให้ list ถูกใช้งานเป็น sequence สำหรับ query ต่อ |
| `Where(...)` | กรองข้อมูลตามเงื่อนไข |
| `Contains(...)` | ตรวจว่า string มีข้อความที่ต้องการหรือไม่ |
| `Skip(...)` | ข้ามข้อมูลจำนวนหนึ่ง ใช้กับ pagination |
| `Take(...)` | เลือกข้อมูลจำนวนหนึ่ง ใช้กับ pagination |
| `ToList()` | แปลงผลลัพธ์กลับเป็น list |

ตัวอย่าง action ด้านล่างเป็นตัวอย่างให้อ่านเพื่อเข้าใจ query string ก่อน ยังไม่ใช่ขั้นบังคับที่ต้องแก้ไฟล์ในบทนี้

เลือกทำได้สองแบบ:

- ถ้าต้องการให้ไฟล์ตรงกับบท 7 ต่อไป ให้แค่อ่านตัวอย่างนี้และไม่ต้องแก้ code
- ถ้าต้องการทดลอง query string จริง ให้แทน action `GetUsers()` เดิมชั่วคราวด้วย code นี้ แล้วทดสอบ URL หลัง code block

ถ้าคุณทดลองแก้ code แล้วต้องการกลับไป flow หลักของหนังสือ ให้เปลี่ยน `GetUsers()` กลับเป็น version ที่ return `Ok(Users)` แบบบท 7

```csharp
[HttpGet]
public IActionResult GetUsers(string? keyword, int page = 1, int pageSize = 10)
{
    // Start with all users, then apply optional filters.
    var query = Users.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(keyword))
    {
        // Case-insensitive email search.
        query = query.Where(user =>
            user.Email.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    // Skip previous pages and take only the current page.
    var result = query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    // Return the filtered page as JSON.
    return Ok(result);
}
```

request สำหรับทดลอง:

```text
GET http://localhost:<http-port>/api/v1/users?keyword=admin&page=1&pageSize=10
```

ให้เปลี่ยน `<http-port>` เป็น port จริงจาก `dotnet run` หรือจาก `Properties/launchSettings.json`

ผลที่ควรเห็นคือ `200 OK` และรายการ user ที่ email มีคำว่า `admin`

ถ้าไม่ได้ทดลองแก้ code ให้ข้าม request นี้ได้ บทนี้ต้องการให้เข้าใจว่า query string ถูกส่งหลังเครื่องหมาย `?` และไม่ใช่ส่วนหนึ่งของ identity ของ resource

ในภาค Admin เราจะทำ pagination, filtering และ sorting แบบจริงจังกว่านี้ด้วย DTO ชื่อ `AdminUserQuery`

## Route Parameter กับ Query String ต่างกันอย่างไร

ใช้ route parameter เมื่อค่าเป็นตัวระบุ resource โดยตรง

```text
GET /api/v1/users/1
```

ใช้ query string เมื่อค่าเป็นตัวเลือกในการค้นหา

```text
GET /api/v1/users?role=Admin&page=1&pageSize=10
```

อย่าออกแบบแบบนี้ถ้า `id` เป็นตัวระบุ resource:

```text
GET /api/v1/users?id=1
```

เขียนแบบนี้จะสื่อความหมายชัดกว่า:

```text
GET /api/v1/users/1
```

## Route ซ้ำคือปัญหา

ถ้ามี action สองตัวที่ใช้ method และ route เดียวกัน ASP.NET Core จะไม่รู้ว่าควรเรียก action ไหน

ตัวอย่างที่ควรหลีกเลี่ยง:

```csharp
// Ambiguous: both actions use the same HTTP method and route shape.
[HttpGet("{value}")]
public IActionResult GetById(int value) => Ok();

[HttpGet("{value}")]
public IActionResult GetByEmail(string value) => Ok();
```

ควรทำ route ให้ชัด:

```csharp
// Clear: numeric ids go to this action.
[HttpGet("{id:int}")]
public IActionResult GetById(int id) => Ok();

// Clear: email lookup has its own route prefix.
[HttpGet("by-email/{email}")]
public IActionResult GetByEmail(string email) => Ok();
```

## แยก 404 กับ 405 ให้ออก

เวลา test API แล้วได้ error ให้ดู status code ก่อน:

| Status code | ความหมายที่พบบ่อย |
| --- | --- |
| `404 Not Found` | ไม่มี endpoint ที่ match path นั้น หรือ action หา resource ไม่เจอ |
| `405 Method Not Allowed` | path match แล้ว แต่ HTTP method ไม่ตรงกับ action ที่มี |

ตัวอย่างเช่น ถ้ามีแค่ action นี้:

```csharp
// This route matches GET /api/v1/users/1.
[HttpGet("{id:int}")]
public IActionResult GetUserById(int id) => Ok();
```

การเรียก `GET /api/v1/users/1` จะ match แต่ `DELETE /api/v1/users/1` จะไม่ match method เดียวกัน

อีกตัวอย่างที่เจอบ่อยคือพิมพ์ route parameter ผิด:

```csharp
// Wrong: this matches the literal text "id:int".
[HttpPut("id:int")]
```

code ด้านบนไม่ได้แปลว่า id เป็น integer แต่แปลว่า route ต้องเป็นข้อความ `id:int` จริง ๆ ควรเขียนแบบนี้:

```csharp
// Correct: this captures an integer route parameter named id.
[HttpPut("{id:int}")]
```

## Checklist เวลา endpoint ไม่ถูกเรียก

ถ้า endpoint ไม่ทำงาน ให้ตรวจตามลำดับนี้:

1. HTTP method ถูกหรือไม่ เช่น `GET`, `POST`, `PUT`, `DELETE`
2. path ขึ้นต้นด้วย `/api/v1/users` หรือไม่
3. route parameter มี `{}` ครบหรือไม่ เช่น `{id:int}`
4. action อยู่ใน controller ที่ลงท้ายด้วย `Controller` หรือไม่
5. `Program.cs` มี `app.MapControllers()` หรือไม่
6. หลังแก้ code ได้ restart application แล้วหรือยัง

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรเข้าใจว่า

- `[Route("api/v1/[controller]")]` ทำงานอย่างไร
- `{id:int}` มีประโยชน์อะไร
- route parameter ต่างจาก query string อย่างไร
- ควรใช้ query string กับ search, filter และ pagination เมื่อไหร่
- route ซ้ำทำให้เกิดปัญหาอะไร
