---
title: ปัญหาที่พบบ่อยใน JWT
description: รวมปัญหา token, claims และ authorization ที่เจอบ่อย
---

## ได้ 401 Unauthorized

ตรวจว่า client ส่ง header ถูกต้อง

```http
Authorization: Bearer jwt-token-here
```

ตรวจว่า token ยังไม่หมดอายุ issuer และ audience ตรงกับ configuration

## ได้ 403 Forbidden

แปลว่า login ผ่านแล้ว แต่ role หรือ policy ไม่พอ ให้ตรวจ claim `role` ใน token

## Token ใช้ไม่ได้หลัง restart

ตรวจว่า signing key ไม่ได้เปลี่ยนทุกครั้งที่ application start
