---
title: 45 - Unit Test
description: ทดสอบ business logic ขนาดเล็กโดยไม่ต้องรัน API ทั้งระบบ
---

Unit test ใช้ทดสอบ logic ขนาดเล็กโดยไม่ต้องรัน application ทั้งตัวหรือเชื่อม database จริง

ในโปรเจกต์นี้สิ่งที่เหมาะกับ unit test ได้แก่ role validation, pagination calculation, mapping และ business rule ที่ไม่ต้องแตะ database

## สร้าง test project

ที่ root ของ solution ให้รันคำสั่งนี้

```powershell
dotnet new xunit -n Backend.Api.Tests -f net10.0
dotnet sln add Backend.Api.Tests\Backend.Api.Tests.csproj
dotnet add Backend.Api.Tests\Backend.Api.Tests.csproj reference Backend.Api\Backend.Api.csproj
```

ถ้ายังไม่มี solution ให้สร้างก่อน

```powershell
dotnet new sln -n Backend.Api
dotnet sln add Backend.Api\Backend.Api.csproj
dotnet sln add Backend.Api.Tests\Backend.Api.Tests.csproj
```

ใน .NET รุ่นใหม่ไฟล์ solution อาจถูกสร้างเป็น `.slnx` ได้ คำสั่ง `dotnet sln add ...` ยังใช้ได้เหมือนเดิมถ้าอยู่ในโฟลเดอร์เดียวกับไฟล์ solution นั้น

## เขียน test สำหรับ Roles

สร้างไฟล์

```text
Backend.Api.Tests/RolesTests.cs
```

เพิ่ม code นี้

```csharp
using Backend.Api.Constants;

namespace Backend.Api.Tests;

public class RolesTests
{
    [Theory]
    [InlineData(Roles.User)]
    [InlineData(Roles.Admin)]
    [InlineData("user")]
    [InlineData("admin")]
    public void IsValid_WhenRoleIsKnown_ReturnsTrue(string role)
    {
        var result = Roles.IsValid(role);

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("SuperAdmin")]
    [InlineData("Manager")]
    public void IsValid_WhenRoleIsUnknown_ReturnsFalse(string role)
    {
        var result = Roles.IsValid(role);

        Assert.False(result);
    }
}
```

## รัน test

```powershell
dotnet test
```

ผลลัพธ์ที่คาดหวังคือ test ผ่านทั้งหมด

## ควร test อะไรเพิ่ม

- `Roles.IsValid`
- logic ป้องกัน admin demote/deactivate ตัวเอง
- validation helper ที่ไม่ต้องใช้ database
- mapping จาก entity เป็น response DTO
- pagination metadata เช่น `TotalPages`

## ไม่ควร unit test อะไร

ไม่ควรพยายาม unit test EF Core query หนัก ๆ ด้วย mock ที่ซับซ้อนเกินไป เพราะจะกลายเป็น test implementation detail

ถ้าต้องทดสอบ endpoint, middleware, auth หรือ database behavior ให้ใช้ integration test ในบทถัดไป

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี test project
- project test reference project API
- มี `RolesTests`
- รัน `dotnet test` ผ่าน
- แยกได้ว่าอะไรควรเป็น unit test และอะไรควรเป็น integration test
