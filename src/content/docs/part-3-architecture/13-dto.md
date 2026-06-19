---
title: 13 - DTO คืออะไรและใช้เมื่อไหร่
description: ใช้ DTO เพื่อควบคุมข้อมูลที่รับเข้าและส่งออกจาก API
---

DTO ย่อมาจาก Data Transfer Object เป็น object ที่ใช้รับหรือส่งข้อมูลผ่าน API โดยไม่ผูกกับ database entity โดยตรง

เมื่อโปรเจกต์ยังเล็ก การส่ง `User` model ออกไปตรง ๆ อาจดูง่าย แต่ในระบบจริงเป็นจุดที่ทำให้ข้อมูลรั่วและ API contract เปลี่ยนโดยไม่ตั้งใจได้ง่าย

## Entity/Model กับ DTO ต่างกันอย่างไร

`Entity` หรือ model ภายในระบบคือ object ที่สะท้อนข้อมูลที่ระบบใช้จริง

ตัวอย่าง entity ในระบบจริง:

```csharp
public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;
}
```

ข้อมูลอย่าง `PasswordHash` จำเป็นต่อระบบ แต่ห้ามส่งออกไปหา client

DTO คือ object ที่ออกแบบมาเพื่อ request/response โดยเฉพาะ

```csharp
public record UserResponse(
    Guid Id,
    string Email,
    string Role,
    bool IsActive);
```

DTO นี้ไม่มี `PasswordHash`

## ทำไมไม่ส่ง Entity ออกไปตรง ๆ

เหตุผลหลัก:

- ป้องกันข้อมูลภายในรั่ว เช่น password hash
- คุม API contract ได้ชัดเจน
- เปลี่ยน database schema ได้โดยไม่กระทบ client ทันที
- แยก request แต่ละ use case ออกจากกัน
- ทำ validation ได้ตรงกับ endpoint

ถ้า API ส่ง Entity ออกไปตรง ๆ ทุกครั้งที่ entity เพิ่ม property ใหม่ มีโอกาสที่ property นั้นจะหลุดออก API โดยไม่ตั้งใจ

## แยก DTO ตาม use case

ไม่จำเป็นต้องมี DTO เดียวใช้ทุกงาน ควรแยกตาม action

```csharp
public record CreateUserRequest(string Email);

public record UpdateUserRequest(string Email);

public record UserResponse(int Id, string Email);
```

ในภาค Authentication เราจะมี DTO เพิ่ม เช่น

```csharp
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
```

`RegisterRequest` และ `LoginRequest` ดูคล้ายกัน แต่แยกกันได้ เพราะ validation และ use case อาจเปลี่ยนไม่เหมือนกันในอนาคต

## สร้างโฟลเดอร์ DTO

สร้างโฟลเดอร์นี้

```text
Dtos/
  Users/
```

สร้างไฟล์

```text
Dtos/Users/CreateUserRequest.cs
Dtos/Users/UpdateUserRequest.cs
Dtos/Users/UserResponse.cs
```

## CreateUserRequest

```csharp
namespace Backend.Api.Dtos.Users;

public record CreateUserRequest(string Email);
```

## UpdateUserRequest

```csharp
namespace Backend.Api.Dtos.Users;

public record UpdateUserRequest(string Email);
```

## UserResponse

```csharp
namespace Backend.Api.Dtos.Users;

public record UserResponse(int Id, string Email);
```

ตอนนี้ DTO ยังสั้นมาก แต่ในบท validation เราจะเพิ่ม attribute เช่น `[Required]`, `[EmailAddress]` และ `[MaxLength]`

## แก้ Controller ให้ใช้ DTO จากโฟลเดอร์

เปิด `UsersController.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Dtos.Users;
```

จากนั้นลบ record ที่เคยประกาศไว้บนไฟล์ Controller

```csharp
public record UserDto(int Id, string Email);
public record CreateUserRequest(string Email);
public record UpdateUserRequest(string Email);
```

ถ้าไม่ลบ จะมีชื่อ DTO ซ้ำหรือทำให้ code สับสน

## Request DTO กับ Response DTO ควรแยกกัน

อย่าใช้ DTO เดียวทั้งรับเข้าและส่งออกเพียงเพราะ field เหมือนกันตอนแรก

ตัวอย่างที่ควรหลีกเลี่ยง:

```csharp
public record UserDto(int Id, string Email);
```

แล้วใช้ `UserDto` ทั้งตอนสร้างและตอนส่งออก เพราะตอนสร้าง user client ไม่ควรส่ง `Id` เข้ามา

แยกแบบนี้ชัดกว่า:

```csharp
public record CreateUserRequest(string Email);

public record UserResponse(int Id, string Email);
```

## ตั้งชื่อ DTO

แนวทางตั้งชื่อที่ใช้ในหนังสือ:

```text
RegisterRequest
LoginRequest
LoginResponse
CurrentUserResponse
AdminUserResponse
ChangeUserRoleRequest
PagedResponse<T>
```

ชื่อควรบอก use case ไม่ใช่แค่บอกว่าเป็น DTO

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรเข้าใจว่า

- DTO ต่างจาก Entity/Model อย่างไร
- ทำไม response ไม่ควรมี password hash
- ทำไม request สำหรับ create และ update อาจไม่เหมือนกัน
- ทำไมควรแยก request DTO และ response DTO
- DTO ควรถูกตั้งชื่อตาม use case อย่างไร
