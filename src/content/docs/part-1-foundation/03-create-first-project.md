---
title: 03 - สร้างโปรเจกต์แรก
description: ใช้ dotnet CLI สร้าง ASP.NET Core Web API project แรก
---

บทนี้เราจะสร้าง ASP.NET Core Web API ด้วย `dotnet CLI` เพื่อให้เข้าใจขั้นตอนพื้นฐานก่อนเพิ่ม feature อื่น

ชื่อโปรเจกต์ที่ใช้ตลอดเล่มคือ `Backend.Api`

## วิธีเรียนบทนี้

บทนี้เป็นบทลงมือทำครั้งแรก ให้พิมพ์คำสั่งตามทีละชุดและดูผลลัพธ์ใน terminal ทุกครั้ง

ถ้า port หรือ URL ที่เครื่องคุณได้ไม่ตรงกับตัวอย่างในหนังสือ ให้ใช้ค่าที่เครื่องคุณแสดงจริง ไม่ต้องแก้ให้เหมือนหนังสือ

## ก่อนเริ่มบทนี้

ให้ตรวจว่าบทที่ 2 ผ่านแล้ว โดยอย่างน้อยคำสั่งนี้ต้องแสดง .NET SDK รุ่นที่ใช้เรียนได้:

```powershell
dotnet --version
```

ถ้าคำสั่งนี้ไม่ทำงาน ให้กลับไปแก้การติดตั้ง .NET SDK ก่อน เพราะคำสั่งทั้งหมดในบทนี้พึ่ง `dotnet CLI`

## เตรียมโฟลเดอร์

เปิด terminal แล้วไปยังโฟลเดอร์ที่ต้องการเก็บงาน

Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force -Path D:\code\aspnet-core-web-api-learning
cd D:\code\aspnet-core-web-api-learning
```

macOS/Linux Bash:

```bash
mkdir -p ~/code/aspnet-core-web-api-learning
cd ~/code/aspnet-core-web-api-learning
```

หลังรันคำสั่งนี้ คุณควรอยู่ในโฟลเดอร์สำหรับเรียนแล้ว


## สร้าง Web API project

Windows PowerShell / macOS/Linux Bash ใช้คำสั่งเดียวกัน:

```text
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

Windows PowerShell:

```powershell
Get-ChildItem
```

macOS/Linux Bash:

```bash
ls
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

## ตรวจว่าโปรเจกต์ build ได้

ก่อนรัน server ให้ตรวจว่าโปรเจกต์ compile ผ่าน:

```powershell
dotnet build
```

ผลลัพธ์ที่ควรเห็นคือ build สำเร็จและไม่มี error:

```text
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

จุดที่สำคัญที่สุดคือ `0 Error(s)` ถ้ามี warning ให้จดข้อความไว้ก่อน แต่ยังไปต่อได้ถ้า build สำเร็จ ส่วน error ต้องแก้ก่อนรัน `dotnet run` เพราะ server จะเริ่มไม่ได้ถ้าโปรเจกต์ยัง compile ไม่ผ่าน

## รันโปรเจกต์

รัน API ด้วยคำสั่งนี้

```powershell
dotnet run
```

เมื่อรันสำเร็จ terminal จะแสดงข้อความประมาณนี้

```text
Now listening on: https://localhost:<https-port>
Now listening on: http://localhost:<http-port>
Application started. Press Ctrl+C to shut down.
```

เลข port ในตัวอย่างนี้เป็นเพียงตัวอย่างเท่านั้น เครื่องของคุณอาจแสดงเป็นค่าอื่น

ให้ใช้ URL และ port ที่ terminal ของคุณแสดงจริงเสมอ

ถ้า `dotnet run` แสดงแค่ HTTP เช่น:

```text
Now listening on: http://localhost:<http-port>
```

แปลว่า .NET กำลังใช้ launch profile ชื่อ `http` อยู่ ถ้าต้องการรันด้วย HTTPS profile ให้หยุด server ด้วย `Ctrl+C` แล้วรัน:

```powershell
dotnet run --launch-profile https
```

เมื่อใช้ profile `https` terminal ควรแสดงทั้ง HTTPS และ HTTP ประมาณนี้:

```text
Now listening on: https://localhost:<https-port>
Now listening on: http://localhost:<http-port>
```

ชื่อ profile และ port เหล่านี้มาจากไฟล์ `Properties/launchSettings.json` ซึ่งบทถัดไปจะอธิบายละเอียดขึ้น

