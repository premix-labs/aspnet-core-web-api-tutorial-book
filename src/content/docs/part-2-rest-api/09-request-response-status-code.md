---
title: 09 - Request, Response และ Status Code
description: เลือกชนิด response และ HTTP status code ให้เหมาะกับผลลัพธ์ของ API
---

API ที่ดีไม่ใช่แค่ส่งข้อมูลกลับได้ แต่ต้องสื่อสารผลลัพธ์ให้ client เข้าใจด้วย status code และ response body ที่เหมาะสม

บทนี้จะจัดระเบียบสิ่งที่เราใช้ใน CRUD API ให้ชัดขึ้น ก่อนเข้าสู่การทดสอบด้วย REST Client

## รับข้อมูลจาก Request Body

เวลาสร้างหรือแก้ไขข้อมูล client มักส่ง JSON มาใน request body

```json
{
  "email": "new-user@example.com"
}
```

ASP.NET Core สามารถ bind JSON นี้เข้า C# object ได้

```csharp
public record CreateUserRequest(string Email);

[HttpPost]
public IActionResult CreateUser(CreateUserRequest request)
{
    return Ok(request.Email);
}
```

ในตัวอย่างนี้ ASP.NET Core อ่าน JSON body แล้วสร้าง `CreateUserRequest` ให้เรา

## Binding มาจากไหนบ้าง

ข้อมูลใน request อาจมาจากหลายที่

```text
Route        /api/users/{id}
Query string /api/users?page=1&pageSize=10
Body         JSON ที่ส่งมากับ POST/PUT/PATCH
Header       Authorization, Content-Type
```

ตัวอย่าง:

```csharp
[HttpPut("{id:int}")]
public IActionResult UpdateUser(int id, UpdateUserRequest request)
```

`id` มาจาก route

`request` มาจาก body

## ส่ง Response ด้วย helper method

`ControllerBase` มี helper method สำหรับสร้าง response หลายแบบ

```text
Ok(value)                 200 OK
CreatedAtAction(...)      201 Created
NoContent()               204 No Content
BadRequest(value)         400 Bad Request
Unauthorized()            401 Unauthorized
Forbid()                  403 Forbidden
NotFound()                404 Not Found
Conflict(value)           409 Conflict
```

ตัวอย่าง:

```csharp
if (user is null)
{
    return NotFound();
}

return Ok(user);
```

## เลือก Status Code ให้ถูก

ตัวอย่างการเลือก status code ใน CRUD API

```text
GET รายการสำเร็จ        -> 200 OK
GET รายการเดียวสำเร็จ   -> 200 OK
GET รายการเดียวไม่พบ    -> 404 Not Found
POST สร้างสำเร็จ        -> 201 Created
PUT แก้ไขสำเร็จ         -> 200 OK หรือ 204 No Content
PUT หา record ไม่เจอ     -> 404 Not Found
DELETE ลบสำเร็จ         -> 204 No Content
DELETE หา record ไม่เจอ  -> 404 Not Found
Request ผิดรูปแบบ       -> 400 Bad Request
ข้อมูลซ้ำหรือชน rule     -> 409 Conflict
```

เลือก status code ให้ตรงกับสิ่งที่เกิดขึ้นจริง เพื่อให้ client ไม่ต้องเดาจากข้อความใน body

## อย่าตอบ 200 ทุกกรณี

มือใหม่มักตอบ `200 OK` แม้เกิด error เช่น ไม่พบข้อมูล หรือ validation ไม่ผ่าน

ตัวอย่างที่ไม่ควรทำ:

```json
{
  "success": false,
  "message": "User not found"
}
```

ถ้าตอบ body แบบนี้พร้อม status `200 OK` client และระบบ monitoring จะเข้าใจว่า request สำเร็จ ทั้งที่จริงควรเป็น `404 Not Found`

ควรใช้ status code ให้ตรงกับผลลัพธ์:

```csharp
return NotFound();
```

## CreatedAtAction สำคัญอย่างไร

ตอนสร้างข้อมูลใหม่ เราใช้:

```csharp
return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
```

ผลที่ได้คือ:

- status code `201 Created`
- response body เป็น user ที่สร้าง
- header `Location` ชี้ไปยัง endpoint สำหรับอ่าน user นั้น

แนวทางนี้ช่วยให้ client รู้ว่าหลังสร้างแล้วควรไปอ่าน resource ใหม่จากไหน

## Response Body ควรสม่ำเสมอ

ในบทแรก ๆ เราจะตอบ body แบบง่ายก่อน แต่ในภาค Error Handling จะออกแบบ response format ให้สม่ำเสมอด้วย `ProblemDetails`

ตัวอย่าง error response ที่อ่านง่าย:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "User was not found.",
  "instance": "/api/users/999"
}
```

ตอนนี้ให้จำหลักก่อนว่า success response และ error response ควรมีรูปแบบที่ client เข้าใจได้

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรตอบได้ว่า

- ข้อมูลจาก route, query string และ body ต่างกันอย่างไร
- สร้างข้อมูลสำเร็จควรตอบ status code อะไร
- ลบข้อมูลสำเร็จควรตอบ body หรือไม่
- ทำไมไม่ควรตอบ `200 OK` ทุกกรณี
- `CreatedAtAction` ให้ประโยชน์อะไร
