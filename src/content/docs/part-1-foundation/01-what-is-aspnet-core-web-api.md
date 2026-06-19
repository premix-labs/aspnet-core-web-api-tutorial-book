---
title: 01 - ASP.NET Core Web API คืออะไร
description: ทำความเข้าใจบทบาทของ ASP.NET Core Web API ในงาน backend
---

ASP.NET Core Web API คือ framework สำหรับสร้าง HTTP API ด้วยภาษา C# และ .NET เหมาะกับงาน backend ที่ต้องรับ request จาก frontend, mobile app หรือระบบอื่น แล้วตอบกลับเป็นข้อมูล เช่น JSON

ในหนังสือเล่มนี้เราจะสร้าง API สำหรับระบบ Secure Admin ทีละขั้น จาก project ว่างไปจนถึงระบบที่มี register, login, JWT, role admin, database, validation, test และ Docker

## API อยู่ตรงไหนของระบบ

ให้มองระบบทั่วไปเป็น 3 ส่วนหลัก

```text
Client  ->  Web API  ->  Database
```

`Client` อาจเป็นเว็บ React, Angular, Vue, mobile app, Postman หรือระบบ backend อื่น

`Web API` คือชั้นกลางที่รับ request ตรวจข้อมูล ตรวจสิทธิ์ ประมวลผล business rule แล้วอ่าน/เขียนข้อมูลกับ database

`Database` คือที่เก็บข้อมูลจริง เช่น user, role, audit log และข้อมูลธุรกิจอื่น

## Web API ทำหน้าที่อะไร

Web API คือทางเข้าของระบบ backend โดยทั่วไปทำงานตามลำดับนี้

```text
HTTP Request
  -> Routing
  -> Controller
  -> Service
  -> Repository หรือ DbContext
  -> Database
  -> HTTP Response
```

ตัวอย่างเช่น frontend เรียก `GET /api/users` เพื่อขอรายการผู้ใช้ ระบบ backend จะค้นข้อมูลผู้ใช้จากฐานข้อมูล แล้วส่ง JSON กลับไป

ถ้า frontend เรียก `POST /api/auth/login` ระบบ backend จะรับ email/password, ตรวจ password hash, สร้าง JWT แล้วส่ง token กลับไป

## ASP.NET Core ทำอะไรให้เรา

ถ้าเขียน HTTP server เองทั้งหมด เราต้องจัดการหลายเรื่อง เช่นเปิด port, parse request, match URL, แปลง JSON, จัดการ error และส่ง response กลับเอง

ASP.NET Core เตรียมสิ่งเหล่านี้ไว้ให้:

- Routing สำหรับจับคู่ URL กับ endpoint
- Controller สำหรับจัดกลุ่ม endpoint
- Model binding สำหรับแปลง JSON เป็น C# object
- Validation สำหรับตรวจ request
- Dependency Injection สำหรับจัดการ service
- Middleware pipeline สำหรับ authentication, authorization, logging และ error handling
- Configuration สำหรับอ่านค่าจาก `appsettings.json` และ environment variables
- Integration กับ EF Core, OpenAPI, testing และ Docker

สิ่งที่เราต้องทำคือเขียน business logic และจัดโครงสร้าง code ให้ดี

## .NET กับ ASP.NET Core ต่างกันอย่างไร

`.NET` คือ platform หลักสำหรับรันภาษา C# และ library ต่าง ๆ

`ASP.NET Core` คือ framework ที่อยู่บน .NET อีกที ใช้สำหรับสร้าง web application และ Web API

จำแบบง่ายได้ว่า .NET คือฐาน ส่วน ASP.NET Core คือเครื่องมือสำหรับทำเว็บและ API

```text
C#
  -> .NET
  -> ASP.NET Core
  -> Web API
```

## Controller API กับ Minimal API

ASP.NET Core ทำ API ได้หลายรูปแบบ สองรูปแบบที่เจอบ่อยคือ Controller API และ Minimal API

Controller API เหมาะกับโปรเจกต์ที่เริ่มมีหลาย endpoint ต้องแยกไฟล์เป็นระบบ ต้องใช้ attribute เช่น `[HttpGet]`, `[Authorize]` และต้องการโครงสร้างที่ชัดเจน

Minimal API เหมาะกับ API ขนาดเล็ก หรือ service ที่ต้องการเขียน endpoint แบบกระชับใน `Program.cs`

หนังสือนี้เลือก Controller API เพราะมือใหม่จะเห็นการแยกหน้าที่ชัดเจนกว่า:

- Controller รับ HTTP request
- DTO กำหนดข้อมูลเข้า/ออก
- Service จัดการ business logic
- Repository จัดการข้อมูล
- Middleware จัดการงานข้าม endpoint เช่น error, auth และ logging

## เราจะสร้างโปรเจกต์อะไร

โปรเจกต์หลักของหนังสือนี้คือ **Backend API**

ความสามารถสุดท้ายของระบบ:

- สมัครสมาชิกและเข้าสู่ระบบ
- hash password ก่อนบันทึกลงฐานข้อมูล
- ออก JWT token หลัง login
- อ่านข้อมูล user ปัจจุบันจาก token
- จำกัด endpoint บางส่วนให้เฉพาะ admin
- จัดการ user ผ่าน admin API
- ตรวจ validation และตอบ error format ที่อ่านง่าย
- บันทึก audit log
- เชื่อม SQL Server ด้วย EF Core
- มี unit test, integration test และ Docker Compose

## สิ่งที่ยังไม่ต้องกังวลตอนนี้

ถ้าคุณเป็นมือใหม่ อาจเห็นคำหลายคำพร้อมกัน เช่น middleware, dependency injection, JWT, EF Core และ migration แล้วรู้สึกเยอะเกินไป

ไม่ต้องจำทุกอย่างในบทแรก หน้าที่ของบทนี้คือให้เข้าใจว่า Web API เป็น backend ที่รับ request และส่ง response ส่วนรายละเอียดแต่ละเรื่องจะค่อย ๆ ต่อกันในบทถัดไป

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรตอบคำถามเหล่านี้ได้

- Web API รับ request และส่ง response เพื่ออะไร
- Client, API และ database อยู่ในระบบเดียวกันอย่างไร
- .NET กับ ASP.NET Core ต่างกันอย่างไร
- ทำไมหนังสือนี้เลือกใช้ Controller API
- โปรเจกต์สุดท้ายของหนังสือจะมี feature อะไรบ้าง
