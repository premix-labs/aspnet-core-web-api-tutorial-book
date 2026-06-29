---
title: ภาค 5 - Validation และ Error Handling
description: ตรวจข้อมูล input และจัดการ error ให้ response สม่ำเสมอ
---

ภาคนี้ทำให้ API แข็งแรงขึ้น โดยตรวจ request ที่เข้ามาและจัดการ error แบบรวมศูนย์

หลังจบภาคฐานข้อมูล API ของเราบันทึกข้อมูลลง SQL Server ได้แล้ว แต่ยังมีปัญหาที่ระบบจริงต้องแก้ เช่น email ผิดรูปแบบ, request body ว่าง, email ซ้ำ, database error และ exception ที่ไม่ควรถูกส่งเป็น stack trace ให้ client เห็น

## วิธีเรียนภาคนี้

ภาคนี้มีทั้ง validation rule, exception class, middleware และ response format ให้ทำทีละชั้น:

1. เริ่มจาก validation ที่ DTO ก่อน
2. แยก custom validation ที่ตรวจรูปแบบ input
3. ย้าย business error ไปให้ service โยน exception
4. ให้ global exception handler แปลง exception เป็น response
5. ปรับ error response ให้ frontend ใช้งานต่อได้ง่าย
6. ทดสอบ status code ด้วย `.http`

อย่าเริ่มจากการคัดลอก handler ทั้งไฟล์ ให้เข้าใจก่อนว่า error แต่ละชนิดเกิดที่ชั้นไหนและควรถูกแปลงเป็น HTTP response แบบใด

ถ้าเครื่องของคุณใช้ port ไม่ตรงกับตัวอย่าง ให้ใช้ port ที่ `dotnet run` หรือ Visual Studio แสดงจริง เช่น `http://localhost:5156` หรือ `https://localhost:7127`

ตัวอย่าง `.http` ในภาคนี้จะใช้ตัวแปรสองตัว:

```http
@baseUrl = http://localhost:5156
@usersPath = /api/users
```

ถ้าโปรเจกต์ของคุณใช้ versioned route เช่น `/api/v1/users` ให้เปลี่ยนเฉพาะ `@usersPath` เป็น:

```http
@usersPath = /api/v1/users
```

หลักคือ route ใน `.http` ต้องตรงกับ `[Route(...)]` ใน `UsersController` เสมอ

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

## ภาพรวม flow หลังจบภาคนี้

```mermaid
flowchart LR
    Client["HTTP Client"] --> Controller["UsersController"]
    Controller --> Validation["DTO Validation"]
    Validation --> Service["UserService"]
    Service --> Exception["Business Exception"]
    Exception --> Handler["GlobalExceptionHandler"]
    Handler --> Problem["ProblemDetails / ValidationProblemDetails"]
    Problem --> Client
```

## แนวคิดสำคัญ

Validation คือการกัน request ที่ผิดตั้งแต่ขอบของระบบ ส่วน error handling คือการแปลงปัญหาที่เกิดระหว่างทำงานให้เป็น response ที่ client เข้าใจได้

ทั้งสองเรื่องนี้ควรทำให้เป็นระบบตั้งแต่ต้น เพราะถ้าปล่อยให้ Controller แต่ละตัวตอบ error คนละรูปแบบ เมื่อโปรเจกต์โตขึ้น frontend และ tester จะทำงานยากทันที
