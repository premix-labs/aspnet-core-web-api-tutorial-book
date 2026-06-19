---
title: "58. Observability, Backup และ Runbook"
description: "ทำให้ทีมรู้ว่า production มีปัญหาอะไร และกู้คืนได้เมื่อเกิดเหตุ"
---

ระบบ production ที่ดีต้องตอบคำถามหลังเกิดปัญหาได้ ไม่ใช่แค่หวังว่า code จะไม่พัง

## Observability ขั้นต่ำ

ควรมีอย่างน้อย:

- structured logs พร้อม `traceId`
- response header `X-Correlation-Id` เพื่อให้ client แจ้ง id เดียวกับ log ได้
- request duration
- error rate
- database connection error
- login failure rate
- refresh token reuse/revoke event
- health check status

log ไม่ควรเก็บ password, token, refresh token หรือข้อมูลลับอื่น ๆ

ใน final project และ validation/progressive project มี middleware ที่อ่าน `X-Correlation-Id` จาก request ถ้ามี ถ้าไม่มีจะใช้ `HttpContext.TraceIdentifier` แล้วส่งกลับใน response header ชื่อเดียวกัน พร้อมเปิด logging scope `CorrelationId` ให้ log ใน request เดียวกันค้นหาได้ง่ายขึ้น

## Metrics ที่ควร monitor

- HTTP 5xx rate
- HTTP 401/403/429 rate
- p95/p99 latency
- database CPU/connection pool
- failed login count
- account lockout count
- migration failure

ถ้าใช้ OpenTelemetry สามารถส่ง trace/metrics ไปยัง backend เช่น Grafana Tempo, Jaeger, Azure Monitor, Datadog หรือ New Relic

## Backup และ restore

backup ที่ไม่เคยลอง restore ยังไม่น่าเชื่อถือ สำหรับ production ควรกำหนด:

- backup frequency
- retention policy
- encryption
- restore drill
- point-in-time recovery ถ้าฐานข้อมูลรองรับ

ก่อน migration ที่เปลี่ยน schema สำคัญ ต้องมี backup หรือ rollback plan ที่ชัดเจน

## Runbook

runbook คือเอกสารสั้น ๆ สำหรับตอนเกิดเหตุ ไม่ใช่เอกสารสวย ๆ ที่ไม่มีใครเปิด ตอน production มีปัญหา ทีมควรรู้ทันทีว่าต้องทำอะไร

ตัวอย่างหัวข้อ runbook:

- API 5xx สูง ต้องดู log/query/connection string จุดไหนก่อน
- database migration fail ต้อง rollback หรือ restore อย่างไร
- JWT signing key รั่ว ต้อง rotate key และ revoke session อย่างไร
- login ถูก brute force ต้องเพิ่ม rate limit หรือ block IP อย่างไร
- deploy แล้ว readiness fail ต้อง rollback image ไหน
- user แจ้งปัญหาพร้อม `X-Correlation-Id` ต้องค้น log ด้วย id นั้นอย่างไร

เมื่อมี runbook ระบบไม่ได้แค่ production-ready ทาง code แต่ทีมพร้อม operate ระบบจริงด้วย

## Checkpoint

ก่อนจบภาคนี้ ให้ตรวจว่าทำได้ครบตามนี้

- response และ error มี correlation id สำหรับไล่ log
- health checks แยก liveness/readiness
- log ไม่เก็บ password, token หรือ secret
- มีแนวทาง backup/restore และ restore drill
- runbook ระบุขั้นตอนสำหรับ incident สำคัญ
