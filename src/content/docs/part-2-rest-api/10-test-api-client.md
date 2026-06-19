---
title: 10 - ทดสอบ API ด้วย REST Client หรือ Postman
description: ทดสอบ endpoint ด้วยเครื่องมือภายนอกเพื่อดู request และ response จริง
---

หลังสร้าง endpoint แล้วต้องทดสอบด้วยเครื่องมือที่ส่ง HTTP request ได้จริง ไม่ควรดูแค่ code แล้วสรุปว่า API ทำงานถูกต้อง

บทนี้ใช้ REST Client ใน Visual Studio Code เป็นหลัก เพราะเก็บ request ไว้ใน repository ได้ง่าย แต่ถ้าคุณถนัด Postman ก็ใช้หลักการเดียวกัน

## เตรียม API ให้รันอยู่

เปิด terminal ที่โฟลเดอร์โปรเจกต์ `Backend.Api`

```powershell
dotnet run
```

ดู URL ที่ terminal แสดง เช่น

```text
https://localhost:7001
```

ถ้าเครื่องคุณแสดง port อื่น ให้ใช้ port ของเครื่องคุณ

## เตรียมไฟล์ Backend.Api.http

ตอนสร้าง project ด้วย template ของ ASP.NET Core จะมีไฟล์ `.http` มาให้แล้ว ชื่อไฟล์มักเป็นชื่อเดียวกับ project

```text
Backend.Api.http
```

ให้เปิดไฟล์นี้แล้วแทน request ตัวอย่างของ template ด้วย request ในบทนี้

ถ้า project ของคุณไม่มีไฟล์นี้ จะสร้างไฟล์ชื่อ `Backend.Api.http` หรือ `requests.http` เองก็ได้ หลักการเหมือนกัน

เพิ่มตัวแปร `baseUrl`

```http
@baseUrl = https://localhost:7001
```

ถ้า port ในเครื่องคุณไม่ใช่ `7001` ให้แก้ `baseUrl` ให้ตรงกับ terminal ตอน `dotnet run`

## ทดสอบ GET รายการทั้งหมด

```http
### Get all users
GET {{baseUrl}}/api/users
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `200 OK` และ JSON array

## ทดสอบ GET รายการเดียว

```http
### Get user by id
GET {{baseUrl}}/api/users/1
Accept: application/json
```

ลองเปลี่ยน id เป็นค่าที่ไม่มี เช่น `999`

```http
### Get missing user
GET {{baseUrl}}/api/users/999
Accept: application/json
```

ผลลัพธ์ที่คาดหวังคือ `404 Not Found`

## ทดสอบ POST

```http
### Create user
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "new-user@example.com"
}
```

ผลลัพธ์ที่คาดหวังคือ `201 Created`

หลัง POST สำเร็จ ให้กด `GET /api/users` อีกครั้ง คุณควรเห็น user ใหม่ใน list

## ทดสอบ PUT

```http
### Update user
PUT {{baseUrl}}/api/users/1
Content-Type: application/json

{
  "email": "updated-admin@example.com"
}
```

ผลลัพธ์ที่คาดหวังคือ `200 OK`

หลัง PUT สำเร็จ ให้กด `GET /api/users/1` อีกครั้งเพื่อดูว่าข้อมูลเปลี่ยนแล้ว

## ทดสอบ DELETE

```http
### Delete user
DELETE {{baseUrl}}/api/users/1
```

ผลลัพธ์ที่คาดหวังคือ `204 No Content`

หลัง DELETE สำเร็จ ให้กด `GET /api/users/1` อีกครั้ง ผลลัพธ์ควรเป็น `404 Not Found`

## ไฟล์ Backend.Api.http แบบเต็ม

คุณสามารถใช้ไฟล์นี้เป็นจุดเริ่มต้น

```http
@baseUrl = https://localhost:7001

### Get all users
GET {{baseUrl}}/api/users
Accept: application/json

### Get user by id
GET {{baseUrl}}/api/users/1
Accept: application/json

### Get missing user
GET {{baseUrl}}/api/users/999
Accept: application/json

### Create user
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "new-user@example.com"
}

### Update user
PUT {{baseUrl}}/api/users/1
Content-Type: application/json

{
  "email": "updated-admin@example.com"
}

### Delete user
DELETE {{baseUrl}}/api/users/1
```

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

## Checkpoint

เมื่อจบบทนี้ คุณควรมีไฟล์ `Backend.Api.http` หรือไฟล์ `.http` ที่เทียบเท่า และทดสอบ endpoint เหล่านี้ได้

```text
GET    /api/users
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
```

และควรตรวจได้ว่า endpoint แต่ละตัวตอบ status code ตรงกับที่ออกแบบไว้
