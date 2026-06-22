---
title: ภาค 3 - จัดโครงสร้างโปรเจกต์ให้พร้อมโต
description: แยก Controller, Service, Repository, DTO และ Response Format
---

ภาคนี้ปรับจาก code ที่เขียนทุกอย่างไว้ใน Controller ไปเป็นโครงสร้างที่ดูแลต่อได้ง่ายขึ้น โดยแยกหน้าที่ของแต่ละชั้นให้ชัดเจน

## วิธีเรียนภาคนี้

ภาคนี้จะเริ่มมีหลายไฟล์มากกว่าภาคก่อน ให้ทำทีละบทและ build หลังจบบทที่มีการแก้ code

อย่าเพิ่งรีบจำ pattern ทั้งหมดในครั้งเดียว ให้จับภาพใหญ่ก่อนว่า:

```text
Controller รับ HTTP
Service จัดการ business logic
Repository จัดการข้อมูล
DTO ควบคุมข้อมูลเข้าออก API
Mapping แปลงข้อมูลภายในเป็นข้อมูลสำหรับ response
```

ถ้า code compile ไม่ผ่าน ให้ดู error แรกก่อน แล้วตรวจ namespace, using, ชื่อ interface และการลงทะเบียน DI ใน `Program.cs`

## บทในภาคนี้

- บทที่ 11: แยก Controller, Service, Repository
- บทที่ 12: Dependency Injection
- บทที่ 13: DTO คืออะไรและใช้เมื่อไหร่
- บทที่ 14: Mapping ระหว่าง Entity กับ DTO
- บทที่ 15: Response Format ที่อ่านง่ายและดูแลต่อได้

## เป้าหมายของภาคนี้

หลังจบภาคนี้ ผู้อ่านควรเข้าใจว่าทำไม Controller ไม่ควรมี business logic หนาเกินไป และควรแยก code แบบใดเมื่อโปรเจกต์เริ่มโตขึ้น

## Checklist หลังจบภาคนี้

โปรเจกต์ของคุณควรมีโครงสร้างหลักประมาณนี้:

```text
Backend.Api/
  Controllers/
  Dtos/
    Users/
  Models/
  Repositories/
  Services/
```

และควรอธิบายได้ว่า request หนึ่งไหลผ่านชั้นเหล่านี้อย่างไร:

```text
HTTP Request
  -> Controller
  -> Service
  -> Repository
  -> data source
  -> Repository
  -> Service
  -> Controller
  -> HTTP Response
```
