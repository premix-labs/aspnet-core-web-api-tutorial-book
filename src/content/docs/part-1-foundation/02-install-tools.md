---
title: 02 - ติดตั้งเครื่องมือที่จำเป็น
description: เตรียมเครื่องมือสำหรับพัฒนา ASP.NET Core Web API บนเครื่อง local
---

ก่อนเริ่มเขียน code เราต้องเตรียมเครื่องมือสำหรับสร้าง รัน ทดสอบ debug และจัดการโปรเจกต์ ASP.NET Core Web API

บทนี้เน้นเครื่องมือที่ใช้จริงตลอดเล่ม ถ้าคำสั่ง checkpoint ท้ายบทผ่านทั้งหมด คุณจะพร้อมทำตามบทต่อไป

## วิธีเรียนบทนี้

บทนี้ให้ทำตามทีละเครื่องมือ อย่าติดตั้งทุกอย่างแล้วค่อยตรวจทีเดียว เพราะถ้าเครื่องมือใดมีปัญหาจะหาสาเหตุยาก

หลังติดตั้งแต่ละเครื่องมือ ให้รันคำสั่งตรวจ version ทันที ถ้าคำสั่งไม่เจอ ให้แก้ PATH หรือ restart terminal ก่อนข้ามไปเครื่องมือถัดไป

## เครื่องมือที่ใช้ในหนังสือ

- .NET SDK สำหรับสร้างและรันโปรเจกต์
- Visual Studio Code หรือ Visual Studio สำหรับเขียน code
- C# Dev Kit หรือ C# extension สำหรับ IntelliSense และ debug
- Git สำหรับเก็บประวัติ code
- REST Client หรือ Postman สำหรับทดสอบ HTTP API
- Docker Desktop สำหรับรัน SQL Server และทดสอบ deployment
- SQL Server ผ่าน Docker container สำหรับภาคฐานข้อมูล

## แนวทางหลักของหนังสือ

หนังสือนี้ใช้ **Visual Studio Code + PowerShell + dotnet CLI** เป็นเส้นทางหลัก เพราะทำให้เห็นคำสั่งที่อยู่เบื้องหลังชัดเจน และใช้ได้ใกล้เคียงกันบนหลายระบบปฏิบัติการ

ถ้าคุณใช้ Visual Studio ก็เรียนตามได้เหมือนกัน แต่คำสั่งในหนังสือจะยังอ้างอิง `dotnet` CLI เป็นหลัก เช่น `dotnet run`, `dotnet build`, `dotnet test` และ `dotnet publish`

แนวคิดสำคัญคือ IDE เป็นเครื่องมือช่วยเขียน code แต่โปรเจกต์ ASP.NET Core จริงควรรันและตรวจสอบผ่าน command line ได้เสมอ

## ติดตั้ง .NET SDK

หนังสือนี้ใช้ .NET 10 และ target framework `net10.0`

หลังติดตั้งแล้วให้เปิด PowerShell แล้วตรวจสอบด้วยคำสั่งนี้

```powershell
dotnet --version
```

ผลลัพธ์ควรเป็นเลขเวอร์ชันของ SDK เช่น

```text
10.0.300
```

ตรวจสอบ SDK ทั้งหมดในเครื่อง:

```powershell
dotnet --list-sdks
```

ตรวจสอบ runtime ทั้งหมด:

```powershell
dotnet --list-runtimes
```

ถ้า `dotnet` ไม่รู้จักคำสั่ง ให้ปิด terminal แล้วเปิดใหม่ ถ้ายังไม่ได้ให้ตรวจว่า .NET SDK ถูกติดตั้งและอยู่ใน `PATH` แล้วหรือยัง

## SDK กับ Runtime ต่างกันอย่างไร

`SDK` ใช้สำหรับพัฒนา เช่น `dotnet new`, `dotnet build`, `dotnet test`, `dotnet publish`

`Runtime` ใช้สำหรับรัน application ที่ build แล้ว

เครื่องนักพัฒนาควรติดตั้ง SDK เพราะ SDK รวม runtime มาด้วย แต่เครื่อง production บางแบบอาจใช้ runtime image อย่างเดียว เช่น Docker image `mcr.microsoft.com/dotnet/aspnet`

## ติดตั้ง Visual Studio Code extensions

ถ้าใช้ Visual Studio Code ให้ติดตั้ง extension เหล่านี้

- C# Dev Kit
- C#
- REST Client
- Docker

หลังติดตั้งแล้วให้ปิดและเปิด Visual Studio Code ใหม่ เพื่อให้ extension โหลด language server และ runtime ครบ

ถ้า Visual Studio Code แจ้งว่าไม่มี JDK สำหรับ Java extension ไม่เกี่ยวกับการ compile โปรเจกต์ ASP.NET Core โดยตรง แต่ถ้าคุณใช้ extension Java ร่วมด้วยสามารถตั้งค่า JDK แยกต่างหากได้

## ถ้าใช้ Visual Studio

ถ้าใช้ Visual Studio ให้ติดตั้ง workload เหล่านี้ผ่าน Visual Studio Installer:

- ASP.NET and web development
- .NET desktop development ถ้าต้องการเครื่องมือ .NET เพิ่มเติม

หลังติดตั้งแล้วสามารถเปิดไฟล์ solution หรือ project ได้จาก Visual Studio แต่ยังแนะนำให้เปิด terminal แล้วตรวจว่า `dotnet` ใช้งานได้จาก command line:

```powershell
dotnet --version
dotnet --list-sdks
```

