---
title: ภาค 3 - จัดโครงสร้างโปรเจกต์ให้พร้อมโต
description: แยก Controller, Service, Repository, DTO และ Response Format
---

ภาคนี้ปรับจาก code ที่เขียนทุกอย่างไว้ใน Controller ไปเป็นโครงสร้างที่ดูแลต่อได้ง่ายขึ้น โดยแยกหน้าที่ของแต่ละชั้นให้ชัดเจน

## บทในภาคนี้

- บทที่ 11: แยก Controller, Service, Repository
- บทที่ 12: Dependency Injection
- บทที่ 13: DTO คืออะไรและใช้เมื่อไหร่
- บทที่ 14: Mapping ระหว่าง Entity กับ DTO
- บทที่ 15: Response Format ที่อ่านง่ายและดูแลต่อได้

## เป้าหมายของภาคนี้

หลังจบภาคนี้ ผู้อ่านควรเข้าใจว่าทำไม Controller ไม่ควรมี business logic หนาเกินไป และควรแยก code แบบใดเมื่อโปรเจกต์เริ่มโตขึ้น
