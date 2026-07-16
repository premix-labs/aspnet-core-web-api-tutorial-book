---
title: '0003 - Auth and Security Model'
description: เหตุผลของแนวทาง auth, token และ production hardening
---

# 0003 - Auth and Security Model

## Status

Accepted

## Decision

หนังสือใช้ JWT access token พร้อม refresh token hardening ใน end-state:

- password hashing ด้วย `PasswordHasher<TUser>`
- access token อายุสั้น
- refresh token rotation
- refresh token reuse detection
- session/device management
- account lockout
- email verification
- password reset
- audit log สำหรับ auth/security events

## Context

ระบบ admin ต้องมี auth behavior ที่จริงจังพอให้ผู้เรียนเห็นความต่างระหว่าง demo login กับ production-oriented auth

## Consequences

ข้อดี:

- สอน security tradeoff ได้ชัด
- final project เป็น reference ที่น่าเชื่อถือ
- มี integration tests รองรับ flow สำคัญ

ข้อเสีย:

- เนื้อหายาวและซับซ้อนขึ้น
- ต้องระวัง progressive chapters ไม่กระโดดไป production hardening ก่อนเวลา
