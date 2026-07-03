---
title: หลักการสอน
description: มาตรฐานการเขียนบทเรียน ASP.NET Core Web API ให้ทำตามได้จริง
---

# หลักการสอน

หนังสือเล่มนี้ต้องสอนให้ผู้เรียนสร้าง backend API ที่รันได้จริง โดยเข้าใจเหตุผลของโค้ด ไม่ใช่แค่พิมพ์ตาม

## Definition of Done ของแต่ละบท

บทหนึ่งจะถือว่าพร้อมสอนเมื่อมีครบ:

- เป้าหมายของบท
- prerequisites
- ไฟล์ที่จะสร้างหรือแก้
- concept ใหม่ก่อนใช้
- คำสั่งที่ copy ได้
- code ทีละส่วน
- expected result
- common errors
- checkpoint

## ลำดับการสอนโค้ด

สอนเป็นชั้น:

1. สร้าง folder/file
2. เพิ่ม using
3. เพิ่ม class, record หรือ interface
4. เพิ่ม property
5. เพิ่ม method ทีละ method
6. ต่อ dependency injection หรือ configuration
7. ต่อ controller/route
8. ทดสอบผล

## ASP.NET Core Rules

- Controller ควรรับ HTTP request และแปลง response
- Service ควรเก็บ business logic
- Repository ควรซ่อน data access
- DTO ควรเป็น API contract ไม่ใช่ entity ภายใน
- `Program.cs` ต้องลงทะเบียน service ก่อน `builder.Build()`
- EF Core command ควรใช้ local tool ผ่าน `dotnet tool run dotnet-ef`
- บท production hardening ต้องบอกข้อจำกัดและความเสี่ยง

## Progressive Path

ต้องรักษาลำดับการเรียน:

- บทต้นไม่ควรใช้ field หรือ class จากบทท้าย
- ถ้า final project ต่างจาก progressive project ต้องอธิบายเหตุผล
- validation project คือหลักฐานว่าผู้อ่านทำตามได้จริง
- final project คือ reference ของ end-state

## Verification Gate

ก่อนถือว่าบทพร้อม ต้องรันคำสั่งตาม scope:

```powershell
npm run build
```

ถ้าแตะ example code:

```powershell
dotnet test -c Release
dotnet publish -c Release
```

ถ้าแตะ Docker:

```powershell
docker compose config
```

