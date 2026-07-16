---
title: '0002 - Progressive and Final Projects'
description: เหตุผลในการมีทั้ง validation/progressive project และ final project
---

# 0002 - Progressive and Final Projects

## Status

Accepted

## Decision

repo ต้องมีสอง project tracks:

- `examples/validation/progressive-backend-api` สำหรับทำตามหนังสือทีละบท
- `examples/final-backend-api` สำหรับ end-state ที่สะอาดและ production-oriented กว่า

## Context

หนังสือที่สอนทีละบทต้องค่อย ๆ เพิ่ม concept แต่ final reference ควรแสดงโครงที่พร้อมต่อยอดจริงกว่า ถ้าใช้ project เดียวอาจเกิดปัญหา: บทต้นเห็น field/feature ที่ยังไม่ได้สอน หรือ final code ถูกจำกัดด้วยลำดับการสอนมากเกินไป

## Rules

- chapter content ต้องตรงกับ progressive project ณ จุดนั้น
- final project ใช้เป็น source of truth สำหรับ end-state
- ความต่างระหว่าง progressive และ final ต้องอธิบายในบทที่เกี่ยวข้อง
- validation report ต้องบันทึกผลทำตามจริง
