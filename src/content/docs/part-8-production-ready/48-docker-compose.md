---
title: 48 - Docker Compose
description: รัน API และ SQL Server พร้อมกันด้วย Docker Compose
---

Docker Compose ใช้รันหลาย container พร้อมกัน เช่น API และ SQL Server

บทนี้จะสร้าง `docker-compose.yml` สำหรับรัน Backend API กับ SQL Server ในเครื่อง local

ภาพรวม container ที่ Compose จะสร้าง:

```mermaid
flowchart LR
    Client["REST Client"] --> Host["localhost:18080"]
    Host --> Api["api container"]
    Api --> DbHost["db:1433"]
    DbHost --> Sql["SQL Server container"]
    Sql --> Volume["sqlserver-data volume"]
```

## วิธีเรียนบทนี้

บทนี้จะเชื่อม API กับ SQL Server ใน Compose:

1. สร้าง `docker-compose.yml`
2. เพิ่ม service `api`
3. เพิ่ม service `db`
4. สร้าง `.env.example`
5. copy เป็น `.env`
6. ตรวจ compose config
7. รัน database, migration และ API

## สิ่งที่จะใช้ในบทนี้

| สิ่งที่จะใช้ | ความหมาย |
| --- | --- |
| `docker-compose.yml` | ไฟล์กำหนดหลาย container |
| service | container หนึ่งชุดใน compose เช่น `api` หรือ `db` |
| `depends_on` | บอกลำดับเริ่ม container เบื้องต้น |
| volume | storage ที่เก็บข้อมูลข้ามการ restart container |
| `.env` | ไฟล์ค่า environment สำหรับ Docker Compose |
| `Server=db,1433` | connection string ที่ API ใช้เรียก SQL Server service ชื่อ `db` |

## หลังจบบทนี้ ไฟล์ที่เปลี่ยน

```text
docker-compose.yml
.env.example
.gitignore
```

## ขั้นที่ 1: สร้าง docker-compose.yml

รันจาก root ของ solution

Windows PowerShell:

```powershell
New-Item -ItemType File -Force -Path docker-compose.yml
```

macOS/Linux Bash:

```bash
touch docker-compose.yml
```

เปิดไฟล์:

```text
docker-compose.yml
```

เริ่มด้วย service `api`:

```yaml
services:
  api:
    build:
      context: ./Backend.Api
      dockerfile: Dockerfile
    ports:
      - "18080:8080"
    depends_on:
      - db
```

ค่า port `"18080:8080"` หมายความว่าเครื่อง host เปิดที่ `http://localhost:18080` แต่ใน container API ฟังที่ port `8080`

## ขั้นที่ 2: เพิ่ม environment ของ api

เพิ่มใต้ `depends_on` ใน service `api`:

```yaml
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: "Server=db,1433;Database=BackendApiDb;User Id=sa;Password=${MSSQL_SA_PASSWORD};TrustServerCertificate=True;"
      DataSeeding__Enabled: "false"
      Jwt__Issuer: "Backend.Api"
      Jwt__Audience: "Backend.ApiClient"
      Jwt__SigningKey: "${JWT_SIGNING_KEY}"
      Jwt__ExpirationMinutes: "60"
      Cors__AllowedOrigins__0: "http://localhost:3000"
```

ใน connection string ใช้ `Server=db,1433` เพราะ `db` คือชื่อ service ใน Docker Compose network

## ขั้นที่ 3: เพิ่ม service db

เพิ่มต่อจาก service `api`:

```yaml
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "${MSSQL_SA_PASSWORD}"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
```

SQL Server เก็บข้อมูลไว้ใน volume เพื่อไม่ให้ข้อมูลหายทุกครั้งที่ restart container

## ขั้นที่ 4: เพิ่ม volumes

เพิ่มท้ายไฟล์:

```yaml
volumes:
  sqlserver-data:
```

ตอนนี้ `docker-compose.yml` ควรมี service `api`, service `db` และ volume `sqlserver-data`

## ขั้นที่ 5: สร้าง .env.example

