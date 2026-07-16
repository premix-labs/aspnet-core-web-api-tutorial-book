---
title: API Contract
description: API surface หลักที่หนังสือและ frontend ต่อไปควรยึดเป็น contract
---

# API Contract

เอกสารนี้สรุป endpoint และ response shape หลักของ final backend API เพื่อใช้ตรวจความสอดคล้องของหนังสือ, final project, validation project และ frontend book ที่จะต่อยอด

## Auth

```text
POST /api/v1/auth/register
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/logout
GET  /api/v1/auth/me
GET  /api/v1/auth/sessions
DELETE /api/v1/auth/sessions/{familyId}
DELETE /api/v1/auth/sessions
```

ตัวอย่าง `LoginRequest`:

```json
{
  "email": "admin@example.com",
  "password": "Passw0rd!"
}
```

ตัวอย่าง `LoginResponse`:

```json
{
  "accessToken": "jwt-token",
  "refreshToken": "refresh-token",
  "user": {
    "id": "user-id",
    "email": "admin@example.com",
    "role": "Admin"
  }
}
```

## Users

```text
GET    /api/v1/users
GET    /api/v1/users/{id}
POST   /api/v1/users
PUT    /api/v1/users/{id}
DELETE /api/v1/users/{id}
```

ช่วงต้นเล่ม endpoint เหล่านี้ใช้สอน CRUD และ architecture ก่อนจะถูกจำกัดสิทธิ์ในบท auth/admin

## Admin Users

```text
GET   /api/v1/admin/users
GET   /api/v1/admin/users/{id}
PATCH /api/v1/admin/users/{id}/role
PATCH /api/v1/admin/users/{id}/status
```

query หลัก:

```text
page
pageSize
search
role
isActive
sortBy
sortDirection
```

ตัวอย่าง paged response:

```json
{
  "items": [
    {
      "id": "user-id",
      "email": "admin@example.com",
      "role": "Admin",
      "isActive": true,
      "createdAt": "2026-07-03T00:00:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalItems": 1,
  "totalPages": 1
}
```

## Audit Logs

```text
GET /api/v1/admin/audit-logs
```

audit log ต้องไม่บันทึก password, token หรือ secret ลง `detail`

## Error Contract

API ควรใช้ `ProblemDetails` และ `ValidationProblemDetails`

ตัวอย่าง validation error:

```json
{
  "type": "https://example.com/errors/validation",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "code": "VALIDATION_FAILED",
  "traceId": "trace-id",
  "errors": {
    "email": ["Email is required."]
  }
}
```

status code ที่ต้องรักษา:

```text
400 validation error
401 no/invalid token
403 authenticated but forbidden
404 resource not found
409 conflict เช่น email ซ้ำ
500 unexpected server error
```
