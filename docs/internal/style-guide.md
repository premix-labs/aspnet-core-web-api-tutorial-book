---
title: Style Guide
description: มาตรฐานภาษา รูปแบบโค้ด และรูปแบบเอกสาร
---

# Style Guide

ใช้เอกสารนี้เพื่อให้หนังสือทั้งเล่มมีเสียงเดียวกันและอ่านตามได้จริง

## ภาษาและคำศัพท์

- ใช้ภาษาไทยเป็นหลัก
- คำเทคนิคที่ผู้อ่านจะเจอใน code ให้คงอังกฤษ เช่น Controller, Service, Repository, DTO, DbContext, migration
- ถ้าคำใหม่สำคัญ ให้แปลความหมายก่อนใช้ใน code
- หลีกเลี่ยงคำกว้าง ๆ เช่น "เพิ่มโค้ดนี้" โดยไม่บอกตำแหน่ง
- ระบุระดับของตัวอย่างให้ชัดเมื่อเป็น demo, portfolio-ready หรือ production-oriented

## ชื่อไฟล์และ Path

ใช้ path ที่สัมพันธ์กับ root ของโปรเจกต์ที่ผู้เรียนกำลังอยู่ เช่น:

```text
Backend.Api/Program.cs
Backend.Api/Controllers/UsersController.cs
Backend.Api/Dtos/Auth/LoginRequest.cs
```

ถ้า command ต้องรันจาก solution root หรือ project root ต้องบอกชัดเจน

## Code Blocks

- ใส่ language tag เช่น `csharp`, `json`, `powershell`, `text`
- ไม่ใช้ code block ยาวเกินประมาณ 30 บรรทัดถ้าไม่จำเป็น
- ถ้าต้องแสดงไฟล์เต็ม ให้แบ่ง section และบอกว่าเป็น final shape
- ห้ามใช้ placeholder ที่ทำให้ copy แล้ว compile ไม่ผ่านโดยไม่อธิบาย

## Commands

คำสั่ง Windows PowerShell:

```powershell
dotnet build
dotnet tool run dotnet-ef database update
```

ถ้าคำสั่งต่างจาก macOS/Linux ให้แยก Bash:

```bash
dotnet build
dotnet tool run dotnet-ef database update
```

## Response Examples

ทุก endpoint สำคัญควรมี:

- method และ route
- request body ถ้ามี
- status code สำเร็จ
- response body สำเร็จ
- error status สำคัญ เช่น `400`, `401`, `403`, `404`, `409`

## Security Language

เมื่อสอน auth, token, password, CORS หรือ secret ต้อง:

- บอกข้อจำกัดของ demo
- ห้ามสื่อว่า secret ใน `appsettings.json` เป็น production best practice
- แยก local development, test และ production configuration
- อธิบายว่าการป้องกันจริงต้อง enforce ที่ backend ไม่ใช่แค่ client
