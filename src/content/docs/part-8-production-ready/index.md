---
title: ภาค 8 - เตรียมโปรเจกต์ให้พร้อมใช้งานจริง
description: Logging, configuration, OpenAPI, testing, Docker และ deployment checklist
---

ภาคนี้ทำให้โปรเจกต์พร้อมส่งงานหรือ deploy มากขึ้น โดยเพิ่ม logging, configuration, OpenAPI, automated tests, Docker และ checklist ก่อน deploy

คำว่า production ready ในหนังสือนี้ไม่ได้แปลว่าระบบพร้อมรับ traffic ระดับใหญ่ทันที แต่หมายถึงโปรเจกต์มีพื้นฐานที่ดีพอสำหรับส่งงานจริง พัฒนาในทีมได้ และต่อยอดสู่ production ได้อย่างมีทิศทาง

## บทในภาคนี้

- บทที่ 41: Logging
- บทที่ 42: Configuration และ Environment Variables
- บทที่ 43: appsettings หลาย environment
- บทที่ 44: OpenAPI และเอกสาร API
- บทที่ 45: Unit Test
- บทที่ 46: Integration Test
- บทที่ 47: Dockerfile
- บทที่ 48: Docker Compose
- บทที่ 49: Build และ Publish
- บทที่ 50: Checklist ก่อน deploy

## สิ่งที่ต้องได้หลังจบภาคนี้

- ระบบมี log ที่ช่วย debug ได้โดยไม่รั่ว secret
- configuration แยก local, development และ production ได้
- secret ถูกส่งผ่าน environment variable หรือ secret store
- OpenAPI document ใช้เป็น contract ของ API ได้
- มี unit test และ integration test เบื้องต้น
- API และ SQL Server รันด้วย Docker Compose ได้
- publish output และ Docker image ถูกตรวจได้ก่อน deploy
- มี checklist ปิดงานก่อนส่งขึ้น production
