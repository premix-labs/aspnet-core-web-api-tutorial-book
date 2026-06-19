---
title: 17 - สร้าง User Entity
description: ออกแบบ User entity สำหรับเก็บข้อมูลผู้ใช้ในฐานข้อมูล
---

Entity คือ class ที่แทนข้อมูลหลักในระบบ และมักถูก map เป็น table ใน database

ในโปรเจกต์นี้ entity แรกคือ `User` เพราะระบบของเราจะต่อยอดไปเป็น register, login, JWT และ admin user management

## สร้างโฟลเดอร์ Models

ที่ root ของโปรเจกต์ `Backend.Api` ให้สร้างโฟลเดอร์นี้

```text
Models/
```

จากนั้นสร้างไฟล์

```text
Models/User.cs
```

## เขียน User entity

```csharp
namespace Backend.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}
```

## อธิบาย property

`Id` คือ primary key ของ user แต่ละคน

`Email` ใช้เป็น username หลักของระบบ และภายหลังเราจะกำหนดให้ไม่ซ้ำกัน

`PasswordHash` ใช้เก็บรหัสผ่านที่ถูก hash แล้ว ห้ามเก็บ plain text password ลง database

`Role` ใช้แยกสิทธิ์ เช่น `User` และ `Admin`

`IsActive` ใช้ปิดบัญชีโดยไม่ต้องลบข้อมูลออกจาก database

`CreatedAtUtc` และ `UpdatedAtUtc` ใช้เก็บเวลาสร้างและแก้ไขข้อมูล โดยใช้ UTC เพื่อให้ระบบไม่ผูกกับ timezone ของเครื่อง server

## ทำไมยังมี PasswordHash ทั้งที่ยังไม่ได้ทำ Login

แม้ระบบ login จะอยู่ในภาค 6 แต่เราใส่ `PasswordHash` ตั้งแต่ตอนออกแบบ entity เพื่อให้ schema ของ table `Users` รองรับระบบ auth ตั้งแต่แรก

ในภาคนี้เราจะยังไม่สร้างรหัสผ่านจริงสำหรับ login จนกว่าจะถึงบท hash password

## ไม่ควรใส่อะไรใน Entity

Entity ไม่ควรมี property สำหรับรับ password ดิบ เช่น

```csharp
public string Password { get; set; } = string.Empty;
```

ถ้า API ต้องรับ password จาก request ให้สร้าง DTO แยก เช่น `RegisterRequest` ในภาค Authentication

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มีไฟล์ `Models/User.cs`
- `User` มี `Id`, `Email`, `PasswordHash`, `Role`, `IsActive`
- ไม่มี property ชื่อ `Password` สำหรับเก็บรหัสผ่านดิบ
