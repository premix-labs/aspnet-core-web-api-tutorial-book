---
title: 04 - เข้าใจโครงสร้างไฟล์ของ ASP.NET Core
description: อธิบายไฟล์สำคัญใน ASP.NET Core Web API project
---

หลังสร้างโปรเจกต์แล้ว เราต้องเข้าใจว่าไฟล์แต่ละตัวทำหน้าที่อะไร เพราะเวลาเพิ่ม feature หรือแก้ bug จะต้องรู้ว่าควรเริ่มดูจากจุดไหน

บทนี้ยังไม่แก้ code เยอะ แต่จะอ่านโครงสร้างให้เข้าใจก่อน

## โครงสร้างเริ่มต้น

โปรเจกต์ที่สร้างด้วย `--use-controllers` จะมีไฟล์หลักประมาณนี้

```text
Backend.Api/
  Controllers/
    WeatherForecastController.cs
  Properties/
    launchSettings.json
  Program.cs
  Backend.Api.csproj
  Backend.Api.http
  appsettings.json
  appsettings.Development.json
```

ไฟล์จริงอาจต่างกันเล็กน้อยตาม template และ SDK ที่ใช้ แต่แนวคิดหลักเหมือนกัน

## Program.cs

`Program.cs` คือจุดเริ่มต้นของ application ใช้ตั้งค่า service, middleware และ endpoint

ตัวอย่าง code ที่มักเจอ:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

สิ่งที่ควรจำ:

- `builder.Services...` คือการลงทะเบียน service ที่ระบบต้องใช้
- `app.Use...` คือการเพิ่ม middleware เข้า pipeline
- `app.Map...` คือการประกาศ endpoint
- `app.Run()` คือเริ่มรัน application

ในบทหลัง ๆ เราจะกลับมาเพิ่ม database, JWT, error handler และ service ของเราในไฟล์นี้

## Controllers

โฟลเดอร์ `Controllers` เก็บ class ที่รับ HTTP request

ตัวอย่างเช่น `UsersController` จะรับ request ที่เกี่ยวกับ user เช่น

```text
GET /api/users
POST /api/users
GET /api/users/{id}
```

Controller ไม่ควรเป็นที่รวมทุกอย่าง เมื่อระบบใหญ่ขึ้น Controller ควรเรียก service แทนการเขียน logic ยาว ๆ เอง

## .csproj

ไฟล์ `Backend.Api.csproj` คือไฟล์ project ของ .NET ใช้กำหนด target framework และ package reference

ตัวอย่าง:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

เมื่อเราติดตั้ง package เช่น EF Core หรือ JWT package ข้อมูลจะถูกเพิ่มเข้ามาในไฟล์นี้

```powershell
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

หลังรันคำสั่งนี้ `.csproj` จะมี `PackageReference` เพิ่มขึ้น

## appsettings.json

`appsettings.json` ใช้เก็บ configuration ของระบบ เช่น connection string, JWT settings, logging level และค่าอื่นที่ไม่ควร hard-code ไว้ใน C#

ตัวอย่าง:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

บทหลัง ๆ เราจะเพิ่ม section เช่น `ConnectionStrings` และ `Jwt`

## appsettings.Development.json

ไฟล์นี้ใช้ override configuration เฉพาะตอนรันใน environment แบบ Development

ตัวอย่างเช่นในเครื่อง local เราอาจเปิด log จาก EF Core ให้ละเอียดขึ้น แต่ใน production ไม่ควรเปิดละเอียดเท่ากัน

แนวคิดสำคัญคือ configuration แยกตาม environment ได้ โดยไม่ต้องแก้ C# code

## launchSettings.json

`Properties/launchSettings.json` ใช้กำหนด profile การรันในเครื่อง local เช่น URL, port และ environment

ตัวอย่างค่าที่เจอบ่อย:

```json
{
  "profiles": {
    "https": {
      "applicationUrl": "https://localhost:7001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

ไฟล์นี้มีผลกับการรันในเครื่อง development เป็นหลัก ไม่ใช่ไฟล์ที่ใช้ตั้งค่า production

## Backend.Api.http

ไฟล์ `.http` ใช้เก็บ request สำหรับทดสอบ API จาก Visual Studio Code หรือ Visual Studio

ช่วงแรกเราจะใช้ไฟล์นี้ทดสอบ `GET /api/users` และ CRUD endpoint หลังจากนั้นจะเพิ่ม request สำหรับ register, login และ admin endpoint

## โครงสร้างที่จะค่อย ๆ เพิ่ม

เมื่อเรียนต่อไป โปรเจกต์จะค่อย ๆ มีโครงสร้างประมาณนี้

```text
Backend.Api/
  Controllers/
  Data/
  Dtos/
  Exceptions/
  Models/
  Options/
  Repositories/
  Services/
  Program.cs
```

อย่าเพิ่งสร้างทุกโฟลเดอร์พร้อมกันถ้ายังไม่เข้าใจ เราจะเพิ่มทีละส่วนตามบทเรียน

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรอธิบายหน้าที่ของไฟล์เหล่านี้ได้

- `Program.cs`
- `Controllers`
- `*.csproj`
- `appsettings.json`
- `appsettings.Development.json`
- `launchSettings.json`
- `.http`
