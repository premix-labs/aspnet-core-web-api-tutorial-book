---
title: 10 - ทดสอบ API ด้วย REST Client หรือ Postman
description: ทดสอบ endpoint ด้วยเครื่องมือภายนอกเพื่อดู request และ response จริง
---

หลังสร้าง endpoint แล้วต้องทดสอบด้วยเครื่องมือที่ส่ง HTTP request ได้จริง ไม่ควรดูแค่ code แล้วสรุปว่า API ทำงานถูกต้อง

บทนี้ใช้ REST Client ใน Visual Studio Code เป็นหลัก เพราะเก็บ request ไว้ใน repository ได้ง่าย แต่ถ้าคุณถนัด Postman ก็ใช้หลักการเดียวกัน

## วิธีเรียนบทนี้

บทนี้ให้ทดสอบ endpoint ตามลำดับ อย่ากด request สุ่ม เพราะข้อมูลยังอยู่ใน memory และเปลี่ยนตาม request ที่ยิงไป

ถ้าผลลัพธ์ไม่ตรง ให้ดูสามอย่างก่อน:

1. API ยังรันอยู่หรือไม่
2. `baseUrl` ใช้ port ตรงกับ terminal หรือไม่
3. ก่อนหน้านี้คุณลบหรือแก้ user ไปแล้วหรือยัง

## เตรียม API ให้รันอยู่

เปิด terminal ที่โฟลเดอร์โปรเจกต์ `Backend.Api`

```powershell
dotnet run
```

ดู URL ที่ terminal แสดง เช่น

```text
http://localhost:<http-port>
https://localhost:<https-port>
```

`<http-port>` และ `<https-port>` คือเลข port จริงจากเครื่องของคุณ ไม่ใช่ค่าที่ต้องพิมพ์ตามตัวอย่าง

ถ้าเริ่มต้นด้วย REST Client แล้วเจอปัญหา certificate กับ HTTPS ให้ใช้ URL แบบ `http` ก่อน เช่น `http://localhost:<http-port>`

## เตรียมไฟล์ Backend.Api.http

ตอนสร้าง project ด้วย template ของ ASP.NET Core จะมีไฟล์ `.http` มาให้แล้ว ชื่อไฟล์มักเป็นชื่อเดียวกับ project

```text
Backend.Api.http
```

ให้เปิดไฟล์นี้แล้วแทน request ตัวอย่างของ template ด้วย request ในบทนี้

ถ้า project ของคุณไม่มีไฟล์นี้ ให้สร้างไฟล์ชื่อ `Backend.Api.http` เป็นชื่อหลักตามหนังสือ ถ้าคุณเลือกใช้ `requests.http` เองก็ได้ แต่ให้ใช้ชื่อนั้นให้ต่อเนื่องทั้งโปรเจกต์

ให้รันคำสั่งจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
if (-not (Test-Path -LiteralPath Backend.Api.http)) {
    New-Item -ItemType File -Path Backend.Api.http
}
```

macOS/Linux Bash:

```bash
touch Backend.Api.http
```

## สิ่งที่จะใช้ในไฟล์ .http

ก่อนเขียน request ให้รู้จัก syntax พื้นฐานของไฟล์ `.http`:

| Syntax | ความหมาย |
| --- | --- |
| `@baseUrl = ...` | ประกาศตัวแปรชื่อ `baseUrl` เพื่อเก็บ URL หลักของ API |
| `{{baseUrl}}` | เรียกใช้ค่าจากตัวแปร `baseUrl` |
| `###` | แยก request แต่ละชุดออกจากกัน |
| `GET`, `POST`, `PUT`, `DELETE` | HTTP method ที่จะส่งไปหา API |
| `Accept: application/json` | บอก API ว่า client ต้องการรับ JSON |
| `Content-Type: application/json` | บอก API ว่า request body ที่ส่งไปเป็น JSON |
| `{ ... }` หลัง header | JSON body ที่ส่งไปกับ `POST` หรือ `PUT` |

การใช้ตัวแปร `baseUrl` ช่วยให้เปลี่ยน port ได้ในที่เดียว ไม่ต้องแก้ทุก request

เพิ่มตัวแปร `baseUrl`

```http
# Use the URL shown by dotnet run on your machine.
@baseUrl = http://localhost:<http-port>
```

ให้แทน `<http-port>` ด้วย port จริงจาก terminal ตอน `dotnet run`

ถ้า terminal แสดง HTTPS และเครื่องคุณ trust development certificate แล้ว จะใช้แบบนี้ก็ได้:

```http
# Use HTTPS only if your development certificate is trusted.
@baseUrl = https://localhost:<https-port>
```

## ทดสอบ GET รายการทั้งหมด

