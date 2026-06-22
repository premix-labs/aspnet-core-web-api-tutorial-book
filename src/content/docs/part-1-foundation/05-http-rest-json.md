---
title: 05 - พื้นฐาน HTTP, REST และ JSON
description: เข้าใจ request, response, method, route, status code และ JSON
---

Web API สื่อสารผ่าน HTTP ดังนั้นก่อนเขียน endpoint เราควรเข้าใจคำสำคัญที่ใช้ทุกวันในงาน backend

ถ้าเข้าใจบทนี้ บท Controller และ CRUD จะง่ายขึ้นมาก

## HTTP Request

Request คือข้อมูลที่ client ส่งมาหา server โดยทั่วไปประกอบด้วย method, URL, header และ body

ตัวอย่าง request:

```http
POST /api/users HTTP/1.1
Host: localhost:<port>
Content-Type: application/json
Authorization: Bearer <token>

{
  "email": "user@example.com",
  "password": "Passw0rd!"
}
```

ส่วนสำคัญของ request:

- `POST` คือ HTTP method
- `/api/users` คือ path หรือ route
- `Content-Type` บอกชนิดข้อมูลใน body
- `Authorization` ใช้ส่ง token หลังระบบมี login
- JSON body คือข้อมูลที่ส่งเข้า API

## HTTP Response

Response คือข้อมูลที่ server ส่งกลับไปหา client โดยทั่วไปประกอบด้วย status code, header และ body

ตัวอย่าง response:

```http
HTTP/1.1 201 Created
Content-Type: application/json
Location: /api/users/1

{
  "id": 1,
  "email": "user@example.com"
}
```

ส่วนสำคัญของ response:

- `201 Created` คือ status code
- `Content-Type` บอกชนิดข้อมูลที่ส่งกลับ
- `Location` มักใช้บอก URL ของ resource ที่เพิ่งสร้าง
- JSON body คือข้อมูลที่ API ส่งกลับ

## HTTP Method ที่ใช้บ่อย

ใน CRUD API เราจะใช้ method เหล่านี้บ่อยที่สุด

```text
GET     อ่านข้อมูล
POST    สร้างข้อมูลใหม่
PUT     แก้ไขข้อมูลทั้งก้อน
PATCH   แก้ไขข้อมูลบางส่วน
DELETE  ลบข้อมูล
```

ตัวอย่างกับ resource `users`:

```text
GET    /api/users       อ่าน user ทั้งหมด
GET    /api/users/1     อ่าน user id 1
POST   /api/users       สร้าง user ใหม่
PUT    /api/users/1     แก้ไข user id 1 ทั้งก้อน
PATCH  /api/users/1     แก้ไข user id 1 บาง field
DELETE /api/users/1     ลบ user id 1
```

## REST คืออะไร

REST คือแนวทางออกแบบ API ให้ resource มี URL ชัดเจน และใช้ HTTP method ให้ตรงกับการกระทำ

คำว่า resource หมายถึงสิ่งที่ API จัดการ เช่น user, product, order หรือ audit log

ตัวอย่าง route ที่อ่านง่าย:

```text
GET /api/users
GET /api/users/1
POST /api/users
```

ตัวอย่าง route ที่ควรหลีกเลี่ยง:

```text
GET /api/getUsers
POST /api/createUser
POST /api/deleteUser
```

เหตุผลคือ method เช่น `GET`, `POST`, `DELETE` บอก action อยู่แล้ว ไม่จำเป็นต้องใส่ action ซ้ำใน URL

## หลักออกแบบ Route แบบง่าย

เวลาออกแบบ REST API ให้เริ่มจาก resource ก่อน แล้วค่อยเลือก HTTP method

ตัวอย่าง resource คือ `users`

```text
GET    /api/users       อ่านรายการ user
GET    /api/users/1     อ่าน user คนเดียว
POST   /api/users       สร้าง user ใหม่
PUT    /api/users/1     แก้ไข user คนเดียว
DELETE /api/users/1     ลบ user คนเดียว
```

หลักที่ควรจำ:

- ใช้คำนามใน URL เช่น `users`, `orders`, `products`
- ใช้ HTTP method เพื่อบอกการกระทำ
- ไม่ควรใช้ `GET` เพื่อแก้ไขหรือลบข้อมูล
- ถ้าต้องระบุ record เดียว ให้ใส่ id ใน path
- ถ้าต้องค้นหา กรอง หรือแบ่งหน้า ให้ใช้ query string

## JSON คืออะไร

JSON คือ format ที่นิยมใช้ส่งข้อมูลระหว่าง client และ API เพราะอ่านง่ายและ mapping กับ object ในภาษา programming ได้สะดวก

ตัวอย่าง JSON:

```json
{
  "id": 1,
  "email": "admin@example.com",
  "role": "Admin",
  "isActive": true
}
```

กฎพื้นฐานที่ควรรู้:

- key ต้องอยู่ในเครื่องหมาย double quote
- string ต้องอยู่ใน double quote
- boolean ใช้ `true` หรือ `false`
- array ใช้ `[]`
- object ใช้ `{}`
- ห้ามมี comma ตัวสุดท้ายหลัง property สุดท้าย

## Status Code ที่ควรรู้ก่อน

Status code คือรหัสที่ server ใช้บอกผลลัพธ์ของ request

```text
200 OK                  สำเร็จและมี body
201 Created             สร้างข้อมูลสำเร็จ
204 No Content          สำเร็จแต่ไม่มี body
400 Bad Request         request ผิดรูปแบบหรือ validation ไม่ผ่าน
401 Unauthorized        ยังไม่ได้ login หรือ token ไม่ถูกต้อง
403 Forbidden           login แล้วแต่ไม่มีสิทธิ์
404 Not Found           ไม่พบข้อมูล
409 Conflict            request ชนกับสถานะปัจจุบันของระบบ เช่น email ซ้ำ
500 Internal Server Error server มีปัญหา
```

อย่าจำแค่เลข ให้จำความหมาย เพราะ client ใช้ status code เหล่านี้ตัดสินใจว่าจะทำอะไรต่อ

## 401 กับ 403 ต่างกันอย่างไร

`401 Unauthorized` หมายถึงระบบยังยืนยันตัวตนไม่ได้ เช่นไม่ส่ง token, token หมดอายุ หรือ token ปลอม

`403 Forbidden` หมายถึงยืนยันตัวตนแล้ว แต่ไม่มีสิทธิ์ทำสิ่งนั้น เช่น user ธรรมดาเรียก admin endpoint

สองรหัสนี้สำคัญมากในภาค Authentication และ Admin

## Header สำคัญที่เจอบ่อย

```text
Content-Type: application/json
Accept: application/json
Authorization: Bearer <token>
```

`Content-Type` บอก server ว่า body ที่ส่งมาเป็นอะไร

`Accept` บอก server ว่า client อยากรับ response แบบไหน

`Authorization` ใช้ส่ง credential เช่น JWT token

## ลองอ่าน Request และ Response

ดู request นี้:

```http
GET /api/users/1 HTTP/1.1
Host: localhost:<port>
Accept: application/json
Authorization: Bearer <token>
```

เราสามารถอ่านได้ว่า client ต้องการอ่าน user id `1`, อยากรับข้อมูลเป็น JSON และส่ง token มาด้วย

ถ้า API ตอบกลับแบบนี้:

```http
HTTP/1.1 200 OK
Content-Type: application/json

{
  "id": 1,
  "email": "admin@example.com"
}
```

แปลว่า request สำเร็จ และ API ส่งข้อมูล user กลับมาในรูปแบบ JSON

ถ้า API ตอบแบบนี้:

```http
HTTP/1.1 404 Not Found
```

แปลว่า route เรียกถูก แต่ไม่พบ resource ที่ต้องการ เช่นไม่มี user id `1`

## แบบฝึกหัด

ลองตอบก่อนดูแนวคำตอบ:

1. ถ้าต้องการอ่านรายการสินค้า ควรใช้ method และ route อะไร
2. ถ้าต้องการสร้าง order ใหม่ ควรใช้ method และ route อะไร
3. ถ้าต้องการลบ user id `10` ควรใช้ method และ route อะไร
4. ถ้า user ยังไม่ได้ login แล้วเรียก endpoint ที่ต้องใช้ token ควรตอบ status code อะไร
5. ถ้า user login แล้วแต่ไม่ใช่ admin แล้วเรียก admin endpoint ควรตอบ status code อะไร

## แนวคำตอบโดยย่อ

- อ่านรายการสินค้า: `GET /api/products`
- สร้าง order ใหม่: `POST /api/orders`
- ลบ user id `10`: `DELETE /api/users/10`
- ยังไม่ได้ login หรือ token ไม่ถูกต้อง: `401 Unauthorized`
- login แล้วแต่ไม่มีสิทธิ์: `403 Forbidden`

## Checkpoint

ก่อนเข้าสู่ภาค Controller คุณควรตอบได้ว่า

- HTTP request มีส่วนประกอบอะไรบ้าง
- HTTP response มีส่วนประกอบอะไรบ้าง
- `GET`, `POST`, `PUT`, `PATCH`, `DELETE` ต่างกันอย่างไร
- ทำไม API นิยมใช้ JSON
- `401` กับ `403` ต่างกันอย่างไร
- REST route สำหรับ user ควรออกแบบประมาณไหน
