---
title: 24 - Custom Validation
description: เพิ่ม validation rule เฉพาะระบบด้วย IValidatableObject และแยก business validation ออกจาก Controller
---

Data Annotations พื้นฐานครอบคลุม rule ทั่วไป เช่น required, email และความยาว แต่ระบบจริงมักมี rule เฉพาะ เช่นห้ามใช้ email domain บางแบบ หรือ email ต้องไม่ซ้ำใน database

บทนี้จะแยก validation เป็นสองระดับ:

```text
DTO validation       ตรวจรูปแบบ input ที่ไม่ต้อง query database
Business validation  ตรวจ rule ของระบบ เช่น email ซ้ำ
```

## วิธีเรียนบทนี้

ให้ทำทีละชั้น:

1. เพิ่ม custom rule ใน request DTO
2. ทดสอบว่า rule นี้ตอบ `400 Bad Request`
3. ตรวจว่า email ซ้ำยังอยู่ใน `UserService`
4. ตรวจว่า Controller ไม่ query repository เอง

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `IValidatableObject` | interface สำหรับเขียน validation rule เองใน DTO |
| `Validate(...)` | method ที่ ASP.NET Core เรียกตอน model validation |
| `ValidationResult` | ผลลัพธ์ที่บอกว่า validation ไม่ผ่าน |
| `yield return` | คืน validation error ทีละรายการ |
| `StringComparison.OrdinalIgnoreCase` | เปรียบเทียบ string แบบไม่สนตัวพิมพ์เล็ก/ใหญ่ |
| business validation | rule ที่ต้องดูข้อมูลระบบ เช่น email ซ้ำ |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
Dtos/Users/CreateUserRequest.cs
Dtos/Users/UpdateUserRequest.cs
Services/UserService.cs
Controllers/UsersController.cs
```

## Rule ตัวอย่างของบทนี้

สมมติระบบของเราไม่ต้องการรับ email domain สำหรับทดสอบ เช่น:

```text
@example.invalid
```

rule นี้เป็น input validation เพราะตรวจได้จากค่า email ที่ client ส่งมา ไม่ต้อง query database

## ขั้นที่ 1: เพิ่ม IValidatableObject ใน CreateUserRequest

เปิดไฟล์:

```text
Dtos/Users/CreateUserRequest.cs
```

แก้ class ให้ implement `IValidatableObject`

```csharp
public class CreateUserRequest : IValidatableObject
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;
}
```

จากนั้นเพิ่ม method `Validate` ไว้ใน class:

```csharp
public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
{
    if (Email.EndsWith("@example.invalid", StringComparison.OrdinalIgnoreCase))
    {
        yield return new ValidationResult(
            "Email domain is not allowed.",
            [nameof(Email)]);
    }
}
```

`[nameof(Email)]` บอกว่า error นี้ผูกกับ field `Email` ทำให้ response validation แสดง error ใต้ field ที่ถูกต้อง

## ขั้นที่ 2: ตรวจภาพรวม CreateUserRequest

ไฟล์ควรมีภาพรวมประมาณนี้:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Dtos.Users;

public class CreateUserRequest : IValidatableObject
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Email.EndsWith("@example.invalid", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Email domain is not allowed.",
                [nameof(Email)]);
        }
    }
}
```

## ขั้นที่ 3: ทำแบบเดียวกันใน UpdateUserRequest

เปิดไฟล์:

```text
Dtos/Users/UpdateUserRequest.cs
```

เพิ่ม `IValidatableObject` และ `Validate(...)` ด้วย rule เดียวกัน:

```csharp
public class UpdateUserRequest : IValidatableObject
{
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Email.EndsWith("@example.invalid", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "Email domain is not allowed.",
                [nameof(Email)]);
        }
    }
}
```

ตอนนี้ rule ในสอง DTO ซ้ำกันเล็กน้อย ยอมรับได้ในช่วงเรียน เพราะยังอ่านง่ายกว่าแยก abstraction เราจะค่อย refactor เมื่อ duplication เริ่มมีเหตุผลจริง

## ตรวจ build

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet build
```

ถ้า build error ว่าไม่รู้จัก `IValidatableObject` หรือ `ValidationResult` ให้ตรวจว่า DTO มี using นี้:

```csharp
using System.ComponentModel.DataAnnotations;
```

## ทดสอบ custom validation

รัน API:

```powershell
dotnet run
```

เพิ่ม request นี้ใน `.http`

```http
@baseUrl = http://localhost:5156

### Blocked email domain
POST {{baseUrl}}/api/users
Content-Type: application/json

{
  "email": "user@example.invalid"
}
```

ผลลัพธ์ที่คาดหวังคือ `400 Bad Request` และ error ควรอยู่ใต้ field `Email`

## Business validation ไม่ควรอยู่ใน DTO

Rule บางอย่างต้อง query database เช่น email ซ้ำหรือไม่

ไม่ควรใส่ database query ลงใน DTO เพราะ DTO ควรเป็น object สำหรับรับส่งข้อมูล ไม่ควรรู้รายละเอียด repository หรือ database

rule แบบนี้ควรอยู่ใน `UserService`:

```csharp
var existingUser = await userRepository.GetByEmailAsync(request.Email);

if (existingUser is not null)
{
    throw new InvalidOperationException("Email already exists.");
}
```

ตอนนี้เรายังใช้ `InvalidOperationException` ชั่วคราว ในบทถัดไปจะเปลี่ยนเป็น custom exception ที่ global exception handler แปลงเป็น `409 Conflict`

## ตรวจ Controller

Controller ไม่ควรตรวจ email ซ้ำหรือ query repository เอง หน้าที่หลักของ Controller คือรับ request แล้วส่งต่อให้ service:

```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    var user = await userService.CreateUserAsync(request);

    return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
}
```

ถ้า Controller ยังมี `try-catch` สำหรับ email ซ้ำอยู่ ตอนนี้ยังพอรับได้จากบทก่อน แต่บทถัดไปจะย้าย error handling ไปไว้ที่ global handler เพื่อให้ Controller บางลงกว่าเดิม

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- `CreateUserRequest` และ `UpdateUserRequest` implement `IValidatableObject`
- ส่ง email domain ที่ห้ามใช้แล้วได้ `400 Bad Request`
- เข้าใจว่า DTO validation ไม่ควร query database
- email ซ้ำถูกตรวจใน `UserService`
- Controller ไม่ query repository เอง
