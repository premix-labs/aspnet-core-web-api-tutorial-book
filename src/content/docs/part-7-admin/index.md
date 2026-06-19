---
title: ภาค 7 - Admin API และ Authorization
description: เพิ่ม role, admin endpoint, user management, audit log และ pagination
---

ภาคนี้ต่อยอดจาก JWT ไปสู่ authorization โดยใช้ role ที่อยู่ใน token เพื่อจำกัดสิทธิ์ endpoint สำหรับผู้ดูแลระบบ

หลังจบภาคนี้ API จะมี admin endpoint สำหรับดูรายชื่อผู้ใช้ เปลี่ยน role เปิดปิดบัญชี ป้องกัน admin ทำ action อันตรายกับบัญชีตัวเอง บันทึก audit log และรองรับ pagination/filtering/sorting

## บทในภาคนี้

- บทที่ 34: ออกแบบ Role และ Permission
- บทที่ 35: สร้าง Admin Endpoint
- บทที่ 36: ดูรายการผู้ใช้สำหรับ Admin
- บทที่ 37: เปลี่ยน Role หรือสถานะผู้ใช้
- บทที่ 38: ป้องกันไม่ให้ Admin ลบตัวเองผิดพลาด
- บทที่ 39: ทำ Audit Log
- บทที่ 40: Pagination, Filtering และ Sorting

## สิ่งที่ต้องได้หลังจบภาคนี้

- มี role constants แทนการใช้ string กระจัดกระจาย
- มีบัญชี admin เริ่มต้นสำหรับทดสอบ
- endpoint admin ถูกป้องกันด้วย `[Authorize(Roles = Roles.Admin)]`
- Admin ดูรายชื่อผู้ใช้ได้
- Admin เปลี่ยน role และสถานะผู้ใช้ได้
- ระบบป้องกัน Admin deactivate หรือ demote ตัวเองผิดพลาด
- action สำคัญถูกบันทึก audit log
- รายการผู้ใช้รองรับ pagination, filtering และ sorting

## ภาพรวมสิทธิ์

ในหนังสือเล่มนี้เราจะเริ่มด้วย role สองแบบ

```text
User
Admin
```

`User` คือผู้ใช้ทั่วไป ส่วน `Admin` คือผู้ดูแลระบบที่จัดการผู้ใช้คนอื่นได้
