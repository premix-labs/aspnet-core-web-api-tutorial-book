---
title: 06 - สร้าง Controller แรก
description: เริ่มสร้าง Controller และ endpoint แรกใน ASP.NET Core Web API
---

Controller คือ class ที่รับ HTTP request แล้วส่ง HTTP response กลับไป ใน ASP.NET Core เรามักวาง Controller ไว้ในโฟลเดอร์ `Controllers`

บทนี้เราจะสร้าง `UsersController` ตัวแรก แล้วทดสอบ `GET /api/users`

## ตรวจว่าโปรเจกต์ใช้ Controller แล้ว

เปิด `Program.cs` แล้วตรวจว่ามีสองส่วนนี้

```csharp
// Register controller services.
builder.Services.AddControllers();

// ...

// Map controller routes into the HTTP pipeline.
app.MapControllers();
```

ถ้าไม่มี `AddControllers()` ระบบจะไม่ลงทะเบียน Controller service

ถ้าไม่มี `MapControllers()` request จะไม่ถูก map ไปยัง Controller

โปรเจกต์ที่สร้างด้วย `dotnet new webapi --use-controllers` ควรมีให้แล้ว

## ลบ Controller ตัวอย่างจาก template

ถ้า template สร้าง `WeatherForecastController.cs` มาให้ สามารถลบไฟล์เหล่านี้ได้ เพราะเราจะสร้าง endpoint ของระบบเราเอง

```text
Controllers/WeatherForecastController.cs
WeatherForecast.cs
```

ถ้ายังไม่อยากลบก็ไม่เป็นไร แต่ในหนังสือจะใช้ `UsersController` เป็นหลัก

## สิ่งที่จะใช้ใน Controller แรก

ก่อนสร้างไฟล์ ให้รู้จักคำสำคัญที่จะเห็นใน code ก่อน:

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `using Microsoft.AspNetCore.Mvc;` | นำ class และ attribute สำหรับเขียน Controller มาใช้ |
| `namespace Backend.Api.Controllers;` | ระบุตำแหน่งทาง logical ของ class ในโปรเจกต์ |
| `[ApiController]` | บอก ASP.NET Core ว่า class นี้เป็น API controller |
| `[Route("api/[controller]")]` | กำหนด route หลักของ controller |
| `ControllerBase` | base class สำหรับ API controller ที่ไม่ render view |
| `[HttpGet]` | บอกว่า method นี้รับ HTTP GET |
| `IActionResult` | return type สำหรับ response หลายแบบ เช่น `Ok`, `NotFound`, `BadRequest` |
| `Ok(value)` | ตอบ `200 OK` พร้อมข้อมูลเป็น JSON |

เมื่อเห็น code ในหัวข้อถัดไป ให้มองว่าเรากำลังประกาศ controller หนึ่งตัวที่เปิด endpoint `GET /api/users`

## สร้าง UsersController

ให้รันคำสั่งจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force -Path Controllers
New-Item -ItemType File -Path Controllers/UsersController.cs
```

macOS/Linux Bash:

```bash
mkdir -p Controllers
touch Controllers/UsersController.cs
```

จากนั้นเปิดไฟล์นี้

```text
Controllers/UsersController.cs
```

ใส่ code นี้

```csharp
using Microsoft.AspNetCore.Mvc;

namespace Backend.Api.Controllers;

// Marks this class as a Web API controller.
[ApiController]
// Base route becomes /api/users because the class name is UsersController.
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    // Handles GET /api/users.
    [HttpGet]
    public IActionResult GetUsers()
    {
        // Temporary sample data. Later chapters will move data to services and a database.
        var users = new[]
        {
            new { Id = 1, Email = "admin@example.com" },
            new { Id = 2, Email = "user@example.com" }
        };

        // Return 200 OK with JSON body.
        return Ok(users);
    }
}
```

## อธิบาย code

`using Microsoft.AspNetCore.Mvc;` ทำให้ใช้ class และ attribute ของ ASP.NET Core MVC ได้ เช่น `ControllerBase`, `ApiController`, `HttpGet` และ `IActionResult`

`namespace Backend.Api.Controllers;` คือ namespace ของไฟล์นี้ ควรตรงกับชื่อโปรเจกต์และโฟลเดอร์

`[ApiController]` บอก ASP.NET Core ว่า class นี้เป็น API controller และเปิด behavior บางอย่างที่เหมาะกับ API เช่น validation response อัตโนมัติ

`[Route("api/[controller]")]` กำหนด route หลักของ controller โดย `[controller]` จะถูกแทนด้วยชื่อ controller ที่ตัดคำว่า `Controller` ออก ดังนั้น `UsersController` จะกลายเป็น `api/users`

`ControllerBase` คือ base class สำหรับ API controller ที่ไม่ต้อง render view

`[HttpGet]` บอกว่า method นี้รับ HTTP GET

`IActionResult` คือชนิด return ที่ยืดหยุ่น สามารถคืน `Ok`, `NotFound`, `BadRequest` และ response แบบอื่นได้

`Ok(users)` สร้าง response status `200 OK` พร้อม body เป็น JSON

## รัน API

รันโปรเจกต์

```powershell
dotnet run
```

ดู URL ที่ terminal แสดง เช่น

```text
https://localhost:<https-port>
http://localhost:<http-port>
```

URL ด้านบนเป็นเพียงตัวอย่าง เครื่องของคุณอาจแสดงเป็นค่าอื่น เช่น `http://localhost:5156` และ `https://localhost:7127`

จากนั้นเปิด endpoint นี้ โดยเปลี่ยน host และ port ให้ตรงกับ terminal ของคุณ

```text
GET http://localhost:5156/api/users
```

ผลลัพธ์ที่คาดหวังคือ JSON array

```json
[
  {
    "id": 1,
    "email": "admin@example.com"
  },
  {
    "id": 2,
    "email": "user@example.com"
  }
]
```

ASP.NET Core จะแปลง property จาก `Id` เป็น `id` และ `Email` เป็น `email` ตอน serialize เป็น JSON ตามค่า default

## ถ้า endpoint เปิดไม่ได้

ตรวจทีละข้อ:

- `dotnet run` ยังรันอยู่หรือไม่
- ใช้ port ตรงกับ terminal หรือไม่
- path เป็น `/api/users` ไม่ใช่ `/users`
- `Program.cs` มี `AddControllers()` และ `MapControllers()` หรือไม่
- ชื่อ class ลงท้ายด้วย `Controller` หรือไม่
- file อยู่ใน namespace `Backend.Api.Controllers` หรือไม่

## Checkpoint

เมื่อจบบทนี้ คุณควรทำได้

- สร้าง `UsersController`
- เข้าใจ `[ApiController]`, `[Route]`, `[HttpGet]`
- เข้าใจว่า `[controller]` แปลงเป็น `users` ได้อย่างไร
- รัน `GET /api/users` แล้วได้ JSON response
