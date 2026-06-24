---
title: 26 - Error Response Format
description: ออกแบบ ProblemDetails และ ValidationProblemDetails ให้ frontend ใช้งานต่อได้ง่าย
---

หลังจากมี global exception handler แล้ว ขั้นต่อไปคือทำให้ error response ทั้งระบบมีรูปแบบสม่ำเสมอ

ASP.NET Core ใช้แนวทาง `ProblemDetails` สำหรับ error response และใช้ `ValidationProblemDetails` สำหรับ validation error ซึ่งเหมาะกับ Web API เพราะเป็น format ที่ client อ่านและประมวลผลต่อได้ง่าย

## วิธีเรียนบทนี้

บทนี้ไม่ได้เพิ่ม endpoint ใหม่ แต่ปรับรูปแบบ error response ให้คาดเดาได้ ให้ทำตามลำดับนี้:

1. อ่านรูปแบบ error ที่ต้องการก่อน
2. ปรับ automatic validation response
3. เพิ่ม `traceId` ให้ `ProblemDetails`
4. ตรวจว่า Controller ไม่สร้าง error object เอง
5. เพิ่ม business rule ตอน update email ซ้ำ

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `ProblemDetails` | error response สำหรับ business error หรือ unexpected error |
| `ValidationProblemDetails` | error response สำหรับ validation error |
| `ApiBehaviorOptions` | options สำหรับปรับ behavior ของ `[ApiController]` |
| `InvalidModelStateResponseFactory` | factory ที่สร้าง response ตอน validation ไม่ผ่าน |
| `BadRequestObjectResult` | result สำหรับตอบ `400 Bad Request` พร้อม body |
| `traceId` | id ที่ใช้เชื่อม response กับ log ฝั่ง server |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Program.cs
Services/UserService.cs
Controllers/UsersController.cs
```

## รูปแบบ error ทั่วไป

สำหรับ business error เช่น user not found หรือ email ซ้ำ เราจะใช้รูปแบบนี้

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "User not found",
  "status": 404,
  "code": "USER_NOT_FOUND"
}
```

ความหมายของแต่ละ field คือ

- `type` ลิงก์อ้างอิงประเภทปัญหา
- `title` ข้อความสั้น ๆ อธิบายปัญหา
- `status` HTTP status code
- `code` error code ของระบบเราเอง

## รูปแบบ validation error

Validation error ควรบอกว่า field ไหนผิดและผิดเพราะอะไร

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed",
  "status": 400,
  "code": "VALIDATION_FAILED",
  "errors": {
    "Email": [
      "The Email field is required."
    ]
  }
}
```

`errors` เป็น dictionary โดย key คือชื่อ field และ value คือรายการข้อความ error ของ field นั้น

## ปรับ automatic validation response

ตอนนี้ `[ApiController]` ตอบ validation error ให้อัตโนมัติอยู่แล้ว แต่เราจะเพิ่ม `code` และปรับ title ให้ตรงกับรูปแบบของระบบ

## ขั้นที่ 1: เพิ่ม using ใน Program.cs

เปิด `Program.cs` แล้วเพิ่ม using

```csharp
using Microsoft.AspNetCore.Mvc;
```

## ขั้นที่ 2: ตั้งค่า InvalidModelStateResponseFactory

เพิ่ม config นี้หลัง `builder.Services.AddControllers();`

```csharp
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problemDetails = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Type = "https://httpstatuses.com/400"
        };

        problemDetails.Extensions["code"] = "VALIDATION_FAILED";
        problemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;

        return new BadRequestObjectResult(problemDetails);
    };
});
```

หลังปรับแล้ว validation error จาก `[Required]`, `[EmailAddress]` และ custom validation จะมี `code` เป็น `VALIDATION_FAILED`

## ขั้นที่ 3: เพิ่ม traceId

เวลา debug production เรามักต้องการค่า trace id เพื่อหา log ที่เกี่ยวข้อง

ปรับ `AddProblemDetails` ใน `Program.cs`

```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});
```

ถ้ามี `builder.Services.AddProblemDetails();` เดิมอยู่ ให้แทนที่ด้วย version นี้

เหตุผลที่เราใส่ `traceId` ทั้งใน `InvalidModelStateResponseFactory` และใน `AddProblemDetails` คือ validation error ถูกสร้างจาก factory โดยตรง ส่วน business error และ unexpected error ถูกเขียนผ่าน `IProblemDetailsService` ใน global exception handler ทั้งสองทางจึงต้องได้ `traceId` เหมือนกัน

## ตรวจ build

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet build
```

ถ้า build error ว่าไม่รู้จัก `ApiBehaviorOptions` หรือ `ValidationProblemDetails` ให้ตรวจ using นี้:

```csharp
using Microsoft.AspNetCore.Mvc;
```

## อย่าออกแบบ error response ตามใจแต่ละ Controller

หลีกเลี่ยง code แบบนี้ใน Controller หลาย ๆ จุด

```csharp
return BadRequest(new { error = "Email is invalid" });
return BadRequest(new { message = "Email is invalid" });
return BadRequest(new { errors = "Email is invalid" });
```

เพราะ frontend ต้องรองรับหลาย format ทั้งที่เป็น error ประเภทเดียวกัน

ให้ใช้ automatic validation response, global exception handler และ `ProblemDetails` เป็นหลัก

## ขั้นที่ 4: ให้ UpdateUserAsync ตรวจ email ซ้ำด้วย

ตอนแก้ email ผู้ใช้ ก็ต้องตรวจ email ซ้ำเช่นกัน

เปิดไฟล์:

```text
Services/UserService.cs
```

ใน method `UpdateUserAsync` หลังตรวจว่า user มีอยู่แล้ว ให้เพิ่ม logic ตรวจ email ซ้ำ:

```csharp
var existingUser = await userRepository.GetByEmailAsync(request.Email);

if (existingUser is not null && existingUser.Id != id)
{
    throw new ConflictException("Email already exists", "EMAIL_ALREADY_EXISTS");
}
```

ตำแหน่งโดยรวมใน method จะประมาณนี้:

```csharp
public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
{
    var user = await userRepository.GetByIdAsync(id);

    if (user is null)
    {
        throw new NotFoundException("User not found", "USER_NOT_FOUND");
    }

    var existingUser = await userRepository.GetByEmailAsync(request.Email);

    if (existingUser is not null && existingUser.Id != id)
    {
        throw new ConflictException("Email already exists", "EMAIL_ALREADY_EXISTS");
    }

    user.Email = request.Email;

    await userRepository.UpdateAsync(user);

    return ToResponse(user);
}
```

## ทดสอบ error response

เพิ่ม request เหล่านี้ใน `.http`

```http
@baseUrl = http://localhost:5156

### Validation failed
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "not-an-email"
}
```

ควรได้ `400 Bad Request` พร้อม `code` เป็น `VALIDATION_FAILED` และมี `traceId`

```http
### Email conflict
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "demo-user@example.com"
}
```

ควรได้ `409 Conflict` พร้อม `code` เป็น `EMAIL_ALREADY_EXISTS`

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- business error ใช้ `ProblemDetails`
- validation error ใช้ `ValidationProblemDetails`
- error response มี `code`
- error response มี `traceId`
- Controller ไม่สร้าง error object เองหลายรูปแบบ
- update user ตรวจ email ซ้ำแล้ว
