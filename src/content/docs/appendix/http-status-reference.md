---
title: ตาราง HTTP Status Code ที่ใช้บ่อย
description: สรุป status code สำคัญสำหรับ Web API
---

```text
200 OK                  สำเร็จ
201 Created             สร้างข้อมูลสำเร็จ
204 No Content          สำเร็จแต่ไม่มี body
400 Bad Request         request ผิดรูปแบบ
401 Unauthorized        ยังไม่ได้ login หรือ token ผิด
403 Forbidden           ไม่มีสิทธิ์
404 Not Found           ไม่พบข้อมูล
409 Conflict            ข้อมูลชนกัน
429 Too Many Requests   request ถี่เกิน rate limit
500 Internal Server Error server error
```

ใช้ status code ให้ตรงกับสถานการณ์ จะช่วยให้ client และ test เข้าใจ API ได้ชัดเจน