ถ้า terminal แสดงข้อความประมาณนี้ แปลว่า API กำลังรันอยู่:

```text
Application started. Press Ctrl+C to shut down.
```

ระหว่างที่ server รันอยู่ terminal นี้จะถูกใช้งานอยู่ ถ้าต้องการรันคำสั่งอื่น ให้เปิด terminal ใหม่ หรือกด `Ctrl+C` เพื่อหยุด server ก่อน

## ทดสอบว่า API เปิดอยู่

เปิด browser หรือ REST Client ไปที่ OpenAPI endpoint

```text
https://localhost:<https-port>/openapi/v1.json
```

ให้แทน `<https-port>` ด้วย port ที่ terminal แสดงจริง

ถ้าเปิดผ่าน `https` แล้ว browser เตือน certificate ให้ยอมรับ certificate สำหรับ local development ได้ หรือใช้ URL แบบ `http` ที่ terminal แสดง

ถ้าต้องการ trust development certificate บนเครื่อง local ให้ใช้คำสั่งนี้:

```powershell
dotnet dev-certs https --trust
```

คำสั่งนี้ใช้สำหรับเครื่อง development เท่านั้น ไม่ใช่ขั้นตอนสำหรับ production server

บน Windows และ macOS คำสั่งนี้มักเปิดหน้าต่างให้ยืนยัน trust certificate ได้โดยตรง ส่วน Linux อาจต้องตั้งค่า certificate trust store ตาม distro ที่ใช้ ถ้าติดปัญหาในบทนี้ ให้ทดสอบผ่าน URL แบบ `http` ที่ terminal แสดงก่อนได้ แล้วค่อยกลับมาตั้งค่า HTTPS ภายหลัง

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

ถ้า `dotnet run` แสดงเฉพาะ HTTP แต่คุณต้องการใช้ HTTPS ให้รัน `dotnet run --launch-profile https` และดู HTTPS port ที่ terminal แสดงจริง

ถ้า browser ขึ้น certificate warning ในเครื่อง local ให้ใช้ `dotnet dev-certs https --trust` หรือทดสอบผ่าน URL แบบ `http` ที่ terminal แสดง

ถ้า `dotnet build` ไม่ผ่าน ให้ดูบรรทัด error แรกก่อน เพราะ error ถัด ๆ ไปอาจเป็นผลต่อเนื่องจาก error แรก

ถ้าเผลอสร้างโปรเจกต์โดยไม่ได้ใส่ `--use-controllers` ให้สร้างใหม่ตามคำสั่งในบทนี้ จะง่ายกว่าปรับ template ที่ผิดตั้งแต่ต้น

## แบบฝึกหัด

ลองทำตามนี้ก่อนเข้าบท Controller:

1. สร้างโปรเจกต์ `Backend.Api` ด้วย `--use-controllers`
2. รัน `dotnet build` ให้ผ่าน
3. รัน `dotnet run` แล้วจด URL ที่เครื่องของคุณแสดง
4. ถ้าต้องการลอง HTTPS ให้รัน `dotnet run --launch-profile https` แล้วจด HTTPS URL ที่เครื่องของคุณแสดง
5. เปิด OpenAPI endpoint ใน browser หรือ REST Client
6. หยุด server ด้วย `Ctrl+C`

## แนวคำตอบโดยย่อ

หลังทำสำเร็จ คุณควรมีโฟลเดอร์ `Backend.Api` ที่มีไฟล์ `Program.cs`, `Backend.Api.csproj`, `appsettings.json` และโฟลเดอร์ `Controllers`

คำสั่ง `dotnet build` ควรแสดง `Build succeeded.` และ `dotnet run` ควรแสดง URL ที่ API กำลัง listen อยู่ เช่น `http://localhost:<http-port>` หรือ `https://localhost:<https-port>` ขึ้นอยู่กับเครื่องของคุณ

## Checkpoint

เมื่อจบบทนี้ คุณควรทำได้ครบตามนี้

- สร้างโปรเจกต์ `Backend.Api`
- ใช้ `--use-controllers` ตอนสร้างโปรเจกต์
- เข้าใจไฟล์เริ่มต้นที่ template สร้างให้
- รันโปรเจกต์ด้วย `dotnet run`
- รู้ว่าถ้าต้องการบังคับใช้ HTTPS profile ให้ใช้ `dotnet run --launch-profile https`
- เปิด OpenAPI endpoint ได้
- หยุด server ด้วย `Ctrl+C`
