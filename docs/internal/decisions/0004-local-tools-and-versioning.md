---
title: '0004 - Local Tools and Versioning'
description: เหตุผลในการใช้ dotnet local tools และล็อก version สำคัญ
---

# 0004 - Local Tools and Versioning

## Status

Accepted

## Decision

ใช้ `dotnet-ef` แบบ local tool ผ่าน `dotnet tool run dotnet-ef` และล็อก package version สำคัญให้สอดคล้องกับ target framework

## Context

ระหว่าง validation พบว่า global `dotnet ef` อาจเป็นคนละ major version กับ EF Core package ทำให้เกิด warning หรือ error ตอน migration

## Consequences

ข้อดี:

- command ทำซ้ำได้ในเครื่องต่าง ๆ
- ลดปัญหา tool version mismatch
- เหมาะกับหนังสือที่ผู้เรียนใช้ environment ต่างกัน

ข้อเสีย:

- ต้องสอน `dotnet tool restore`
- command ยาวกว่า `dotnet ef`