รันจาก root ของ solution

Windows PowerShell:

```powershell
New-Item -ItemType File -Force -Path .env.example
```

macOS/Linux Bash:

```bash
touch .env.example
```

เปิด `.env.example` แล้วเพิ่ม:

```text
MSSQL_SA_PASSWORD=Replace_With_Strong_Local_Password_123!
JWT_SIGNING_KEY=replace-with-local-development-signing-key-at-least-32-bytes
```

ไฟล์นี้บอกชื่อค่าที่จำเป็น แต่ในงานจริงควรเปลี่ยน secret ให้เหมาะกับ environment นั้น

## ขั้นที่ 6: กัน .env ไม่ให้ commit

เปิด `.gitignore` แล้วตรวจว่ามี:

```text
.env
```

ถ้ายังไม่มี ให้เพิ่มบรรทัดนี้

`.env.example` commit ได้ แต่ `.env` ไม่ควร commit เพราะมี secret จริง

## ขั้นที่ 7: copy .env.example เป็น .env

Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

macOS/Linux Bash:

```bash
cp .env.example .env
```

เปิด `.env` แล้วเปลี่ยนค่า password/signing key ให้เหมาะกับเครื่องของคุณ

## ขั้นที่ 8: ตรวจ compose config

ก่อนรัน container ให้ตรวจ config:

```powershell
docker compose config
```

คำสั่งนี้ช่วยจับ YAML indentation ผิด, variable หาย หรือ syntax ผิดก่อนรันจริง

## ขั้นที่ 9: รัน database และ migration

รัน SQL Server ก่อน:

```powershell
docker compose up -d db
```

จากนั้นรัน migration จากเครื่อง host:

Windows PowerShell:

```powershell
cd Backend.Api
$env:LOCAL_SQL_PASSWORD="Replace_With_Strong_Local_Password_123!"
$env:ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BackendApiDb;User Id=sa;Password=$($env:LOCAL_SQL_PASSWORD);TrustServerCertificate=True;"
$env:Jwt__SigningKey="replace-with-local-development-signing-key-at-least-32-bytes"
dotnet tool run dotnet-ef database update
cd ..
```

macOS/Linux Bash:

```bash
cd Backend.Api
export LOCAL_SQL_PASSWORD='Replace_With_Strong_Local_Password_123!'
export ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=BackendApiDb;User Id=sa;Password=${LOCAL_SQL_PASSWORD};TrustServerCertificate=True;"
export Jwt__SigningKey='replace-with-local-development-signing-key-at-least-32-bytes'
dotnet tool run dotnet-ef database update
cd ..
```

ให้แน่ใจว่า connection string ใน terminal ชี้ไปที่ `localhost,1433` ตอนรัน migration จากเครื่อง host ไม่ใช่ `db,1433`

## ขั้นที่ 10: รัน API

รัน API พร้อม build image:

```powershell
docker compose up --build api
```

ถ้าต้องการรันทั้งชุด:

```powershell
docker compose up --build
```

เปิด API ผ่าน host port:

```text
http://localhost:18080
```

## คำสั่งดู log และหยุด container

ดู log:

```powershell
docker compose logs -f api
docker compose logs -f db
```

หยุด container:

```powershell
docker compose down
```

ถ้าต้องการลบ volume database ด้วย:

```powershell
docker compose down -v
```

คำสั่ง `-v` จะลบข้อมูล SQL Server ใน volume ด้วย ใช้อย่างระวัง

## เรื่อง migration ใน production

Compose นี้ยังไม่ได้รัน migration อัตโนมัติ

แนวทาง production จริงอาจใช้ migration job แยก, CI/CD step หรือ deployment script แทนการให้ API migrate เองทุกครั้งที่ start

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `docker-compose.yml`
- API และ SQL Server อยู่ใน compose เดียวกัน
- API ใช้ `Server=db,1433`
- ใช้ environment variables แทน secret hard-code ใน C#
- มี `.env.example` และไม่ commit `.env`
- `docker compose config` ผ่าน
- รัน `docker compose up --build` ได้
