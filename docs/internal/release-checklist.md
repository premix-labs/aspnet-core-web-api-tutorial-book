---
title: Release Checklist
description: รายการตรวจคุณภาพก่อนเผยแพร่หนังสือและตัวอย่างโค้ด
---

# Release Checklist

ใช้ checklist นี้ก่อน publish หนังสือหรือ release ใหญ่

## Manuscript

- ทุกบทมี goal, prerequisites, files changed, steps, expected result และ checkpoint
- route, port, project name และ command ตรงกับ project จริง
- ไม่มีบทที่อ้างถึงไฟล์หรือ class ที่ยังไม่เคยสร้าง
- ไม่มี code block ที่ copy แล้ว compile ไม่ได้โดยไม่อธิบาย
- production warning ครบสำหรับ secrets, JWT, CORS, Docker และ logging
- บท 51-58 มี traceability ไปยัง final project และ tests

## Website

```powershell
npm run build
```

ถ้าจะ deploy GitHub Pages:

```powershell
$env:SITE="https://example.github.io"
$env:BASE_PATH="/aspnet-core-web-api-tutorial-book"
npm run build
```

ตรวจ:

- sidebar ครบ
- search ใช้งานได้
- Mermaid diagrams render ได้
- canonical/base path ถูกต้อง
- ภาษาไทยไม่เพี้ยน

## Final Project

```powershell
cd examples/final-backend-api
dotnet restore
dotnet tool restore
dotnet build -c Release
dotnet test -c Release
dotnet publish Backend.Api/Backend.Api.csproj -c Release
docker compose config
docker compose build
```

ต้องตรวจอย่างน้อย:

- health checks
- register/login
- current user
- admin users
- role/status changes
- audit logs
- refresh token/session endpoints
- email verification/reset password paths ถ้า scope แตะ auth hardening

## Validation Project

```powershell
cd examples/validation/progressive-backend-api
dotnet restore
dotnet tool restore
dotnet build -c Release
dotnet test -c Release
dotnet publish Backend.Api/Backend.Api.csproj -c Release
docker compose config
```

ต้องยืนยันว่า validation project ทำตามหนังสือทีละบทและไม่ลัดไปใช้ final project ก่อนบทจะสอน

## Security and Secrets

- ไม่มี `.env` จริงถูก track
- `.env.example` ไม่มี secret จริง
- tracked `appsettings*.json` ไม่เก็บ production secrets
- JWT signing key, SQL password และ SMTP credentials มาจาก environment/user secrets
- error response ไม่ leak stack trace
- Dockerfile ไม่รัน runtime เป็น root
- CORS policy แยก dev/prod

## Sign-off

ก่อน release ให้ update:

```text
docs/internal/manuscript-status.md
docs/internal/validation-report.md
```

