---
title: 19 - ตั้งค่า Connection String
description: ตั้งค่า connection string และลงทะเบียน AppDbContext ให้ใช้ SQL Server
---

Connection string คือข้อความที่บอก application ว่าต้องเชื่อมต่อ database ที่ไหน ใช้ database ชื่ออะไร และใช้ credential แบบใด

บทนี้เราจะตั้งค่า SQL Server สำหรับเครื่อง local ก่อน ส่วน production จะย้ายค่าลับไปอยู่ใน environment variable ในภาค Production Ready

## วิธีเรียนบทนี้

บทนี้มีทั้งงาน infrastructure และ code ให้แยกทำเป็น 3 รอบ:

1. เปิด SQL Server local หรือ container ให้พร้อม
2. ใส่ connection string ใน `appsettings.json`
3. ลงทะเบียน `AppDbContext` ใน `Program.cs`

หลังจบแต่ละรอบให้ตรวจทันที อย่ารอจนถึง migration เพราะถ้า connection string ผิด migration จะล้มเหลวในบทถัดไป

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| connection string | ข้อความที่บอก app ว่าจะเชื่อม database ที่ไหน |
| `ConnectionStrings` | section มาตรฐานใน configuration ของ .NET |
| `GetConnectionString(...)` | อ่านค่า connection string จาก configuration |
| `AddDbContext<TContext>()` | ลงทะเบียน `DbContext` เข้า DI container |
| `UseSqlServer(...)` | บอก EF Core ให้ใช้ SQL Server provider |
| `TrustServerCertificate=True` | ใช้กับ local development เพื่อลดปัญหา certificate |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
appsettings.json
Program.cs
```

## เตรียม SQL Server ด้วย Docker

ถ้าเครื่องคุณยังไม่มี SQL Server local สามารถรันด้วย Docker ได้

Windows PowerShell:

```powershell
$env:LOCAL_SQL_PASSWORD="Replace_With_Strong_Local_Password_123!"

docker run --name backend-api-sql `
  -e "ACCEPT_EULA=Y" `
  -e "MSSQL_SA_PASSWORD=$env:LOCAL_SQL_PASSWORD" `
  -p 1433:1433 `
  -d mcr.microsoft.com/mssql/server:2022-latest
```

macOS/Linux Bash:

```bash
export LOCAL_SQL_PASSWORD="Replace_With_Strong_Local_Password_123!"

docker run --name backend-api-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=$LOCAL_SQL_PASSWORD" \
  -p 1433:1433 \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

รหัสผ่านของ SQL Server ต้องซับซ้อนพอ เช่นมีตัวพิมพ์ใหญ่ ตัวพิมพ์เล็ก ตัวเลข และสัญลักษณ์

ตรวจว่า container ทำงานอยู่

```powershell
docker ps
```

ถ้าต้องการดู log

```powershell
docker logs backend-api-sql
```

## เพิ่ม connection string ใน appsettings.json

เปิดไฟล์ `appsettings.json` แล้วเพิ่ม `ConnectionStrings`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=BackendApiDb;User Id=sa;Password=Replace_With_Strong_Local_Password_123!;TrustServerCertificate=True;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

ถ้าไฟล์ของคุณมี key อื่นอยู่แล้ว ให้รวม `ConnectionStrings` เข้าไปใน object เดิม ไม่ต้องสร้าง JSON ซ้อนกันสองชุด

อ่าน connection string นี้เป็นส่วน ๆ:

| ส่วน | ความหมาย |
| --- | --- |
| `Server=localhost,1433` | SQL Server อยู่ที่เครื่องเรา port `1433` |
| `Database=BackendApiDb` | ชื่อ database ที่จะใช้ |
| `User Id=sa` | user ของ SQL Server container |
| `Password=...` | password ที่ตั้งไว้ตอน `docker run` |
| `TrustServerCertificate=True` | อนุญาต certificate local สำหรับ dev |

## ลงทะเบียน DbContext ใน Program.cs

เปิด `Program.cs` แล้วเพิ่ม using ด้านบน

```csharp
using Microsoft.EntityFrameworkCore;
using Backend.Api.Data;
```

จากนั้นเพิ่ม code หลังบรรทัดสร้าง `builder`

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

ตำแหน่งโดยรวมใน `Program.cs` จะประมาณนี้

```csharp
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
builder.Services.AddOpenApi();
```

ควรวาง `AddDbContext` ก่อน `builder.Build()` เพราะหลังจาก build app แล้ว DI container จะถูกสร้างเรียบร้อย การลงทะเบียน service เพิ่มทีหลังไม่ใช่ flow ปกติ

## ทำไมต้อง throw เมื่อไม่พบ connection string

ถ้าไม่มี connection string application ควรหยุดตั้งแต่ตอน start และบอก error ให้ชัด เพราะถ้าปล่อยให้รันต่อไปจะไป error ตอนใช้งาน database ซึ่ง debug ยากกว่า

## ตรวจหลังตั้งค่า

รันจากโฟลเดอร์ `Backend.Api`

```powershell
dotnet build
docker ps
```

`dotnet build` ต้องผ่าน ส่วน `docker ps` ควรเห็น container ชื่อ `backend-api-sql` ถ้าคุณใช้ Docker

บทนี้ยังไม่ต้องเห็น table ใน database เพราะเรายังไม่ได้สร้าง migration

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- SQL Server local หรือ container ทำงานอยู่
- `appsettings.json` มี `ConnectionStrings:DefaultConnection`
- ค่า password ในตัวอย่างเป็น placeholder สำหรับ local development เท่านั้น และภาค Production Ready จะย้าย secret ออกจาก appsettings
- `Program.cs` ลงทะเบียน `AppDbContext`
- project ยัง build ผ่านด้วย `dotnet build`
