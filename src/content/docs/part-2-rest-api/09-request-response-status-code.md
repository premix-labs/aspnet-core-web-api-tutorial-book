---
title: 09 - Request, Response และ Status Code
description: เลือกชนิด response และ HTTP status code ให้เหมาะกับผลลัพธ์ของ API
---

API ที่ดีไม่ใช่แค่ส่งข้อมูลกลับได้ แต่ต้องสื่อสารผลลัพธ์ให้ client เข้าใจด้วย status code และ response body ที่เหมาะสม

บทนี้จะจัดระเบียบสิ่งที่เราใช้ใน CRUD API ให้ชัดขึ้น ก่อนเข้าสู่การทดสอบด้วย REST Client

## วิธีเรียนบทนี้

บทนี้ไม่ใช่การจำ status code ทั้งหมด ให้จำว่าแต่ละ action ควรสื่อสารผลลัพธ์อะไร:

- อ่านสำเร็จ ใช้ `200 OK`
- สร้างสำเร็จ ใช้ `201 Created`
- ลบสำเร็จและไม่มี body ใช้ `204 No Content`
- หาไม่เจอ ใช้ `404 Not Found`
- request body ผิด ใช้ `400 Bad Request`

## รับข้อมูลจาก Request Body

เวลาสร้างหรือแก้ไขข้อมูล client มักส่ง JSON มาใน request body

```json
{
  "email": "new-user@example.com"
}
```

ASP.NET Core สามารถ bind JSON นี้เข้า C# object ได้

```csharp
// DTO used to bind the JSON request body.
public record CreateUserRequest(string Email);

[HttpPost]
public IActionResult CreateUser(CreateUserRequest request)
{
    // request.Email now contains the value from the JSON body.
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
// id is bound from the route, request is bound from the JSON body.
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
    // Missing resources should be translated to 404.
    return NotFound();
}

// Existing resources are returned with 200 OK.
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
// Correct status for a missing resource.
return NotFound();
```

## CreatedAtAction สำคัญอย่างไร

ตอนสร้างข้อมูลใหม่ เราใช้:

```csharp
// Creates a 201 response and a Location header pointing to GetUserById.
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

## เลือก Response แบบเป็นขั้นตอน

เวลาจะ return response จาก action ให้ถามตัวเองตามลำดับนี้:

1. request นี้สำเร็จหรือไม่
2. ถ้าไม่สำเร็จ เป็นเพราะ client ส่งข้อมูลผิด, ยังไม่ได้ login, ไม่มีสิทธิ์, ไม่พบข้อมูล หรือข้อมูลชน rule
3. ถ้าสำเร็จ มี body ที่ต้องส่งกลับหรือไม่
4. ถ้าเป็นการสร้าง resource ใหม่ ควรมี `Location` header หรือไม่

ตัวอย่างการตัดสินใจ:

| สถานการณ์ | Response ที่เหมาะสม |
| --- | --- |
| อ่านรายการ user สำเร็จ | `Ok(users)` |
| หา user ตาม id ไม่เจอ | `NotFound()` |
| สร้าง user สำเร็จ | `CreatedAtAction(...)` |
| ลบ user สำเร็จ | `NoContent()` |
| request body ผิดรูปแบบ | `BadRequest(...)` หรือ validation response อัตโนมัติ |
| email ซ้ำ | `Conflict(...)` |

## ตรวจ Response จาก REST Client

เวลา test API อย่าดูเฉพาะ body ให้ดู 3 จุดพร้อมกัน:

- status code เช่น `200`, `201`, `204`, `404`
- response header เช่น `Content-Type` หรือ `Location`
- response body เช่น JSON ที่ API ส่งกลับ

ตัวอย่างเช่นหลัง `POST /api/users` สำเร็จ คุณควรเห็น `201 Created` และควรมีข้อมูล user ที่สร้างกลับมา

หลัง `DELETE /api/users/1` สำเร็จ คุณควรเห็น `204 No Content` และไม่ควรคาดหวัง JSON body กลับมา

## แบบฝึกหัด

ลองตอบว่าแต่ละกรณีควรใช้ status code อะไร:

1. `GET /api/users/999` แต่ไม่มี user id `999`
2. `POST /api/users` แล้วสร้าง user สำเร็จ
3. `DELETE /api/users/1` แล้วลบสำเร็จ
4. `PUT /api/users/999` แต่ไม่มี user id `999`
5. request body เป็น JSON ผิดรูปแบบ

## แนวคำตอบโดยย่อ

- ไม่พบ user: `404 Not Found`
- สร้างสำเร็จ: `201 Created`
- ลบสำเร็จ: `204 No Content`
- แก้ไข record ที่ไม่มี: `404 Not Found`
- JSON ผิดรูปแบบ: `400 Bad Request`

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรตอบได้ว่า

- ข้อมูลจาก route, query string และ body ต่างกันอย่างไร
- สร้างข้อมูลสำเร็จควรตอบ status code อะไร
- ลบข้อมูลสำเร็จควรตอบ body หรือไม่
- ทำไมไม่ควรตอบ `200 OK` ทุกกรณี
- `CreatedAtAction` ให้ประโยชน์อะไร
