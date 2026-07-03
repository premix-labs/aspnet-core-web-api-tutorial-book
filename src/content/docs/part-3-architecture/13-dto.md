---
title: 13 - DTO คืออะไรและใช้เมื่อไหร่
description: ใช้ DTO เพื่อควบคุมข้อมูลที่รับเข้าและส่งออกจาก API
---

DTO ย่อมาจาก Data Transfer Object เป็น object ที่ใช้รับหรือส่งข้อมูลผ่าน API โดยไม่ผูกกับ database entity โดยตรง

เมื่อโปรเจกต์ยังเล็ก การส่ง `User` model ออกไปตรง ๆ อาจดูง่าย แต่ในระบบจริงเป็นจุดที่ทำให้ข้อมูลรั่วและ API contract เปลี่ยนโดยไม่ตั้งใจได้ง่าย

## วิธีเรียนบทนี้

บทนี้ให้คิดแบบแยก “ข้อมูลข้างในระบบ” ออกจาก “ข้อมูลที่ API ยอมรับหรือส่งออก”

เวลาเจอ class ในบทนี้ ให้ถามตัวเองว่า:

- class นี้ใช้ภายในระบบหรือใช้คุยกับ client
- field ไหนควรรับจาก client
- field ไหนควรส่งกลับ
- field ไหนห้ามหลุดออกจาก API

## ก่อนเริ่มบทนี้

ให้ทำบท 12 ให้จบก่อน และตรวจว่าโปรเจกต์ build ผ่านหลังแยก Service/Repository แล้ว:

```powershell
dotnet build
```

ตอนเริ่มบทนี้ `UsersController` อาจยังมี `CreateUserRequest` และ `UpdateUserRequest` เป็น record อยู่ในไฟล์ Controller เอง ซึ่งเป็นสภาพที่คาดไว้จากบท 12

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

บทนี้จะสร้าง DTO แยกออกจาก Controller:

```text
Dtos/Users/CreateUserRequest.cs  เพิ่มใหม่
Dtos/Users/UpdateUserRequest.cs  เพิ่มใหม่
Dtos/Users/UserResponse.cs       เพิ่มใหม่
Controllers/UsersController.cs   ลบ request record เดิม และเพิ่ม using DTO
```

หลังจบบทนี้ `UserResponse` จะถูกสร้างไว้ก่อน แต่การ map response ให้ใช้ `UserResponse` ทุก endpoint จะทำในบท 14

## Entity/Model กับ DTO ต่างกันอย่างไร

`Entity` หรือ model ภายในระบบคือ object ที่สะท้อนข้อมูลที่ระบบใช้จริง

ตัวอย่าง entity ในระบบจริง:

ตัวอย่างนี้ให้อ่านเพื่อเห็นปัญหา ไม่ต้องพิมพ์ตามในโปรเจกต์ตอนนี้

```csharp
public class User
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    // Internal field required by the server but never returned to clients.
    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;
}
```

ข้อมูลอย่าง `PasswordHash` จำเป็นต่อระบบ แต่ห้ามส่งออกไปหา client

DTO คือ object ที่ออกแบบมาเพื่อ request/response โดยเฉพาะ

ตัวอย่างนี้ก็ยังเป็นตัวอย่างแนวคิด ไม่ใช่ไฟล์ที่ต้องสร้างในบทนี้

```csharp
// Response DTO exposes only the fields the client should see.
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
// Request DTO for creating a user.
public record CreateUserRequest(string Email);

// Request DTO for updating a user.
public record UpdateUserRequest(string Email);

// Response DTO returned from user endpoints.
public record UserResponse(int Id, string Email);
```

ในภาค Authentication เราจะมี DTO เพิ่ม เช่น

```csharp
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;

    // Password is accepted from the client but should never be returned.
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;

    // Password is input-only and should never appear in a response DTO.
    public string Password { get; set; } = string.Empty;
}
```

`RegisterRequest` และ `LoginRequest` ดูคล้ายกัน แต่แยกกันได้ เพราะ validation และ use case อาจเปลี่ยนไม่เหมือนกันในอนาคต

## สิ่งที่จะใช้ในบทนี้