คำสั่ง `dotnet build` และ `dotnet run` จะเริ่มใช้หลังจากสร้างโปรเจกต์ในบทถัดไป ตอนนี้ยังไม่ต้องรัน เพราะยังไม่มีไฟล์ project ให้ build หรือ run

เหตุผลที่ต้องตรวจ command line คือเมื่อขึ้น production, CI/CD หรือ Docker เราจะพึ่งคำสั่งเหล่านี้มากกว่าเมนูของ IDE

## ติดตั้ง Git

ใช้ Git เพื่อเก็บประวัติการทำงานของโปรเจกต์ และใช้ร่วมกับ GitHub

ตรวจสอบ Git ด้วยคำสั่งนี้

```powershell
git --version
```

ถ้าคำสั่งทำงาน คุณจะเห็นเวอร์ชัน เช่น

```text
git version 2.x.x
```

## ติดตั้ง REST Client หรือ Postman

ช่วงแรกเราจะทดสอบ API ด้วยไฟล์ `.http` เพราะเก็บ request ไว้ใน repo ได้ และกลับมารันทดสอบซ้ำได้ง่าย

ถ้าใช้ VS Code ให้ติดตั้ง extension `REST Client`

ถ้าใช้ Postman ให้สร้าง collection แยกไว้สำหรับโปรเจกต์นี้ และตั้ง variable ชื่อ `baseUrl`

## ติดตั้ง Docker Desktop

ในภาคฐานข้อมูลและ deployment เราจะใช้ Docker เพื่อรัน SQL Server และ API ใน container

ตรวจสอบ Docker:

```powershell
docker version
```

ตรวจสอบ Docker Compose:

```powershell
docker compose version
```

ถ้า Docker ยังไม่เปิด คำสั่งเหล่านี้จะ error ให้เปิด Docker Desktop ก่อนแล้วลองใหม่

## โฟลเดอร์สำหรับงาน

แนะนำให้สร้างโฟลเดอร์แยกสำหรับโปรเจกต์ฝึก เช่น

```text
D:\code\aspnet-core-web-api-learning
```

Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force -Path D:\code\aspnet-core-web-api-learning
```

macOS/Linux Bash:

```bash
mkdir -p ~/code/aspnet-core-web-api-learning
```

หลีกเลี่ยงการสร้างโปรเจกต์ไว้ใน path ที่ซับซ้อนเกินไปหรือมี permission แปลก ๆ เพราะอาจทำให้ Docker, Git หรือ terminal ใช้งานยากขึ้น

## Terminal และ path ในแต่ละระบบ

ตัวอย่างในหนังสือใช้ PowerShell บน Windows เป็นหลัก:

```powershell
cd D:\code\aspnet-core-web-api-learning
```

ถ้าใช้ macOS หรือ Linux แนวคิดเหมือนกัน แต่ path จะต่างออกไป เช่น:

```bash
cd ~/code/aspnet-core-web-api-learning
```

คำสั่ง `dotnet`, `git`, `docker` และ `docker compose` ใช้เหมือนกันเกือบทั้งหมด สิ่งที่ต่างกันบ่อยคือรูปแบบ path และ shell ที่ใช้รันคำสั่ง

## ปัญหาที่เจอบ่อย

ถ้า `dotnet` ไม่รู้จักคำสั่ง ให้ปิด terminal แล้วเปิดใหม่ ถ้ายังไม่ได้ให้ตรวจว่า .NET SDK ถูกเพิ่มเข้า `PATH` แล้ว

ถ้า `dotnet --version` แสดงเวอร์ชันเก่ากว่าที่ต้องการ ให้ตรวจ `dotnet --list-sdks` เพื่อดู SDK ทั้งหมดที่ติดตั้งในเครื่อง

ถ้า Docker command error ให้เปิด Docker Desktop ก่อน แล้วรอจนสถานะเป็น running จากนั้นลอง `docker version` อีกครั้ง

ถ้า REST Client ใช้งานไม่ได้ ให้ตรวจว่าเปิดไฟล์นามสกุล `.http` อยู่ และติดตั้ง extension ชื่อ `REST Client` แล้ว

ถ้าใช้ Visual Studio แล้วรันได้ แต่ command line รันไม่ได้ ให้แก้ command line ก่อน เพราะบทเรียนถัดไปจะใช้คำสั่ง `dotnet` เป็นหลัก

## แบบฝึกหัด

ก่อนเริ่มสร้างโปรเจกต์ ลองทำสิ่งเหล่านี้ในเครื่องของคุณ:

1. เปิด terminal แล้วตรวจเวอร์ชันของ .NET SDK
2. ตรวจว่ามี Git ติดตั้งอยู่หรือไม่
3. ตรวจว่า Docker Desktop เปิดใช้งานได้หรือไม่
4. เปิด editor ที่จะใช้ตลอดเล่ม แล้วสร้างโฟลเดอร์สำหรับเรียน

## แนวคำตอบโดยย่อ

เครื่องพร้อมเรียนต่อเมื่อคำสั่งเหล่านี้ทำงานได้:

```powershell
dotnet --version
git --version
docker version
docker compose version
```

ถ้าคำสั่งใดไม่ผ่าน ให้แก้เครื่องมือส่วนนั้นก่อน อย่าข้ามไปสร้างโปรเจกต์ เพราะปัญหาจะตามไปในบทฐานข้อมูลและ Docker

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจสอบว่าคำสั่งเหล่านี้ทำงานได้

```powershell
dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
git --version
docker version
docker compose version
```

ถ้าคำสั่งใดไม่ผ่าน ให้แก้เครื่องมือส่วนนั้นก่อน เพราะบทต่อไปจะเริ่มสร้างโปรเจกต์จริงแล้ว
