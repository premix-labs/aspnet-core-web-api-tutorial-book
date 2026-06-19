---
title: ตัวอย่าง REST Client และ Postman Collection
description: แนวทางเก็บ request สำหรับทดสอบ API
---

หนังสือจะใช้ไฟล์ `.http` เป็นหลัก เพราะเก็บใน Git ได้ง่ายและอ่านเป็น text ได้ทันที

ถ้าสร้าง project ด้วย template ของ ASP.NET Core ไฟล์นี้มักชื่อ `Backend.Api.http` ตามชื่อ project แต่ถ้าทีมของคุณอยากตั้งชื่อ `requests.http` ก็ทำได้เหมือนกัน

## ตัวอย่างโครงไฟล์

```http
@baseUrl = https://localhost:7001
@token = paste-token-here

GET {{baseUrl}}/api/users
Accept: application/json

###

GET {{baseUrl}}/api/auth/me
Authorization: Bearer {{token}}
Accept: application/json
```

ถ้าใช้ Postman ให้สร้าง collection ตาม endpoint เดียวกัน และแยก environment สำหรับ local, staging และ production
