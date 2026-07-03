---
title: Browser Test Plan
description: แผนตรวจเว็บไซต์หนังสือใน browser ก่อนเผยแพร่
---

# Browser Test Plan

ใช้ checklist นี้เมื่อตรวจเว็บไซต์หนังสือหลัง build หรือก่อน deploy

## Viewports

```text
Desktop: 1440x900
Tablet: 768x1024
Mobile: 390x844
```

## Pages

- หน้าแรก
- book plan
- part index ทุกภาค
- บทแรกของแต่ละภาค
- บทที่มี Mermaid diagram
- บท production hardening สำคัญ
- appendix สำคัญ

## Interactions

- sidebar navigation
- search
- table of contents
- code block readability
- Mermaid rendering
- internal links

## Expected Result

- ภาษาไทยไม่เพี้ยน
- code block ไม่ล้น layout
- table อ่านได้บน mobile
- search คืนผลลัพธ์
- ไม่มี console error สำคัญ

