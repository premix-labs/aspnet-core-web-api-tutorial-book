---
title: 26 - Error Response Format
description: ออกแบบ ProblemDetails และ ValidationProblemDetails ให้ frontend ใช้งานต่อได้ง่าย
---

หลังจากมี global exception handler แล้ว ขั้นต่อไปคือทำให้ error response ทั้งระบบมีรูปแบบสม่ำเสมอ

ASP.NET Core ใช้แนวทาง `ProblemDetails` สำหรับ error response และใช้ `ValidationProblemDetails` สำหรับ validation error ซึ่งเหมาะกับ Web API เพราะเป็น format ที่ client อ่านและประมวลผลต่อได้ง่าย

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

เปิด `Program.cs` แล้วเพิ่ม using

```csharp
using Microsoft.AspNetCore.Mvc;
```

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

## เพิ่ม traceId

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

## อย่าออกแบบ error response ตามใจแต่ละ Controller

หลีกเลี่ยง code แบบนี้ใน Controller หลาย ๆ จุด

```csharp
return BadRequest(new { error = "Email is invalid" });
return BadRequest(new { message = "Email is invalid" });
return BadRequest(new { errors = "Email is invalid" });
```

เพราะ frontend ต้องรองรับหลาย format ทั้งที่เป็น error ประเภทเดียวกัน

ให้ใช้ automatic validation response, global exception handler และ `ProblemDetails` เป็นหลัก

## UpdateUserAsync ควรจัดการ email ซ้ำด้วย

ตอนแก้ email ผู้ใช้ ก็ต้องตรวจ email ซ้ำเช่นกัน

ตัวอย่าง logic ใน `UserService`

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

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- business error ใช้ `ProblemDetails`
- validation error ใช้ `ValidationProblemDetails`
- error response มี `code`
- error response มี `traceId`
- Controller ไม่สร้าง error object เองหลายรูปแบบ
- update user ตรวจ email ซ้ำแล้ว
