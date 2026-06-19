---
title: "51. Production-grade User Model"
description: "ออกแบบ User model ให้รองรับ auth, audit, lockout, token rotation และ database constraint"
---

User model สำหรับ production ต้องเก็บมากกว่า email/password/role เพราะระบบจริงต้องตอบคำถามเหล่านี้ได้:

- email นี้ซ้ำกับ user เดิมไหม ถ้าตัวพิมพ์ใหญ่เล็กต่างกัน
- account ถูก lock เพราะ login ผิดหลายครั้งหรือไม่
- password ถูกเปลี่ยนล่าสุดเมื่อไร
- email verified แล้วหรือยัง
- refresh token ใดถูก revoke หรือ rotate แล้ว
- update ชนกันจากหลาย request หรือหลาย admin หรือไม่

ใน final project เราปรับ `User` ให้มี field สำคัญเพิ่ม เช่น `NormalizedEmail`, `IsEmailVerified`, `AccessFailedCount`, `LockoutEnd`, `LastLoginAt`, `PasswordChangedAt`, `RowVersion` และ relation ไปยัง `RefreshTokens`

จุดสำคัญคือ model ในภาคนี้ไม่ใช่การคัดลอกทับ `User` จากบท 17 แบบตรง ๆ เพราะบทก่อนหน้านี้ยังใช้ `int Id`, `CreatedAtUtc` และ `UpdatedAtUtc` อยู่ ส่วน final project ยกระดับเป็น `Guid Id`, `DateTimeOffset` และชื่อ field แบบ production เช่น `CreatedAt` การเปลี่ยนนี้ต้องทำพร้อม repository, DTO, controller, current user claim, audit log และ migration ไม่เช่นนั้น project จะ compile ไม่ผ่าน

```csharp
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string NormalizedEmail { get; set; }
    public required string PasswordHash { get; set; }
    public required string Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsEmailVerified { get; set; }
    public int AccessFailedCount { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? PasswordChangedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
```

## ลำดับการย้ายจาก progressive model

ถ้าคุณกำลังอัปเกรด project ที่ทำตามบท 1-8 ให้คิดเป็นงาน migration ไม่ใช่แก้ไฟล์เดียว:

1. เปลี่ยน `User.Id` จาก `int` เป็น `Guid` และปรับ DTO/route/current user service ที่อ้าง id ทั้งหมด
2. เปลี่ยน timestamp จาก `CreatedAtUtc`/`UpdatedAtUtc` เป็น `CreatedAt`/`UpdatedAt` แบบ `DateTimeOffset`
3. เพิ่ม `NormalizedEmail` และให้ register/seed data เขียนค่านี้ทุกครั้ง
4. เพิ่ม field security เช่น `IsEmailVerified`, `AccessFailedCount`, `LockoutEnd`, `LastLoginAt`, `PasswordChangedAt` และ `RowVersion`
5. ปรับ EF Core mapping และ index ให้ตรงกับ query จริง
6. เขียน migration ที่ backfill ข้อมูลเดิมก่อนบังคับ `NOT NULL` หรือ unique constraint
7. รัน test ทั้ง auth, admin user list, audit log และ migration ก่อนเพิ่ม refresh token ในบทถัดไป

final project ทำงานเหล่านี้ไว้แล้ว ส่วนบทนี้อธิบายเหตุผลและจุดที่ต้องตรวจเมื่อทำ production hardening

## ทำไมต้องมี NormalizedEmail

ถ้าใช้ `Email` เป็น unique index ตรง ๆ อาจเกิดข้อมูลซ้ำเชิงธุรกิจได้ เช่น `Admin@example.com` และ `admin@example.com` ในบาง collation หรือบาง database อาจเทียบไม่เหมือนกัน วิธีที่ควบคุมได้คือสร้าง `NormalizedEmail` จาก email ที่ trim และแปลงเป็นตัวพิมพ์เดียวกัน แล้วทำ unique index ที่ field นี้

```csharp
entity.HasIndex(user => user.NormalizedEmail).IsUnique();
entity.Property(user => user.NormalizedEmail).HasMaxLength(320).IsRequired();
```

## ทำไมต้องมี RowVersion

`RowVersion` ช่วยทำ optimistic concurrency บน SQL Server ถ้า admin สองคนแก้ user เดียวกันพร้อมกัน ระบบจะรู้ว่า row เปลี่ยนไปแล้ว ไม่ใช่เขียนทับแบบเงียบ ๆ

```csharp
entity.Property(user => user.RowVersion).IsRowVersion();
```

## Index สำหรับ query จริง

นอกจาก unique index ของ `NormalizedEmail` แล้ว final project เพิ่ม index สำหรับ query ที่ระบบใช้บ่อย:

```csharp
entity.HasIndex(user => user.CreatedAt);
entity.HasIndex(user => new { user.Role, user.IsActive, user.CreatedAt });
```

index ชุดนี้ช่วยหน้ารายการ admin user ที่ filter ตาม role/status และเรียงตามเวลาสร้าง รวมถึงช่วย query นับ admin ที่ยัง active

## Migration ต้องคิดถึงข้อมูลเดิม

Production migration ไม่ควรแค่เพิ่ม column แล้วสร้าง unique index ทันที เพราะถ้ามีข้อมูลเดิมอยู่ migration อาจพัง final project จึงแก้ migration ให้เติม `NormalizedEmail` จาก `Email` ก่อน แล้วค่อยบังคับ not null และสร้าง unique index

```sql
UPDATE Users
SET NormalizedEmail = UPPER(LTRIM(RTRIM(Email)))
```

นี่คือความต่างระหว่าง migration สำหรับ demo กับ migration ที่คิดถึง production data จริง

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- `User` มี `NormalizedEmail`, lockout fields, password timestamp และ `RowVersion`
- `NormalizedEmail` มี unique index
- migration เติมข้อมูลเดิมก่อนบังคับ constraint ใหม่
- query สำคัญมี index รองรับ
