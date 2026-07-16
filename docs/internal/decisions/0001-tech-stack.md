---
title: '0001 - Tech Stack'
description: เหตุผลในการเลือก stack หลักของหนังสือ
---

# 0001 - Tech Stack

## Status

Accepted

## Decision

หนังสือใช้ stack หลัก:

- .NET 10
- ASP.NET Core Web API แบบ Controllers
- EF Core
- SQL Server
- JWT Bearer Authentication
- xUnit integration tests
- Docker Compose
- Astro/Starlight สำหรับเว็บไซต์หนังสือ

## Context

เป้าหมายคือสอน backend API ที่ใกล้งานจริง มี authentication, admin, database, tests และ deployment path โดยยังให้มือใหม่ทำตามได้ทีละบท

## Consequences

ข้อดี:

- stack สอดคล้องกับ enterprise .NET backend
- Controllers เหมาะกับการสอน route/action/status code
- EF Core + SQL Server ทำ migration/database flow ชัด
- Docker Compose ช่วยสอน runtime environment

ข้อเสีย:

- setup หนักกว่า Minimal API หรือ in-memory tutorial
- SQL Server ต้องใช้ local install หรือ Docker
- architecture ดูเยอะกว่าบาง framework เช่น Django DRF