```http
### Get all users
GET {{baseUrl}}/api/v1/users
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `200 OK` และ JSON array

## ทดสอบ GET รายการเดียว

```http
### Get user by id
GET {{baseUrl}}/api/v1/users/1
Accept: application/json
```

ลองเปลี่ยน id เป็นค่าที่ไม่มี เช่น `999`

```http
### Get missing user
GET {{baseUrl}}/api/v1/users/999
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `404 Not Found`

## ทดสอบ POST

```http
### Create user
POST {{baseUrl}}/api/v1/users
Content-Type: application/json

{
  "email": "new-user@example.com"
}
```

ผลลัพธ์ที่คาดหวังคือ `201 Created`

หลัง POST สำเร็จ ให้กด `GET /api/v1/users` อีกครั้ง คุณควรเห็น user ใหม่ใน list

## ทดสอบ PUT

```http
### Update user
PUT {{baseUrl}}/api/v1/users/1
Content-Type: application/json

{
  "email": "updated-admin@example.com"
}
```

ผลลัพธ์ที่คาดหวังคือ `200 OK`

หลัง PUT สำเร็จ ให้กด `GET /api/v1/users/1` อีกครั้งเพื่อดูว่าข้อมูลเปลี่ยนแล้ว

ลองทดสอบ id ที่ไม่มีอยู่ด้วย:

```http
### Update missing user
PUT {{baseUrl}}/api/v1/users/999
Content-Type: application/json

{
  "email": "missing@example.com"
}
```

ผลลัพธ์ที่คาดหวังคือ `404 Not Found`

## ทดสอบ DELETE

```http
### Delete user
DELETE {{baseUrl}}/api/v1/users/1
```

ผลลัพธ์ที่คาดหวังคือ `204 No Content`

หลัง DELETE สำเร็จ ให้กด `GET /api/v1/users/1` อีกครั้ง ผลลัพธ์ควรเป็น `404 Not Found`

ลองทดสอบลบ id ที่ไม่มีอยู่ด้วย:

```http
### Delete missing user
DELETE {{baseUrl}}/api/v1/users/999
```

ผลลัพธ์ที่คาดหวังคือ `404 Not Found`

## ลำดับการทดสอบที่แนะนำ

ให้ทดสอบตามลำดับนี้เมื่อเริ่ม server ใหม่:

| ลำดับ | Request | ผลลัพธ์ที่ควรได้ |
| --- | --- | --- |
| 1 | `GET /api/v1/users` | `200 OK` และมี user เริ่มต้น |
| 2 | `GET /api/v1/users/1` | `200 OK` และได้ user id `1` |
| 3 | `GET /api/v1/users/999` | `404 Not Found` |
| 4 | `POST /api/v1/users` | `201 Created` และได้ user ใหม่ |
| 5 | `GET /api/v1/users` | `200 OK` และเห็น user ใหม่ใน list |
| 6 | `PUT /api/v1/users/1` | `200 OK` และ email เปลี่ยน |
| 7 | `GET /api/v1/users/1` | `200 OK` และเห็น email ใหม่ |
| 8 | `PUT /api/v1/users/999` | `404 Not Found` |
| 9 | `DELETE /api/v1/users/999` | `404 Not Found` |
| 10 | `DELETE /api/v1/users/1` | `204 No Content` |
| 11 | `GET /api/v1/users/1` | `404 Not Found` |

ถ้าคุณทดสอบ `DELETE` ก่อน `PUT` แล้ว `PUT /api/v1/users/1` ได้ `404 Not Found` ถือว่าถูกต้อง เพราะ user id `1` ถูกลบจาก memory ไปแล้ว ให้ restart API ถ้าต้องการกลับไปข้อมูลเริ่มต้น

## ไฟล์ Backend.Api.http แบบเต็ม

ส่วนนี้ให้ประกอบไฟล์จาก block ย่อยด้านล่าง ไม่ต้องคัดลอกเป็นก้อนเดียว ถ้าไฟล์ของคุณมี request บางส่วนอยู่แล้ว ให้เพิ่มเฉพาะส่วนที่ยังขาด

ส่วนที่ 1: ตัวแปร URL หลัก

```http
# Base URL for the API. Change the port to match dotnet run.
@baseUrl = http://localhost:<http-port>
```

ส่วนที่ 2: request สำหรับอ่านข้อมูล

```http
### Get all users
GET {{baseUrl}}/api/v1/users
Accept: application/json

### Get user by id
# id 1 exists in the initial in-memory data.
GET {{baseUrl}}/api/v1/users/1
Accept: application/json

### Get missing user
# id 999 should not exist, so this request should return 404.
GET {{baseUrl}}/api/v1/users/999
Accept: application/json
```

ส่วนที่ 3: request สำหรับสร้างข้อมูล

```http
### Create user
POST {{baseUrl}}/api/v1/users
Content-Type: application/json

{
  "email": "new-user@example.com"
}
```

