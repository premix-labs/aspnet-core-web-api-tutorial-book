---
title: Security Review Checklist
description: รายการตรวจ security สำหรับหนังสือและตัวอย่าง ASP.NET Core Web API
---

# Security Review Checklist

ใช้ checklist นี้เมื่อแก้ auth, admin, production hardening, Docker หรือ deployment docs

## Secrets

- ไม่มี connection string จริงใน tracked config ของ end-state/final project
- ไม่มี JWT signing key จริงใน repo
- `.env` ถูก ignore และมี `.env.example`
- docs ใช้ placeholder ที่ชัดเจน
- README ไม่สอนให้ commit secret จริง

## Authentication

- password เก็บด้วย hash เท่านั้น
- login failure ไม่ leak ว่า email มีอยู่หรือไม่
- inactive user และ locked user มี behavior ชัดเจน
- access token อายุสั้น
- refresh token rotate และ revoke ได้
- refresh token reuse detection มี test
- logout/session revoke กระทบ token ฝั่ง server จริง

## Authorization

- admin endpoint ใช้ backend authorization ไม่ใช่แค่ frontend/client guard
- no token ได้ `401`
- authenticated แต่ role ไม่พอได้ `403`
- self-protection rules มี test
- role update normalize role ก่อนบันทึก

## Error Handling

- validation error ใช้ `ValidationProblemDetails`
- business error ใช้ `ProblemDetails`
- error response มี `code` และ `traceId`
- production ไม่ส่ง stack trace
- log มี correlation id

## Data Protection

- API ไม่ส่ง `PasswordHash`, token hash หรือ secret field
- audit log ไม่บันทึก access token, refresh token หรือ password
- reset/verification token เก็บเป็น hash และใช้ครั้งเดียว
- token มี expiry

## Infrastructure

- Docker runtime ใช้ non-root user
- `.dockerignore` กัน `.env`, `bin`, `obj`, publish output
- CORS ไม่เปิด wildcard สำหรับ production
- rate limiting และ security headers ถูกเปิดใน end-state
- health checks แยก liveness/readiness

