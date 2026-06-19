---
title: 44 - OpenAPI และเอกสาร API
description: สร้างเอกสาร endpoint ให้ client และทีมใช้งานผ่าน OpenAPI
---

OpenAPI คือ specification สำหรับอธิบาย HTTP API เช่น endpoint, method, request body, response และ schema

ASP.NET Core มี built-in OpenAPI support ผ่าน `Microsoft.AspNetCore.OpenApi` สำหรับสร้าง OpenAPI document ส่วน interactive UI เช่น Swagger UI หรือ Scalar เป็นส่วนเสริมที่สามารถเพิ่มภายหลังได้

## ติดตั้ง OpenAPI package

ถ้า template ยังไม่มี package นี้ ให้ติดตั้ง

```powershell
dotnet add package Microsoft.AspNetCore.OpenApi
```

## เปิด OpenAPI document

ใน `Program.cs` ควรมี service นี้

```csharp
builder.Services.AddOpenApi();
```

และ map endpoint ใน development

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

เมื่อรัน API จะเปิดดู document ได้จาก URL ประมาณนี้

```text
https://localhost:7001/openapi/v1.json
```

port ให้ใช้ค่าจริงที่ `dotnet run` แสดงในเครื่องของคุณ

## เพิ่ม response metadata ใน Controller

OpenAPI จะอ่าน metadata จาก attribute ใน controller ได้ เราควรใส่ response type สำคัญให้ชัดขึ้น

ตัวอย่าง `AuthController`

```csharp
[HttpPost("login")]
[ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
public async Task<IActionResult> Login(LoginRequest request)
{
    var response = await authService.LoginAsync(request);

    return Ok(response);
}
```

ตัวอย่าง `AdminUsersController`

```csharp
[HttpGet]
[ProducesResponseType(typeof(PagedResponse<AdminUserResponse>), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
public async Task<IActionResult> GetUsers([FromQuery] AdminUserQuery query)
{
    var users = await adminUserService.GetUsersAsync(query);

    return Ok(users);
}
```

## เปิด XML Documentation

ถ้าต้องการให้ OpenAPI มี summary/description จาก XML comments ให้เปิด generation ใน `.csproj`

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
```

จากนั้นเพิ่ม XML comments ให้ DTO หรือ action ที่สำคัญ

```csharp
/// <summary>
/// Login with email and password.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// User email address.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}
```

## ควรเปิด OpenAPI ใน production ไหม

คำตอบขึ้นกับระบบ

ถ้าเป็น public API อาจเปิด OpenAPI public ได้ แต่ต้องตรวจว่าไม่มีข้อมูล sensitive และควรมี versioning

ถ้าเป็น internal admin API อาจจำกัดเฉพาะ development/staging หรือป้องกันด้วย network/authentication เพิ่ม

สำหรับหนังสือนี้ให้เปิดเฉพาะ development ก่อน

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `builder.Services.AddOpenApi()`
- มี `app.MapOpenApi()` ใน development
- เปิด `/openapi/v1.json` ได้
- controller สำคัญมี `ProducesResponseType`
- เข้าใจว่า OpenAPI built-in ให้ document ไม่ใช่ interactive UI โดยอัตโนมัติ