ส่วนที่ 4: request สำหรับแก้ไขและลบข้อมูล

```http
### Update user
PUT {{baseUrl}}/api/v1/users/1
Content-Type: application/json

{
  "email": "updated-admin@example.com"
}

### Update missing user
# id 999 should not exist, so this request should return 404.
PUT {{baseUrl}}/api/v1/users/999
Content-Type: application/json

{
  "email": "missing@example.com"
}

### Delete missing user
# id 999 should not exist, so this request should return 404.
DELETE {{baseUrl}}/api/v1/users/999

### Delete user
# A successful delete should return 204 No Content.
DELETE {{baseUrl}}/api/v1/users/1
```

หลังเพิ่มครบแล้ว ให้เริ่มทดสอบจาก `GET /api/v1/users` ก่อนเสมอ เพื่อดูว่าข้อมูลเริ่มต้นยังอยู่ครบหรือไม่

## Postman ใช้ยังไง

ถ้าใช้ Postman ให้ทำแนวคิดเดียวกัน:

- สร้าง collection ชื่อ `Backend.Api`
- สร้าง variable `baseUrl`
- เพิ่ม request สำหรับ GET, POST, PUT, DELETE
- ตรวจ status code และ response body ทุกครั้ง

อย่าทดสอบแค่กรณีสำเร็จ ให้ทดสอบกรณีผิดด้วย เช่น id ที่ไม่มี และ JSON body ที่ส่งผิด

## เก็บ request ไว้ทำไม

ไฟล์ `Backend.Api.http` ทำหน้าที่เหมือนเอกสารทดสอบ API แบบง่าย ทุกครั้งที่แก้ endpoint เราสามารถกดทดสอบซ้ำได้ทันที

เมื่อโปรเจกต์โตขึ้น ไฟล์นี้จะมี request สำหรับ register, login, แนบ token, admin endpoint และ error case ต่าง ๆ

## ถ้าผลลัพธ์ไม่ตรง

ถ้าได้ `404 Not Found` ให้ตรวจ path เช่น `/api/v1/users` ต้องมี `/api` นำหน้า และใช้ id ที่ยังมีอยู่ใน memory

ถ้าได้ `405 Method Not Allowed` ให้ตรวจ HTTP method และ attribute ใน controller เช่น `PUT` ต้องคู่กับ `[HttpPut("{id:int}")]`

ถ้า request ไม่ถึง API ให้ตรวจว่า `dotnet run` หรือ Visual Studio debug ยังรันอยู่ และ `baseUrl` ใช้ port ตรงกับ terminal

ถ้าแก้ code แล้วผลลัพธ์ยังเหมือนเดิม ให้หยุด application แล้ว start ใหม่

ถ้าใช้ HTTPS แล้ว REST Client error เรื่อง certificate ให้ใช้ URL แบบ `http` ก่อน หรือ trust development certificate ด้วย `dotnet dev-certs https --trust`

## ตรวจไฟล์ .http ก่อนปิดภาค

ก่อนเข้าสู่ภาค Architecture ให้เปิด `Backend.Api.http` แล้วตรวจว่า request หลักมีครบ ไม่ต้องเพิ่มซ้ำถ้าคุณมีอยู่แล้ว:

```text
GET all users
GET user by id
GET missing user
POST create user
PUT update user
PUT missing user
DELETE missing user
DELETE user
```

กรณี `GET missing user`, `PUT missing user` และ `DELETE missing user` ใช้ตรวจว่า API ตอบ `404 Not Found` ถูกต้องเมื่อ id ไม่มีอยู่จริง

## Checklist ปิดภาค

ก่อนเข้าภาค Architecture ให้ตรวจว่า:

- `UsersController` มีครบทั้ง `GET`, `POST`, `PUT`, `DELETE`
- ทุก action ที่รับ id ใช้ `{id:int}`
- `POST` ตอบ `201 Created`
- `DELETE` สำเร็จตอบ `204 No Content`
- กรณี id ไม่มีตอบ `404 Not Found`
- ไฟล์ `.http` มี request สำหรับกรณีสำเร็จและกรณีผิด
- คุณอธิบายได้ว่า route parameter ต่างจาก request body อย่างไร

## Checkpoint

เมื่อจบบทนี้ คุณควรมีไฟล์ `Backend.Api.http` หรือไฟล์ `.http` ที่เทียบเท่า และทดสอบ endpoint เหล่านี้ได้

```text
GET    /api/v1/users
GET    /api/v1/users/{id}
POST   /api/v1/users
PUT    /api/v1/users/{id}
DELETE /api/v1/users/{id}
```

และควรตรวจได้ว่า endpoint แต่ละตัวตอบ status code ตรงกับที่ออกแบบไว้
