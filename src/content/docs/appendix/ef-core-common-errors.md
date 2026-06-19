---
title: ปัญหาที่พบบ่อยใน EF Core
description: รวม error ที่มักเจอเมื่อใช้ Entity Framework Core
---

## dotnet ef ไม่ทำงาน

ตรวจว่า restore local tool แล้ว

```powershell
dotnet tool restore
dotnet tool run dotnet-ef --version
```

ถ้ายังไม่มี tool manifest ให้กลับไปทำตามบท 16 ก่อน โดยใช้ `dotnet new tool-manifest` และ `dotnet tool install dotnet-ef --version 10.0.9`

## เชื่อม database ไม่ได้

ตรวจ connection string, port, username, password และสถานะของ SQL Server

## Migration ไม่ตรงกับ database

ตรวจว่า migration ล่าสุดถูกสร้างแล้ว และรันคำสั่งนี้

```powershell
dotnet tool run dotnet-ef database update
```
