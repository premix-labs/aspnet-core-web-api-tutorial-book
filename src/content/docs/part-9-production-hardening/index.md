---
title: "ภาค 9: Production Hardening"
description: "ยกระดับ Backend API จาก runnable project ให้เป็นระบบที่พร้อมรับความเสี่ยงจริงมากขึ้น"
---

ภาคก่อนหน้าทำให้ API ใช้งานได้จริงในระดับพื้นฐานแล้ว แต่ production ไม่ได้แปลว่าแค่ build ผ่านหรือ deploy ได้ Production คือระบบต้องทนต่อความเสี่ยงที่เกิดขึ้นจริง เช่น token หลุด, brute force login, email ซ้ำเพราะตัวพิมพ์, database schema โตขึ้น, frontend มาจาก origin ที่ควบคุมได้, monitoring ตรวจเจอปัญหา และทีม rollback ได้เมื่อ deploy ผิดพลาด

ในภาคนี้เราจะปรับ project ให้แข็งขึ้นโดยไม่เปลี่ยนแนวทางหลักของเล่ม ตัวอย่าง code อยู่ใน `examples/final-backend-api`

ต่างจากภาค 1-8 ที่เป็น workshop ทีละไฟล์ ภาคนี้เป็น production hardening guide ที่อธิบาย design, migration risk, endpoint ที่เพิ่ม และ test ที่ final project ใช้ยืนยันพฤติกรรมจริง ถ้าต้องการดู implementation ครบ ให้เทียบกับ `examples/final-backend-api` ควบคู่กับบทนี้

## สิ่งที่เพิ่มในภาคนี้

1. Production-grade User model และ database constraint
2. Refresh token rotation และ revoke
3. Account lockout และ password policy
4. Email verification และ reset password flow
5. CORS, rate limiting และ security headers
6. Liveness/readiness health checks
7. Secrets, CI/CD และ deployment gate
8. Observability, backup และ runbook

## ขอบเขตที่ต้องเข้าใจ

หนังสือเล่มนี้ยังเป็น backend API book ไม่ใช่หนังสือ platform engineering เต็มรูปแบบ ดังนั้นบางเรื่องจะสอน pattern และ checklist สำหรับต่อกับระบบจริง เช่น email provider, secret manager, metrics backend และ production database backup แต่ตัวอย่าง final project จะมี baseline ที่รันได้จริงและมี test รองรับส่วนสำคัญ เช่น refresh token, lockout, normalized email, email verification และ reset password
