---
title: ภาค 5 - Validation และ Error Handling
description: ตรวจข้อมูล input และจัดการ error ให้ response สม่ำเสมอ
---

ภาคนี้ทำให้ API แข็งแรงขึ้น โดยตรวจ request ที่เข้ามาและจัดการ error แบบรวมศูนย์

หลังจบภาคฐานข้อมูล API ของเราบันทึกข้อมูลลง SQL Server ได้แล้ว แต่ยังมีปัญหาที่ระบบจริงต้องแก้ เช่น email ผิดรูปแบบ, request body ว่าง, email ซ้ำ, database error และ exception ที่ไม่ควรถูกส่งเป็น stack trace ให้ client เห็น

## บทในภาคนี้

- บทที่ 23: Validation ด้วย Data Annotations
- บทที่ 24: Custom Validation
- บทที่ 25: Global Exception Handler
- บทที่ 26: Error Response Format
- บทที่ 27: เลือกใช้ HTTP Status Code ให้ถูกต้อง

## สิ่งที่ต้องได้หลังจบภาคนี้

- DTO ตรวจ input ด้วย Data Annotations ได้
- API ตอบ `400 Bad Request` อัตโนมัติเมื่อ request ไม่ผ่าน validation
- มี custom validation rule สำหรับ rule ที่ attribute พื้นฐานไม่พอ
- มี exception handler กลาง ไม่ต้องใส่ `try-catch` ซ้ำใน Controller
- Error response มีรูปแบบสม่ำเสมอและมี `code` ให้ frontend ใช้ต่อได้
- เลือก status code หลัก ๆ สำหรับ CRUD, validation และ business error ได้ถูกต้อง

## แนวคิดสำคัญ

Validation คือการกัน request ที่ผิดตั้งแต่ขอบของระบบ ส่วน error handling คือการแปลงปัญหาที่เกิดระหว่างทำงานให้เป็น response ที่ client เข้าใจได้

ทั้งสองเรื่องนี้ควรทำให้เป็นระบบตั้งแต่ต้น เพราะถ้าปล่อยให้ Controller แต่ละตัวตอบ error คนละรูปแบบ เมื่อโปรเจกต์โตขึ้น frontend และ tester จะทำงานยากทันที
