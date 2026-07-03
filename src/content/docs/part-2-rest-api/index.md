---
title: ภาค 2 - Controller และ REST API
description: สร้าง endpoint ด้วย Controller และทำ CRUD API พื้นฐาน
---

ภาคนี้เริ่มเขียน endpoint จริงด้วย Controller โดยใช้ข้อมูลใน memory ก่อน เพื่อให้เข้าใจ flow ของ HTTP API โดยยังไม่ต้องกังวลเรื่องฐานข้อมูล

## วิธีเรียนภาคนี้

ให้ทำตามบทเรียนด้วยโปรเจกต์ `Backend.Api` ที่สร้างจากภาค 1 และรัน API จริงทุกครั้งที่เพิ่ม endpoint

ภาคนี้ใช้ข้อมูลใน memory ด้วย `static List<T>` ดังนั้นข้อมูลจะเปลี่ยนระหว่างที่ application ยังรันอยู่ ถ้า restart server ข้อมูลจะกลับไปเป็นค่าเริ่มต้น เรื่องนี้ตั้งใจให้เกิดขึ้นเพื่อให้เห็นผลของ `POST`, `PUT` และ `DELETE` ชัดเจนก่อนเชื่อมต่อ database

เวลา test ให้ใช้ port ที่ terminal ของคุณแสดงจริง เช่น `http://localhost:<http-port>` หรือ `https://localhost:<https-port>` ไม่ต้องยึดเลข port จากตัวอย่างในหนังสือ

## บทในภาคนี้

- บทที่ 6: สร้าง Controller แรก
- บทที่ 7: ทำ GET, POST, PUT, DELETE
- บทที่ 8: Routing และ Route Parameter
- บทที่ 9: Request, Response และ Status Code
- บทที่ 10: ทดสอบ API ด้วย REST Client หรือ Postman

## สิ่งที่ต้องได้หลังจบภาคนี้

- สร้าง Controller ได้
- เขียน endpoint CRUD ได้
- รับค่าจาก route และ request body ได้
- เลือก response และ status code พื้นฐานได้ถูกต้อง
- ทดสอบ API ด้วยเครื่องมือภายนอกได้

## Checklist ก่อนเข้าภาคถัดไป

ก่อนจบภาคนี้ โปรเจกต์ของคุณควรมีสิ่งเหล่านี้:

- `UsersController` อยู่ในโฟลเดอร์ `Controllers`
- `Program.cs` มี `builder.Services.AddControllers()` และ `app.MapControllers()`
- endpoint `GET`, `POST`, `PUT`, `DELETE` ทำงานได้
- route parameter ใช้รูปแบบ `{id:int}` ไม่ใช่ `"id:int"`
- ไฟล์ `.http` ใช้ `baseUrl` ตรงกับ port จริงของเครื่องคุณ
- ทดสอบทั้งกรณีสำเร็จและกรณีไม่พบข้อมูลได้
- `dotnet build` ผ่านหลังเพิ่ม CRUD endpoint
- ยังไม่ต้องสร้าง `Services`, `Repositories`, `Dtos` หรือเชื่อมต่อ database ในภาคนี้ เพราะจะเริ่มแยก architecture ในภาคถัดไป
