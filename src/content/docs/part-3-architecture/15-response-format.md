---
title: 15 - Response Format ที่อ่านง่ายและดูแลต่อได้
description: วางรูปแบบ response ให้ API สื่อสารกับ client ได้สม่ำเสมอ
---

API ที่ดีควรมี response format ที่คาดเดาได้ โดยเฉพาะ error response เพราะ frontend ต้องใช้ข้อมูลเหล่านี้แสดงข้อความหรือจัดการ flow ต่อ

บทนี้จะวางหลักก่อนเข้าสู่ภาค database และภาค validation/error handling

บทนี้ยังไม่เน้นเขียน code ใหม่ เป้าหมายคือให้อ่าน response เป็น และรู้ว่าตอนไหนควรใช้ status code หรือ body แบบไหน ก่อนที่บทหลังจะเริ่มสร้าง validation และ exception handler จริง

## วิธีเรียนบทนี้

บทนี้เป็นบทจัดระเบียบความคิด ให้เปิด `.http` หรือ response ที่เคยทดสอบในบทก่อน แล้วเทียบว่าแต่ละ endpoint ควรตอบรูปแบบไหน

ยังไม่ต้องสร้าง wrapper response เองทุก endpoint เพราะภาค validation/error handling จะเริ่มจัด response error ให้เป็นระบบมากขึ้น

## สิ่งที่ต้องตัดสินใจเมื่อออกแบบ response

ก่อน return response ให้ถามตัวเองก่อนว่า request นี้อยู่ในกรณีไหน:

| กรณี | รูปแบบที่ใช้ |
| --- | --- |
| อ่านหรือแก้ไขสำเร็จและมีข้อมูล | ส่ง DTO ตรง ๆ |
| สร้าง resource สำเร็จ | `201 Created` พร้อม DTO และ `Location` header |
| ลบสำเร็จ | `204 No Content` |
| validation ไม่ผ่าน | `ValidationProblemDetails` |
| error ทั่วไปหรือ resource ไม่พบ | `ProblemDetails` |
| รายการที่แบ่งหน้า | `PagedResponse<T>` |

เป้าหมายคือ client คาดเดาได้ว่าเมื่อสำเร็จจะได้ข้อมูลแบบไหน และเมื่อผิดพลาดจะได้ error format แบบไหน

## วิธีเลือก response แบบเป็นขั้นตอน

เวลาออกแบบ endpoint ให้ไล่ถามตามลำดับนี้:

```text
1. Request สำเร็จหรือผิดพลาด
2. ถ้าสำเร็จ มีข้อมูลต้องส่งกลับไหม
3. ถ้าสร้าง resource ใหม่ ต้องใช้ 201 Created หรือไม่
4. ถ้าลบสำเร็จ ต้องมี body หรือไม่
5. ถ้าผิดพลาด เป็น validation error หรือ error ทั่วไป
6. ถ้าเป็นรายการ ต้องมี pagination metadata หรือไม่
```

ตัวอย่างการตัดสินใจ:

```text
DELETE /api/users/1
  -> สำเร็จ
  -> ไม่มีข้อมูลต้องส่งกลับ
  -> ใช้ 204 No Content
```

```text
POST /api/users
  -> สำเร็จ
  -> สร้าง resource ใหม่
  -> ใช้ 201 Created พร้อม UserResponse
```

```text
GET /api/users/999
  -> ผิดพลาด
  -> ไม่ใช่ validation error
  -> ใช้ 404 Not Found พร้อม ProblemDetails
```

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

สิ่งที่ต้องดูใน success response:

- body ควรเป็น DTO ไม่ใช่ entity ภายใน
- field ควรมีเท่าที่ client ต้องใช้
- status code ต้องตรงกับสิ่งที่เกิดขึ้น เช่น `200 OK`, `201 Created`, `204 No Content`

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

อ่าน field ของ `ProblemDetails` แบบง่าย:

| Field | ความหมาย |
| --- | --- |
| `type` | URL หรือรหัสอ้างอิงชนิดของปัญหา |
| `title` | ชื่อสั้น ๆ ของ error |
| `status` | HTTP status code |
| `detail` | รายละเอียดที่อ่านเข้าใจได้ |
| `instance` | path ของ request ที่เกิดปัญหา |

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

อ่าน field สำคัญ:

| Field | ความหมาย |
| --- | --- |
| `title` | บอกว่ามี validation error |
| `status` | ปกติเป็น `400` |
| `errors` | object ที่แยก error ตามชื่อ field |
| `errors.Email` | list ข้อความ error ของ field `Email` |

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

อ่าน field ของ pagination:

| Field | ความหมาย |
| --- | --- |
| `items` | รายการข้อมูลในหน้าปัจจุบัน |
| `page` | เลขหน้าปัจจุบัน |
| `pageSize` | จำนวนรายการต่อหน้า |
| `totalItems` | จำนวนข้อมูลทั้งหมดที่ match query |
| `totalPages` | จำนวนหน้าทั้งหมด |

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

แนวทางนี้จะใช้ต่อเนื่องกับบทท้ายเล่ม

## ตัวอย่างการเลือก response

ใช้ตารางนี้เป็นแนวทาง เวลาอ่านให้ดูจากซ้ายไปขวา: endpoint คืออะไร, สถานการณ์คืออะไร, status code ควรเป็นอะไร, body ควรเป็น DTO หรือ error format แบบไหน

| Endpoint | สถานการณ์ | Status code | Body |
| --- | --- | --- | --- |
| `GET /api/users/1` | พบ user | `200 OK` | `UserResponse` |
| `GET /api/users/999` | ไม่พบ user | `404 Not Found` | `ProblemDetails` |
| `POST /api/users` | สร้างสำเร็จ | `201 Created` | `UserResponse` |
| `DELETE /api/users/1` | ลบสำเร็จ | `204 No Content` | ไม่มี body |
| `POST /api/users` | email ไม่ถูกต้อง | `400 Bad Request` | `ValidationProblemDetails` |
| `GET /api/admin/users?page=1` | อ่านรายการแบบแบ่งหน้า | `200 OK` | `PagedResponse<AdminUserResponse>` |

## Checkpoint สำหรับออกแบบ endpoint ใหม่

เมื่อสร้าง endpoint ใหม่ ให้ตอบคำถามเหล่านี้ก่อนเขียน code:

- ถ้าสำเร็จจะใช้ status code อะไร
- ถ้าสำเร็จจะมี response body หรือไม่
- response body เป็น DTO ตัวไหน
- ถ้า resource ไม่พบจะตอบอะไร
- ถ้า request ไม่ถูกต้องจะใช้ validation response แบบไหน
- มีข้อมูลลับหรือ field ภายในหลุดออก response หรือไม่

## แบบฝึกหัด

ลองตอบว่ากรณีต่อไปนี้ควรใช้ response format แบบใด:

1. สร้าง user สำเร็จ
2. ส่ง email ว่างมาตอนสร้าง user
3. อ่าน user id ที่ไม่มีอยู่
4. ลบ user สำเร็จ
5. อ่านรายการ user แบบแบ่งหน้า

## แนวคำตอบโดยย่อ

- สร้าง user สำเร็จ: `201 Created` พร้อม response DTO
- email ว่าง: `400 Bad Request` พร้อม `ValidationProblemDetails`
- user ไม่มีอยู่: `404 Not Found` พร้อม `ProblemDetails`
- ลบสำเร็จ: `204 No Content`
- รายการแบบแบ่งหน้า: `200 OK` พร้อม `PagedResponse<T>`

## Checkpoint

ก่อนเข้าสู่ภาค database คุณควรเข้าใจว่า

- Success response กับ error response อาจมีรูปแบบต่างกันได้
- ทำไม error response ควรสม่ำเสมอ
- `ProblemDetails` ใช้เพื่ออะไร
- Pagination response ควรมี metadata อะไรบ้าง
- ทำไมไม่ควรส่ง `PasswordHash` หรือ stack trace ออก API
