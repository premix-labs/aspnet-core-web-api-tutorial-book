---
title: 48 - Docker Compose
description: รัน API และ SQL Server พร้อมกันด้วย Docker Compose
---

Docker Compose ใช้รันหลาย container พร้อมกัน เช่น API และ SQL Server

บทนี้จะสร้าง `docker-compose.yml` สำหรับรัน Backend API กับ SQL Server ในเครื่อง local

## สร้าง docker-compose.yml

สร้างไฟล์

```text
docker-compose.yml
```

เพิ่ม config นี้

```yaml
services:
  api:
    build:
      context: ./Backend.Api
      dockerfile: Dockerfile
    ports:
      - "18080:8080"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=db,1433;Database=BackendApiDb;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;"
      DataSeeding__Enabled: "false"
      Jwt__Issuer: "Backend.Api"
      Jwt__Audience: "Backend.ApiClient"
      Jwt__SigningKey: "${JWT_SIGNING_KEY}"
      Jwt__ExpirationMinutes: "60"
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "${MSSQL_SA_PASSWORD}"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql

volumes:
  sqlserver-data:
```

ใน connection string ของ API ใช้ `Server=db,1433` เพราะ `db` คือชื่อ service ใน Docker Compose network

ค่า port `"18080:8080"` หมายความว่าเครื่อง host เปิดที่ `http://localhost:18080` แต่ใน container API ยังฟังที่ port `8080`

สร้างไฟล์ `.env.example` เพื่อบอกชื่อค่าที่จำเป็น โดยยังไม่ใส่ secret จริง:

```text
MSSQL_SA_PASSWORD=Replace_With_Strong_Local_Password_123!
JWT_SIGNING_KEY=replace-with-local-development-signing-key-at-least-32-bytes
```

เวลาใช้งานจริงให้ copy เป็น `.env` แล้วเปลี่ยนค่าให้เหมาะกับเครื่องหรือ environment นั้น ไฟล์ `.env` ต้องอยู่ใน `.gitignore`

ตัวอย่างนี้ตั้ง `DataSeeding__Enabled=false` เพื่อให้ API container start ได้ก่อน แม้ฐานข้อมูลยังไม่ได้รัน migration ถ้าต้องการ seed admin/demo user ให้รัน migration ก่อน แล้วค่อยเปิด seeding ใน environment ของ API

## รัน Compose

```powershell
copy .env.example .env
docker compose up --build
```

ถ้าต้องการรันแบบ background

```powershell
docker compose up --build -d
```

## ดู log

```powershell
docker compose logs -f api
docker compose logs -f db
```

## หยุด container

```powershell
docker compose down
```

ถ้าต้องการลบ volume database ด้วย

```powershell
docker compose down -v
```

คำสั่ง `-v` จะลบข้อมูล SQL Server ใน volume ด้วย ใช้อย่างระวัง

## เรื่อง migration

Compose นี้ยังไม่ได้รัน migration อัตโนมัติ

สำหรับช่วงเรียน ให้รัน database container ก่อน แล้วรัน migration จากเครื่อง host จากโฟลเดอร์ `Backend.Api`

```powershell
docker compose up -d db

cd Backend.Api
$env:LOCAL_SQL_PASSWORD="Replace_With_Strong_Local_Password_123!"
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BackendApiDb;User Id=sa;Password=$($env:LOCAL_SQL_PASSWORD);TrustServerCertificate=True;"
$env:Jwt__SigningKey="replace-with-local-development-signing-key-at-least-32-bytes"
dotnet tool run dotnet-ef database update
cd ..
```

จากนั้นค่อยรัน API

```powershell
docker compose up --build api
```

แนวทาง production จริงอาจใช้ migration job แยก, CI/CD step หรือ deployment script แทนการให้ API migrate เองทุกครั้งที่ start

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `docker-compose.yml`
- API และ SQL Server อยู่ใน compose เดียวกัน
- API ใช้ `Server=db,1433`
- ใช้ environment variables แทน secret hard-code ใน C#
- มี `.env.example` และไม่ commit `.env`
- รัน `docker compose up --build` ได้
