---
title: 44 - OpenAPI และเอกสาร API
description: สร้างเอกสาร endpoint ให้ client และทีมใช้งานผ่าน OpenAPI
---

OpenAPI คือ specification สำหรับอธิบาย HTTP API เช่น endpoint, method, request body, response และ schema

ASP.NET Core มี built-in OpenAPI support ผ่าน `Microsoft.AspNetCore.OpenApi` สำหรับสร้าง OpenAPI document ส่วน interactive UI เช่น Swagger UI หรือ Scalar เป็นส่วนเสริมที่สามารถเพิ่มภายหลังได้

## วิธีเรียนบทนี้

บทนี้จะทำให้ API มีเอกสาร contract ที่เครื่องอ่านได้:

1. ติดตั้ง `Microsoft.AspNetCore.OpenApi`
2. เปิด `AddOpenApi`
3. map `/openapi/v1.json` เฉพาะ development
4. เพิ่ม `ProducesResponseType` ใน controller สำคัญ
5. เปิด XML documentation ถ้าต้องการรายละเอียดเพิ่ม
6. ทดสอบ URL OpenAPI ด้วย port จริงของเครื่อง

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| OpenAPI | specification ที่อธิบาย API contract |
| `Microsoft.AspNetCore.OpenApi` | package built-in ของ ASP.NET Core สำหรับสร้าง document |
| `AddOpenApi()` | register OpenAPI service |
| `MapOpenApi()` | เปิด endpoint `/openapi/v1.json` |
| `ProducesResponseType` | metadata ที่บอก status code และ response type |
| XML documentation | comment ที่เอาไปเติมรายละเอียดในเอกสาร API ได้ |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Backend.Api.csproj
Program.cs
Controllers/AuthController.cs
Controllers/AdminUsersController.cs
```

## ขั้นที่ 1: ติดตั้ง OpenAPI package

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet add package Microsoft.AspNetCore.OpenApi
```

ถ้า template ของคุณมี package นี้อยู่แล้ว คำสั่งจะอัปเดตหรือแจ้งว่า reference มีอยู่แล้ว

## ขั้นที่ 2: เพิ่ม AddOpenApi

เปิด `Program.cs`

เพิ่ม service นี้ก่อน `builder.Build()`:

```csharp
builder.Services.AddOpenApi();
```

`AddOpenApi()` เตรียม service ที่ใช้สร้าง OpenAPI document จาก endpoint metadata ใน application

## ขั้นที่ 3: เปิด OpenAPI endpoint เฉพาะ development

ใน `Program.cs` เพิ่มหลังสร้าง `app` แล้ว:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

เราเปิดเฉพาะ development ก่อน เพราะเอกสาร API อาจเปิดเผย endpoint ภายใน เช่น admin endpoint

## ขั้นที่ 4: ทดสอบ OpenAPI document

รัน API:

```powershell
dotnet run
```

เปิด URL นี้ ถ้าใช้ HTTPS port ตามตัวอย่างของหนังสือ:

```text
https://localhost:7127/openapi/v1.json
```

ถ้าเครื่องคุณแสดง port อื่น ให้ใช้ port ที่ `dotnet run`, Visual Studio หรือ Visual Studio Code แสดงจริง เช่น `http://localhost:5156/openapi/v1.json`

## ขั้นที่ 5: เพิ่ม response metadata ใน AuthController

OpenAPI จะอ่าน metadata จาก attribute ใน controller ได้ เราควรใส่ response type สำคัญให้ชัดขึ้น

ตัวอย่าง `AuthController`:

```csharp
[HttpPost("login")]
[ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> Login(LoginRequest request)
```

ถ้า action คืน `Ok(response)` metadata ด้านบนช่วยให้ OpenAPI รู้ว่า response สำเร็จเป็น `LoginResponse`

## ขั้นที่ 6: เพิ่ม response metadata ใน AdminUsersController

ตัวอย่าง `GET /api/admin/users`:

```csharp
[HttpGet]
[ProducesResponseType(typeof(PagedResponse<AdminUserResponse>),
    StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails),
    StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails),
    StatusCodes.Status403Forbidden)]
public async Task<IActionResult> GetUsers([FromQuery] AdminUserQuery query)
```

`ProducesResponseType` ไม่ได้เปลี่ยน behavior ของ endpoint แต่ช่วยให้เอกสาร API ชัดขึ้น

## ขั้นที่ 7: เปิด XML Documentation

ถ้าต้องการให้ OpenAPI มี summary/description จาก XML comments ให้เปิด generation ใน `.csproj`

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

จากนั้นเพิ่ม XML comments ให้ DTO หรือ action ที่สำคัญ:

```csharp
/// <summary>
/// Login with email and password.
/// </summary>
public class LoginRequest
{
}
```

ไม่จำเป็นต้องใส่ XML comment ทุก class ตั้งแต่เริ่ม ให้เริ่มจาก request/response สำคัญก่อน

## ควรเปิด OpenAPI ใน production ไหม

คำตอบขึ้นกับระบบ

ถ้าเป็น public API อาจเปิด OpenAPI public ได้ แต่ต้องตรวจว่าไม่มีข้อมูล sensitive และควรมี versioning

ถ้าเป็น internal admin API อาจจำกัดเฉพาะ development/staging หรือป้องกันด้วย network/authentication เพิ่ม

สำหรับหนังสือนี้ให้เปิดเฉพาะ development ก่อน

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `builder.Services.AddOpenApi()`
- มี `app.MapOpenApi()` ใน development
- เปิด `/openapi/v1.json` ได้ด้วย port จริงของเครื่อง
- controller สำคัญมี `ProducesResponseType`
- เข้าใจว่า OpenAPI built-in ให้ document ไม่ใช่ interactive UI โดยอัตโนมัติ
