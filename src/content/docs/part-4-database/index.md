---
title: ภาค 4 - เชื่อมต่อฐานข้อมูลด้วย EF Core
description: ใช้ Entity Framework Core เชื่อมต่อ SQL Server และจัดการข้อมูลจริง
---

ภาคนี้เปลี่ยนจากข้อมูลใน memory ไปใช้ฐานข้อมูลจริงด้วย Entity Framework Core เพื่อให้ API เก็บข้อมูลถาวรได้

จุดสำคัญของภาคนี้คือผู้อ่านต้องเข้าใจ flow ครบตั้งแต่ติดตั้ง package, สร้าง entity, สร้าง `DbContext`, ตั้งค่า connection string, สร้าง migration, update database, ทำ CRUD ผ่าน repository และ seed ข้อมูลเริ่มต้น

## บทในภาคนี้

- บทที่ 16: ติดตั้ง Entity Framework Core
- บทที่ 17: สร้าง User Entity
- บทที่ 18: สร้าง DbContext
- บทที่ 19: ตั้งค่า Connection String
- บทที่ 20: ใช้ Migration
- บทที่ 21: ทำ CRUD กับฐานข้อมูลจริง
- บทที่ 22: Seed ข้อมูลเริ่มต้น

## สิ่งที่ต้องได้หลังจบภาคนี้

- โปรเจกต์ติดตั้ง EF Core package ที่จำเป็นแล้ว
- มี `User` entity และ `AppDbContext`
- เชื่อมต่อ SQL Server ผ่าน connection string ได้
- สร้างและรัน migration ได้
- CRUD API ใช้ database จริง ไม่ใช่ in-memory list
- มี seed data สำหรับทดสอบระบบฐานข้อมูล

## ข้อควรรู้ก่อนเริ่ม

ภาคนี้ยังไม่ทำระบบ login จริง แม้ `User` entity จะมี `PasswordHash` แล้วก็ตาม เพราะการสร้าง password hash อย่างถูกต้องจะสอนในภาค Authentication

ดังนั้นข้อมูล seed ในภาคนี้เป็นข้อมูลทดสอบฐานข้อมูลเท่านั้น ไม่ใช่บัญชีที่ใช้ login ได้จริง
