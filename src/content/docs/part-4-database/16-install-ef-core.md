---
title: 16 - ติดตั้ง Entity Framework Core
description: เพิ่ม package และเครื่องมือที่จำเป็นสำหรับใช้ EF Core กับ SQL Server
---

บทนี้เราจะเพิ่ม Entity Framework Core หรือ EF Core เข้ามาในโปรเจกต์ เพื่อให้ API ติดต่อฐานข้อมูลได้โดยเขียน C# เป็นหลัก แทนการเขียน SQL เองทุกคำสั่ง

หลังจบบทนี้ โปรเจกต์จะยังไม่เชื่อม database สำเร็จทันที เพราะเรายังไม่ได้สร้าง `DbContext` และ connection string แต่จะมี package และเครื่องมือที่จำเป็นครบก่อน

## ก่อนเริ่มบทนี้

ให้ตรวจว่าคุณอยู่ที่โฟลเดอร์ root ของโปรเจกต์ API คือโฟลเดอร์ที่มีไฟล์ `Backend.Api.csproj`

```text
Backend.Api/
  Backend.Api.csproj
  Program.cs
```

บทนี้ยังไม่ต้องมี SQL Server ที่เชื่อมต่อได้ และยังไม่ต้องสร้าง database เพราะเราจะติดตั้ง package กับ tool ก่อนเท่านั้น

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

เมื่อทำจบบทนี้ควรมีการเปลี่ยนแปลงหลัก ๆ ดังนี้

```text
Backend.Api.csproj
dotnet-tools.json
```

`Backend.Api.csproj` จะมี EF Core packages ส่วน `dotnet-tools.json` จะบอกว่าโปรเจกต์ใช้ local tool `dotnet-ef`

## EF Core คืออะไร

EF Core คือ Object-Relational Mapper หรือ ORM ของ .NET หน้าที่หลักคือช่วย mapping ระหว่าง C# class กับ table ใน database

ตัวอย่างเช่น class `User` จะถูก map เป็น table `Users` และ property เช่น `Email` จะกลายเป็น column ใน table

## Package ที่ต้องติดตั้ง

ให้เปิด terminal ที่ root ของโปรเจกต์ `Backend.Api` แล้วรันคำสั่งนี้

```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
```

`Microsoft.EntityFrameworkCore.SqlServer` คือ provider สำหรับคุยกับ SQL Server

`Microsoft.EntityFrameworkCore.Design` ใช้สำหรับคำสั่ง design-time เช่นสร้าง migration

## ติดตั้ง dotnet-ef แบบ local tool

`dotnet-ef` คือ command-line tool สำหรับจัดการ migration และ database update

ในหนังสือเล่มนี้เราจะใช้ `dotnet-ef` แบบ local tool ของโปรเจกต์ แทนการติดตั้งแบบ global เพราะจะช่วยให้ version ของ EF Core tools ตรงกับ package ในโปรเจกต์มากกว่า

```powershell
dotnet new tool-manifest
dotnet tool install dotnet-ef --version 10.0.9
```

หลังรันคำสั่งนี้ จะมีไฟล์ `dotnet-tools.json` ในโปรเจกต์ ไฟล์นี้ทำหน้าที่บอกว่าโปรเจกต์นี้ใช้ tool อะไรและใช้ version ไหน

ถ้าโปรเจกต์มี `dotnet-tools.json` อยู่แล้ว ให้ข้าม `dotnet new tool-manifest` แล้วใช้ `dotnet tool restore` เพื่อ restore tool ตาม manifest ได้

## ตรวจสอบว่า dotnet-ef ใช้ได้

```powershell
dotnet tool run dotnet-ef --version
```

ถ้า command ทำงานได้ จะเห็นเลข version ของ EF Core tools

ถ้า version ของ `dotnet-ef` ต่ำกว่า EF Core package ใน `.csproj` อาจเจอ warning หรือ error ตอนสร้าง migration ได้ ดังนั้นควรให้ `dotnet-ef` เป็น version หลักเดียวกับ EF Core package เช่น EF Core 10.x ก็ควรใช้ `dotnet-ef` 10.x

## ตรวจสอบไฟล์ .csproj

หลังติดตั้ง package แล้ว เปิดไฟล์ `Backend.Api.csproj` ควรเห็น `PackageReference` คล้ายแบบนี้

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="..." />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="..." />
</ItemGroup>
```

เลข version อาจไม่เหมือนตัวอย่าง เพราะขึ้นอยู่กับช่วงเวลาที่ติดตั้ง package

## ถ้าเจอ error package incompatible

ให้ตรวจค่า `TargetFramework` ใน `.csproj`

```xml
<TargetFramework>net10.0</TargetFramework>
```

EF Core major version ควรไปทางเดียวกับ .NET ที่ใช้ เช่น project ที่ target `net10.0` ควรใช้ EF Core 10.x

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- ติดตั้ง `Microsoft.EntityFrameworkCore.SqlServer`
- ติดตั้ง `Microsoft.EntityFrameworkCore.Design`
- รัน `dotnet tool run dotnet-ef --version` ได้
- เห็น package ในไฟล์ `.csproj`
