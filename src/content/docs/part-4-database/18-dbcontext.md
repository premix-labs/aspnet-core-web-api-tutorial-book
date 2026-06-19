---
title: 18 - สร้าง DbContext
description: สร้าง AppDbContext และกำหนด mapping ของ User entity
---

`DbContext` คือ class หลักของ EF Core ใช้เป็นตัวกลางระหว่าง application กับ database

ถ้าเปรียบแบบง่าย ๆ `DbContext` คือจุดที่บอก EF Core ว่าโปรเจกต์นี้มี entity อะไรบ้าง และ entity เหล่านั้นควรถูก map กับ database อย่างไร

## สร้างโฟลเดอร์ Data

ที่ root ของโปรเจกต์ `Backend.Api` ให้สร้างโฟลเดอร์นี้

```text
Data/
```

จากนั้นสร้างไฟล์

```text
Data/AppDbContext.cs
```

## เขียน AppDbContext

```csharp
using Microsoft.EntityFrameworkCore;
using Backend.Api.Models;

namespace Backend.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(user => user.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(user => user.CreatedAtUtc)
                .IsRequired();
        });
    }
}
```

## อธิบาย code

`DbSet<User> Users` บอก EF Core ว่าเราต้องการจัดการ entity `User` ผ่าน table ชื่อ `Users`

`OnModelCreating` ใช้กำหนดรายละเอียดของ schema เช่นความยาว column, required field และ unique index

`HasIndex(user => user.Email).IsUnique()` ทำให้ email ซ้ำกันไม่ได้ใน database ซึ่งสำคัญมากสำหรับระบบ login

## ทำไมต้องกำหนด MaxLength

ถ้าไม่กำหนดความยาว string EF Core อาจสร้าง column เป็นชนิดที่กว้างเกินจำเป็น เช่น `nvarchar(max)`

สำหรับ field อย่าง email และ role เรารู้ขนาดโดยประมาณ จึงกำหนด `HasMaxLength` เพื่อให้ database schema ชัดเจนกว่า

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มีไฟล์ `Data/AppDbContext.cs`
- มี `DbSet<User> Users`
- email ถูกกำหนดเป็น unique index
- string field สำคัญมี `HasMaxLength`
