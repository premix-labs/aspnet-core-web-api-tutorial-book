---
title: 27 - เลือกใช้ HTTP Status Code ให้ถูกต้อง
description: สรุปแนวทางเลือก status code สำหรับ validation, CRUD, auth และ business error
---

Status code คือภาษากลางของ HTTP ที่บอก client ว่า request สำเร็จหรือผิดพลาดแบบใด

ถ้าใช้ status code ถูก frontend, mobile app, automated test และ monitoring จะทำงานง่ายขึ้นมาก

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

```text
GET /api/users             -> 200 OK
GET /api/users/{id} พบ     -> 200 OK
GET /api/users/{id} ไม่พบ  -> 404 Not Found
POST /api/users สำเร็จ     -> 201 Created
POST /api/users email ซ้ำ  -> 409 Conflict
PUT /api/users/{id} สำเร็จ -> 200 OK
PUT /api/users/{id} ไม่พบ  -> 404 Not Found
DELETE /api/users/{id} สำเร็จ -> 204 No Content
DELETE /api/users/{id} ไม่พบ  -> 404 Not Found
```

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

## Checkpoint

เมื่อจบภาคนี้ คุณควรทำได้ครบตามนี้

- อธิบายได้ว่า `400`, `404`, `409`, `500` ต่างกันอย่างไร
- API ตอบ validation error เป็น `400`
- API ตอบ not found เป็น `404`
- API ตอบ email ซ้ำเป็น `409`
- API ไม่ส่ง stack trace กลับไปหา client
- error response มี `code` และ `traceId`
