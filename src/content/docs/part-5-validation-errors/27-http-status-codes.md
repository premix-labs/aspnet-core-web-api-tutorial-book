---
title: 27 - เลือกใช้ HTTP Status Code ให้ถูกต้อง
description: สรุปแนวทางเลือก status code สำหรับ validation, CRUD, auth และ business error
---

Status code คือภาษากลางของ HTTP ที่บอก client ว่า request สำเร็จหรือผิดพลาดแบบใด

ถ้าใช้ status code ถูก frontend, mobile app, automated test และ monitoring จะทำงานง่ายขึ้นมาก

## วิธีเรียนบทนี้

บทนี้เป็นบทสรุปภาค ให้เปิด `.http` แล้วเทียบทีละ request ว่า:

1. request สำเร็จหรือผิดพลาด
2. ถ้าผิดพลาด เป็น input validation, resource not found, conflict หรือ server error
3. response body เป็น DTO, `ProblemDetails` หรือ `ValidationProblemDetails`
4. status code ตรงกับความหมายจริงหรือไม่

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `2xx` | request สำเร็จ |
| `4xx` | client ส่ง request ที่ระบบปฏิเสธได้อย่างคาดการณ์ได้ |
| `5xx` | server มีปัญหาที่ไม่คาดคิด |
| `400` | validation หรือ request format ผิด |
| `404` | resource ที่ขอไม่มีอยู่ |
| `409` | request ชนกับ state ปัจจุบันของระบบ |
| `500` | unexpected server error |

## Status code ที่ใช้ในโปรเจกต์นี้

```text
200 OK                    อ่านหรือแก้ไขข้อมูลสำเร็จและมี body
201 Created               สร้างข้อมูลใหม่สำเร็จ
204 No Content            ทำงานสำเร็จแต่ไม่มี response body
400 Bad Request           request ผิดรูปแบบหรือ validation ไม่ผ่าน
401 Unauthorized          ยังไม่ได้ login หรือ token ไม่ถูกต้อง
403 Forbidden             login แล้วแต่ไม่มีสิทธิ์
404 Not Found             ไม่พบ resource
409 Conflict              request ชนกับ state ปัจจุบัน เช่น email ซ้ำ
500 Internal Server Error server มีปัญหาที่ไม่คาดคิด
```

## CRUD ควรตอบอะไร

| Endpoint | สถานการณ์ | Status code | Body |
| --- | --- | --- | --- |
| `GET /api/users` | อ่านรายการสำเร็จ | `200 OK` | list ของ `UserResponse` |
| `GET /api/users/{id}` | พบ user | `200 OK` | `UserResponse` |
| `GET /api/users/{id}` | ไม่พบ user | `404 Not Found` | `ProblemDetails` |
| `POST /api/users` | สร้างสำเร็จ | `201 Created` | `UserResponse` |
| `POST /api/users` | email ซ้ำ | `409 Conflict` | `ProblemDetails` |
| `PUT /api/users/{id}` | แก้ไขสำเร็จ | `200 OK` | `UserResponse` |
| `PUT /api/users/{id}` | ไม่พบ user | `404 Not Found` | `ProblemDetails` |
| `DELETE /api/users/{id}` | ลบสำเร็จ | `204 No Content` | ไม่มี body |
| `DELETE /api/users/{id}` | ไม่พบ user | `404 Not Found` | `ProblemDetails` |

## Validation ใช้ 400

ถ้า request ผิดรูปแบบ เช่น email ไม่ถูกต้อง body ว่าง หรือ field จำเป็นไม่ถูกส่งมา ให้ใช้ `400 Bad Request`

ตัวอย่าง response

```json
{
  "title": "Validation failed",
  "status": 400,
  "code": "VALIDATION_FAILED",
  "errors": {
    "Email": ["The Email field is required."]
  }
}
```

## Email ซ้ำใช้ 409

กรณี email ซ้ำไม่ใช่ request ผิด syntax เพราะ email ที่ส่งมาอาจเป็น email ที่ถูกต้องตามรูปแบบ แต่ชนกับข้อมูลที่มีอยู่แล้วในระบบ

จึงเหมาะกับ `409 Conflict`

```json
{
  "title": "Email already exists",
  "status": 409,
  "code": "EMAIL_ALREADY_EXISTS"
}
```

## 401 ต่างจาก 403 อย่างไร

`401 Unauthorized` หมายถึงยังยืนยันตัวตนไม่สำเร็จ เช่นไม่ส่ง token หรือ token หมดอายุ

`403 Forbidden` หมายถึงรู้แล้วว่าผู้ใช้คือใคร แต่ผู้ใช้นั้นไม่มีสิทธิ์ทำ action นี้ เช่น user ธรรมดาเรียก admin endpoint

เราจะใช้สอง status นี้จริงในภาค Authentication และ Admin

## 500 ใช้เมื่อไหร่

`500 Internal Server Error` ใช้กับ error ที่ server ไม่คาดคิด เช่น bug ใน code, database ล่ม หรือ dependency ภายนอกล่ม

ไม่ควรใช้ `500` กับกรณีที่คาดการณ์ได้ เช่น user not found หรือ validation failed เพราะกรณีเหล่านั้นควรมี status code ที่เฉพาะเจาะจงกว่า

## Test case ที่ควรมีหลังจบภาคนี้

เพิ่ม request เหล่านี้ลงใน `Backend.Api.http`

```http
@baseUrl = http://localhost:5156

### Validation failed
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "not-an-email"
}

### User not found
GET {{baseUrl}}/api/users/999999
Accept: application/json

### Email conflict
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "demo-user@example.com"
}
```

ให้ใช้ `baseUrl` ตาม port จริงของเครื่อง ถ้าเครื่องคุณรัน HTTPS ที่ `https://localhost:7127` และ certificate พร้อมแล้ว จะใช้ HTTPS ก็ได้

## วิธีอ่านผลลัพธ์หลังทดสอบ

หลังยิง request ให้ตรวจทั้ง status และ body:

| Test | Status ที่ควรได้ | สิ่งที่ต้องเห็นใน body |
| --- | --- | --- |
| Validation failed | `400` | `code = VALIDATION_FAILED` และ `errors.Email` |
| User not found | `404` | `code = USER_NOT_FOUND` |
| Email conflict | `409` | `code = EMAIL_ALREADY_EXISTS` |

ถ้า status ถูกแต่ body คนละ format ให้กลับไปตรวจบท 26

ถ้า body ถูกแต่ status เป็น `500` ให้กลับไปตรวจว่า service โยน `NotFoundException` หรือ `ConflictException` ไม่ใช่ exception ทั่วไป

## Checkpoint

เมื่อจบภาคนี้ คุณควรทำได้ครบตามนี้

- อธิบายได้ว่า `400`, `404`, `409`, `500` ต่างกันอย่างไร
- API ตอบ validation error เป็น `400`
- API ตอบ not found เป็น `404`
- API ตอบ email ซ้ำเป็น `409`
- API ไม่ส่ง stack trace กลับไปหา client
- error response มี `code` และ `traceId`
