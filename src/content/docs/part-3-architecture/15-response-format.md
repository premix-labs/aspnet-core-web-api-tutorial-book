---
title: 15 - Response Format ที่อ่านง่ายและดูแลต่อได้
description: วางรูปแบบ response ให้ API สื่อสารกับ client ได้สม่ำเสมอ
---

API ที่ดีควรมี response format ที่คาดเดาได้ โดยเฉพาะ error response เพราะ frontend ต้องใช้ข้อมูลเหล่านี้แสดงข้อความหรือจัดการ flow ต่อ

บทนี้จะวางหลักก่อนเข้าสู่ภาค database และภาค validation/error handling

## Success response ควรเรียบง่าย

สำหรับ response ที่สำเร็จ เราสามารถส่ง DTO กลับตรง ๆ ได้

```json
{
  "id": 1,
  "email": "admin@example.com"
}
```

สำหรับรายการ:

```json
[
  {
    "id": 1,
    "email": "admin@example.com"
  },
  {
    "id": 2,
    "email": "user@example.com"
  }
]
```

ไม่จำเป็นต้องห่อทุก response ด้วย `{ "success": true, "data": ... }` เสมอไป ถ้าทีมไม่ได้ต้องการรูปแบบนั้นจริง ๆ

## Error response ต้องสม่ำเสมอ

สิ่งที่ควรสม่ำเสมอมากที่สุดคือ error response เพราะ client ต้องแยกกรณี error และแสดงข้อความให้ผู้ใช้

ASP.NET Core รองรับรูปแบบมาตรฐานชื่อ `ProblemDetails`

ตัวอย่าง:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "User was not found.",
  "instance": "/api/users/999"
}
```

ในภาค Validation และ Error Handling เราจะใช้แนวทางนี้กับ exception handler กลาง

## Validation error

กรณี validation ไม่ผ่าน อาจมีหลาย field ที่ผิดพร้อมกัน

ตัวอย่าง:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "The Email field is required."
    ],
    "Password": [
      "The field Password must be a string with a minimum length of 8."
    ]
  }
}
```

รูปแบบนี้ช่วย frontend รู้ว่า field ไหนผิด และแสดงข้อความใต้ input ได้

## Pagination response

สำหรับ endpoint ที่มี pagination ไม่ควรส่ง array อย่างเดียว เพราะ client ต้องรู้จำนวนข้อมูลทั้งหมดและหน้าปัจจุบัน

ตัวอย่าง response:

```json
{
  "items": [
    {
      "id": "4d523c5c-8f42-4cf2-93f2-bcf52de9c111",
      "email": "admin@example.com",
      "role": "Admin",
      "isActive": true
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 25,
  "totalPages": 3
}
```

ในภาค Admin เราจะสร้าง `PagedResponse<T>` เพื่อใช้กับรายการ user

## อย่าใช้ response format เพื่อซ่อน status code

ตัวอย่างที่ควรหลีกเลี่ยง:

```json
{
  "success": false,
  "message": "User not found"
}
```

ถ้าส่ง response นี้พร้อม status `200 OK` จะทำให้ client และ monitoring เข้าใจว่า request สำเร็จ

ควรใช้:

```text
404 Not Found
```

พร้อม error body ที่อธิบายปัญหา

## Response ควรไม่เผยข้อมูลลับ

ห้ามส่งข้อมูลเหล่านี้ออก API:

- password
- password hash
- secret key
- connection string
- internal token
- stack trace ใน production
- field ภายในที่ client ไม่จำเป็นต้องรู้

การใช้ DTO และ mapping ช่วยป้องกันปัญหานี้

## แนวทางของหนังสือนี้

หนังสือนี้ใช้แนวทางดังนี้:

- success response ส่ง DTO ตรง ๆ
- create สำเร็จใช้ `201 Created`
- delete สำเร็จใช้ `204 No Content`
- validation error ใช้ `ValidationProblemDetails`
- exception/error ใช้ `ProblemDetails`
- paginated list ใช้ `PagedResponse<T>`

แนวทางนี้ตรงกับ final project ในท้ายเล่ม

## Checkpoint

ก่อนเข้าสู่ภาค database คุณควรเข้าใจว่า

- Success response กับ error response อาจมีรูปแบบต่างกันได้
- ทำไม error response ควรสม่ำเสมอ
- `ProblemDetails` ใช้เพื่ออะไร
- Pagination response ควรมี metadata อะไรบ้าง
- ทำไมไม่ควรส่ง `PasswordHash` หรือ stack trace ออก API