ก่อนสร้างไฟล์ DTO ให้รู้จักสิ่งเหล่านี้ก่อน:

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `record` | type ของ C# ที่เหมาะกับข้อมูลสั้น ๆ เช่น request/response DTO |
| `namespace Backend.Api.Dtos.Users;` | จัดกลุ่ม DTO ของ user ให้อยู่ใน namespace ชัดเจน |
| `CreateUserRequest` | DTO สำหรับข้อมูลที่ client ส่งมาตอนสร้าง user |
| `UpdateUserRequest` | DTO สำหรับข้อมูลที่ client ส่งมาตอนแก้ไข user |
| `UserResponse` | DTO สำหรับข้อมูลที่ API ส่งกลับไปหา client |
| `using Backend.Api.Dtos.Users;` | บอกไฟล์อื่นให้มองเห็น DTO ใน namespace นี้ |

หลักสำคัญคือ request DTO และ response DTO มีหน้าที่ต่างกัน แม้ตอนนี้ field จะยังน้อยก็ตาม

ลำดับการแก้ในบทนี้:

```text
สร้างโฟลเดอร์ Dtos/Users
  -> สร้าง CreateUserRequest
  -> สร้าง UpdateUserRequest
  -> สร้าง UserResponse
  -> แก้ UsersController ให้ import DTO จากโฟลเดอร์
  -> ลบ record เดิมทั้งหมดออกจาก Controller
  -> build ตรวจความถูกต้อง
```

## สร้างโฟลเดอร์ DTO

สร้างโฟลเดอร์นี้

```text
Dtos/
  Users/
```

ให้รันจากโฟลเดอร์ `Backend.Api`

Windows PowerShell:

```powershell
New-Item -ItemType Directory -Force -Path Dtos/Users
```

macOS/Linux Bash:

```bash
mkdir -p Dtos/Users
```

โฟลเดอร์ `Dtos` ใช้เก็บ object ที่เป็น API contract ส่วน `Users` ใช้แยก DTO ของ feature user ออกจาก feature อื่น เช่น auth หรือ admin

## ขั้นที่ 1: สร้าง CreateUserRequest

สร้างไฟล์

```text
Dtos/Users/CreateUserRequest.cs
```

Windows PowerShell:

```powershell
if (-not (Test-Path -LiteralPath Dtos/Users/CreateUserRequest.cs)) {
    New-Item -ItemType File -Path Dtos/Users/CreateUserRequest.cs
}
```

macOS/Linux Bash:

```bash
touch Dtos/Users/CreateUserRequest.cs
```

ใส่ code นี้ในไฟล์:

```csharp
namespace Backend.Api.Dtos.Users;

// Request body for POST /api/v1/users.
public record CreateUserRequest(string Email);
```

ไฟล์นี้ใช้กับ `POST /api/v1/users` เท่านั้น ความหมายคือ client ต้องส่ง email เข้ามาเพื่อสร้าง user

`record CreateUserRequest(string Email)` แปลว่า DTO นี้มี property ชื่อ `Email` หนึ่งตัว และ ASP.NET Core จะ bind JSON body เข้า object นี้ให้

## ขั้นที่ 2: สร้าง UpdateUserRequest

สร้างไฟล์

```text
Dtos/Users/UpdateUserRequest.cs
```

Windows PowerShell:

```powershell
if (-not (Test-Path -LiteralPath Dtos/Users/UpdateUserRequest.cs)) {
    New-Item -ItemType File -Path Dtos/Users/UpdateUserRequest.cs
}
```

macOS/Linux Bash:

```bash
touch Dtos/Users/UpdateUserRequest.cs
```

ใส่ code นี้ในไฟล์:

```csharp
namespace Backend.Api.Dtos.Users;

// Request body for PUT /api/v1/users/{id}.
public record UpdateUserRequest(string Email);
```

ไฟล์นี้ใช้กับ `PUT /api/v1/users/{id}` แม้ตอนนี้ field เหมือน `CreateUserRequest` แต่แยกไว้เพราะ rule ตอนสร้างกับตอนแก้อาจต่างกันในอนาคต

ตัวอย่างเช่น ตอนสร้างอาจต้องมี password แต่ตอนแก้ email อาจไม่ต้องส่ง password

## ขั้นที่ 3: สร้าง UserResponse

สร้างไฟล์

```text
Dtos/Users/UserResponse.cs
```

Windows PowerShell:

```powershell
if (-not (Test-Path -LiteralPath Dtos/Users/UserResponse.cs)) {
    New-Item -ItemType File -Path Dtos/Users/UserResponse.cs
}
```

macOS/Linux Bash:

