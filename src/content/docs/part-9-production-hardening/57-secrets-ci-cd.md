---
title: "57. Secrets และ CI/CD"
description: "แยก config ลับออกจาก code และให้ pipeline เป็นด่านตรวจคุณภาพก่อน deploy"
---

Production-grade API ต้องไม่เก็บ secret จริงไว้ใน repository เช่น connection string, JWT signing key, SMTP password หรือ cloud credential

## Secrets

ใช้ลำดับนี้ตาม environment:

- local development: user secrets หรือ `.env` ที่ไม่ commit
- CI/CD: GitHub Actions secrets หรือ secret store ของ platform
- production: cloud secret manager หรือ environment variables ที่ platform จัดการ

ค่าเหล่านี้ต้องมาจาก environment ตอน deploy:

```text
ConnectionStrings__DefaultConnection
Jwt__SigningKey
Cors__AllowedOrigins__0
```

ใน repository ให้ commit ได้เฉพาะ `.env.example` ที่มีชื่อ key และ placeholder เท่านั้น:

```text
MSSQL_SA_PASSWORD=Replace_With_Strong_Local_Password_123!
JWT_SIGNING_KEY=replace-with-local-development-signing-key-at-least-32-bytes
```

ไฟล์ `.env` จริงต้องอยู่ใน `.gitignore` และไม่ควรถูกแนบไปกับ pull request หรือ release artifact

## CI/CD gate ขั้นต่ำ

ก่อน deploy production ควรมี pipeline ที่รันอย่างน้อย:

```yaml
name: backend-api

on:
  push:
    branches: [main]
  pull_request:

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore Backend.Api.slnx
      - run: dotnet test Backend.Api.slnx --no-restore
      - run: docker build -t backend-api-ci -f Backend.Api/Dockerfile .
```

## Deployment gate

อย่า deploy production ถ้า:

- test ไม่ผ่าน
- migration ยังไม่ได้ review
- ไม่มี backup ก่อน migration สำคัญ
- secret ยังเป็นค่า development
- CORS ยังเปิดกว้าง
- JWT signing key สั้นหรือใช้ซ้ำกับ environment อื่น

CI/CD ที่ดีไม่ได้มีหน้าที่ deploy เร็วอย่างเดียว แต่ต้องช่วยกันไม่ให้ของเสี่ยงหลุดขึ้น production

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- ไม่มี production secret ใน repository
- local ใช้ user secrets หรือ `.env` ที่ไม่ commit
- CI/CD รับ secret จาก platform secret store
- pipeline รัน restore, test และ docker build
- deployment gate ตรวจ migration, backup, CORS และ JWT signing key
