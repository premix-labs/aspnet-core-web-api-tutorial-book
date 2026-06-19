---
title: 49 - Build และ Publish
description: ตรวจ build, test, publish และ Docker image ก่อน deploy
---

ก่อน deploy ต้องตรวจว่า project build ผ่าน test ผ่าน และสามารถ publish เป็น output สำหรับ runtime ได้

บทนี้รวบรวมคำสั่งปิดงานก่อนส่ง API ไปใช้งานจริง

## Restore

```powershell
dotnet restore
```

`dotnet build`, `dotnet test` และ `dotnet publish` restore ให้อัตโนมัติได้ แต่การแยก restore ชัดเจนมีประโยชน์ใน CI/CD

## Build

```powershell
dotnet build -c Release
```

ใช้ `Release` configuration เพื่อให้ใกล้กับตอน deploy จริง

## Test

```powershell
dotnet test -c Release
```

ถ้า test ต้องใช้ database ให้แน่ใจว่า test database พร้อมก่อนรัน

## Publish

ถ้าอยู่ที่ root ของ solution ให้ระบุ path ไปยัง project

```powershell
dotnet publish Backend.Api\Backend.Api.csproj -c Release -o .\publish
```

`dotnet publish` จะ compile application, อ่าน dependency และสร้าง output ที่พร้อมนำไปรันบน hosting system

หลัง publish แล้วควรเห็นไฟล์ประมาณนี้ในโฟลเดอร์ `publish`

```text
Backend.Api.dll
Backend.Api.deps.json
Backend.Api.runtimeconfig.json
appsettings.json
```

## Run published app

ทดสอบ output ที่ publish แล้ว

```powershell
dotnet .\publish\Backend.Api.dll
```

ถ้ารันไม่ได้ ให้แก้ก่อน deploy เพราะนี่คือสิ่งที่ server หรือ container จะรันจริง

## Build Docker image

Dockerfile ของบทที่ 47 อยู่ใน `Backend.Api/` ดังนั้นถ้าอยู่ที่ root ของ solution ให้ระบุ context เป็นโฟลเดอร์นั้น

```powershell
docker build -t backend-api:release .\Backend.Api
```

## Inspect image

```powershell
docker images backend-api
```

## Run smoke test

หลังรัน container หรือ compose แล้ว ให้ทดสอบ endpoint สำคัญ

```http
GET http://localhost:18080/openapi/v1.json
```

ถ้า OpenAPI เปิดเฉพาะ development endpoint นี้อาจไม่เปิดใน production ให้ทดสอบ endpoint public อื่นแทน เช่น login validation

```http
POST http://localhost:18080/api/auth/login
Content-Type: application/json

{
  "email": "wrong@example.com",
  "password": "wrong"
}
```

ควรได้ response ที่ควบคุมได้ ไม่ใช่ stack trace

## Checkpoint

ก่อนอ่านบทสุดท้าย ให้ตรวจว่าทำได้ครบตามนี้

- `dotnet restore` ผ่าน
- `dotnet build -c Release` ผ่าน
- `dotnet test -c Release` ผ่าน
- `dotnet publish` ผ่าน
- run published app ได้
- build Docker image ได้
- smoke test endpoint สำคัญได้