```bash
touch Dtos/Users/UserResponse.cs
```

ใส่ code นี้ในไฟล์:

```csharp
namespace Backend.Api.Dtos.Users;

// Response body returned by user endpoints.
public record UserResponse(int Id, string Email);
```

ไฟล์นี้ใช้กับ response ที่ API ส่งกลับไปหา client

`UserResponse` มี `Id` ได้ เพราะ server เป็นคนสร้าง id แล้วส่งกลับไปให้ client รู้ว่า resource ที่สร้างหรืออ่านได้มี id อะไร

ตอนนี้ DTO ยังสั้นมาก แต่ในบท validation เราจะเพิ่ม attribute เช่น `[Required]`, `[EmailAddress]` และ `[MaxLength]`

## ตรวจโครงสร้างไฟล์

หลังสร้างไฟล์ครบ โครงสร้างควรเป็นแบบนี้:

```text
Backend.Api/
  Dtos/
    Users/
      CreateUserRequest.cs
      UpdateUserRequest.cs
      UserResponse.cs
```

## แก้ Controller ให้ใช้ DTO จากโฟลเดอร์

เปิด `UsersController.cs` แล้วเพิ่ม using

```csharp
using Backend.Api.Dtos.Users;
```

บรรทัดนี้ทำให้ Controller มองเห็น DTO ที่อยู่ใน namespace `Backend.Api.Dtos.Users`

จากนั้นลบ record ที่เคยประกาศไว้บนไฟล์ Controller ออกทั้งหมด ถ้าไฟล์ของคุณมี 3 record แบบนี้ ให้ลบทั้ง 3 บรรทัด

```csharp
public record UserDto(int Id, string Email);
public record CreateUserRequest(string Email);
public record UpdateUserRequest(string Email);
```

เหตุผลที่ต้องลบ `UserDto` ด้วย เพราะมันเป็น DTO แบบเก่าที่เคยอยู่ใน Controller ถ้าปล่อยไว้จะทำให้คนอ่านสับสนว่าโปรเจกต์ควรใช้ `UserDto` หรือ `UserResponse`

ถ้า Controller ของคุณไม่มี `UserDto` แล้ว ให้ลบเฉพาะ record ที่ยังมีอยู่จริง เช่น `CreateUserRequest` และ `UpdateUserRequest`

หลังจบบทนี้เราจะมี `UserResponse` เตรียมไว้ในโฟลเดอร์ `Dtos/Users` แต่ยังไม่จำเป็นต้องใช้แทน response ทุกจุดทันที บทที่ 14 จะสอน mapping จาก `User` ภายในระบบไปเป็น `UserResponse` ก่อนส่งออก API

หลังจบบทนี้ Controller ควรใช้ DTO จากโฟลเดอร์ `Dtos/Users` ไม่ใช่ประกาศ request record ไว้ในไฟล์ Controller แล้ว

หลังแก้ Controller แล้ว ให้ build เพื่อตรวจว่า namespace และ using ถูกต้อง:

```powershell
dotnet build
```

ถ้าเจอ error ว่า `UserDto`, `CreateUserRequest` หรือ `UpdateUserRequest` ซ้ำ ให้ตรวจว่ายังมี record เดิมประกาศอยู่ใน `UsersController.cs` หรือไม่

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

## ข้อผิดพลาดที่เจอบ่อย

ถ้า build error ว่าไม่รู้จัก `CreateUserRequest` ให้ตรวจว่าไฟล์ Controller มี using นี้หรือยัง:

```csharp
// Import DTOs from the Dtos/Users folder.
using Backend.Api.Dtos.Users;
```

ถ้า build error ว่ามี type ชื่อซ้ำ ให้ลบ record เดิมที่เคยประกาศไว้ใน `UsersController.cs`

ถ้าเผลอใช้ `UserResponse` เป็น request body ตอน `POST` ให้จำว่า client ไม่ควรส่ง `Id` เข้ามาตอนสร้าง user

## Checkpoint

ก่อนอ่านบทต่อไป คุณควรเข้าใจว่า

- DTO ต่างจาก Entity/Model อย่างไร
- ทำไม response ไม่ควรมี password hash
- ทำไม request สำหรับ create และ update อาจไม่เหมือนกัน
- ทำไมควรแยก request DTO และ response DTO
- DTO ควรถูกตั้งชื่อตาม use case อย่างไร
