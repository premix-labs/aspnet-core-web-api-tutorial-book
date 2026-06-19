---
title: 03 - สร้างโปรเจกต์แรก
description: ใช้ dotnet CLI สร้าง ASP.NET Core Web API project แรก
---

บทนี้เราจะสร้าง ASP.NET Core Web API ด้วย `dotnet CLI` เพื่อให้เข้าใจขั้นตอนพื้นฐานก่อนเพิ่ม feature อื่น

ชื่อโปรเจกต์ที่ใช้ตลอดเล่มคือ `Backend.Api`

## เตรียมโฟลเดอร์

เปิด PowerShell แล้วไปยังโฟลเดอร์ที่ต้องการเก็บงาน

ตัวอย่าง:

```powershell
cd D:\code\aspnet-core-web-api-learning
```

ถ้าโฟลเดอร์ยังไม่มี ให้สร้างก่อน

```powershell
mkdir D:\code\aspnet-core-web-api-learning
cd D:\code\aspnet-core-web-api-learning
```

## สร้าง Web API project

รันคำสั่งนี้

```powershell
dotnet new webapi -n Backend.Api --use-controllers
cd Backend.Api
```

ความหมายของแต่ละส่วน:

- `dotnet new webapi` สร้างโปรเจกต์จาก template Web API
- `-n Backend.Api` ตั้งชื่อโปรเจกต์และโฟลเดอร์
- `--use-controllers` ให้ template สร้างโปรเจกต์แบบ Controller
- `cd Backend.Api` เข้าไปในโฟลเดอร์โปรเจกต์

ถ้าไม่ใส่ `--use-controllers` ใน .NET รุ่นใหม่บางเวอร์ชัน project อาจถูกสร้างเป็น Minimal API ซึ่งยังทำ API ได้เหมือนกัน แต่โครงสร้างไฟล์และวิธีเขียน endpoint จะไม่ตรงกับบทเรียนต่อจากนี้

## ดูไฟล์ที่ถูกสร้าง

รันคำสั่งนี้

```powershell
Get-ChildItem
```

คุณควรเห็นไฟล์และโฟลเดอร์ประมาณนี้

```text
Controllers/
Properties/
Program.cs
Backend.Api.csproj
Backend.Api.http
appsettings.json
appsettings.Development.json
```

ถ้ามี `WeatherForecastController.cs` แสดงว่า template สร้างตัวอย่าง Controller มาให้แล้ว เราจะลบหรือแทนที่ด้วย `UsersController` ในบทถัดไป

## รันโปรเจกต์

รัน API ด้วยคำสั่งนี้

```powershell
dotnet run
```

เมื่อรันสำเร็จ terminal จะแสดงข้อความประมาณนี้

```text
Now listening on: https://localhost:7001
Now listening on: http://localhost:5000
Application started. Press Ctrl+C to shut down.
```

เลข port อาจไม่เหมือนกันในแต่ละเครื่อง ให้ใช้ค่าที่เครื่องของคุณแสดงจริง

## ทดสอบว่า API เปิดอยู่

เปิด browser หรือ REST Client ไปที่ OpenAPI endpoint

```text
https://localhost:7001/openapi/v1.json
```

ถ้า port ของคุณไม่ใช่ `7001` ให้เปลี่ยนตาม terminal

ถ้าเปิดผ่าน `https` แล้ว browser เตือน certificate ให้ยอมรับ certificate สำหรับ local development ได้ หรือใช้ URL แบบ `http` ที่ terminal แสดง

## หยุดโปรเจกต์

กลับไปที่ terminal แล้วกด

```text
Ctrl+C
```

การหยุด server สำคัญ เพราะถ้าเปิดค้างไว้หลายตัว อาจเกิดปัญหา port ชนกันในบทต่อไป

## ปัญหาที่เจอบ่อย

ถ้าเจอ error ว่า port ถูกใช้แล้ว ให้หยุด process เดิมก่อน หรือปิด terminal ที่รัน project ค้างอยู่

ถ้า `dotnet run` restore package นานในครั้งแรก เป็นเรื่องปกติ เพราะ .NET ต้องดาวน์โหลด package ที่จำเป็น

ถ้าเปิด OpenAPI ไม่ได้ ให้ดู URL จาก terminal อีกครั้ง อย่าเดา port เอง

## Checkpoint

เมื่อจบบทนี้ คุณควรทำได้ครบตามนี้

- สร้างโปรเจกต์ `Backend.Api`
- ใช้ `--use-controllers` ตอนสร้างโปรเจกต์
- เข้าใจไฟล์เริ่มต้นที่ template สร้างให้
- รันโปรเจกต์ด้วย `dotnet run`
- เปิด OpenAPI endpoint ได้
- หยุด server ด้วย `Ctrl+C`
