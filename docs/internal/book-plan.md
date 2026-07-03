---
title: แผนหนังสือ ASP.NET Core Web API
description: สารบัญ ขอบเขต และผลลัพธ์สุดท้ายของหนังสือ ASP.NET Core Web API
---

# แผนหนังสือ ASP.NET Core Web API

หนังสือเล่มนี้สอนสร้าง backend API ด้วย ASP.NET Core จากพื้นฐานไปถึงระบบที่มี database, validation, authentication, admin, testing, Docker และ production hardening

## กลุ่มผู้อ่าน

- ผู้เริ่มต้น ASP.NET Core Web API
- ผู้ที่รู้ C# พื้นฐานและอยากทำ backend API จริง
- ผู้ที่ต้องการ portfolio backend project ที่มี auth, admin และ database
- ผู้ที่อยากเข้าใจ flow แบบ Controller, Service, Repository, DTO และ EF Core

## ผลลัพธ์สุดท้าย

ผู้เรียนควรได้ backend API ที่มี:

- CRUD user API
- EF Core + SQL Server
- validation และ global error handling
- register/login
- JWT authentication
- current user endpoint
- role-based authorization
- admin user management
- audit log
- pagination/filtering/sorting
- unit และ integration tests
- Dockerfile และ Docker Compose
- production hardening เช่น refresh token, lockout, email verification, reset password, CORS, rate limiting, security headers และ health checks

## Part 1: Foundation

1. ASP.NET Core Web API คืออะไร
2. ติดตั้งเครื่องมือ
3. สร้างโปรเจกต์แรก
4. เข้าใจ project structure
5. HTTP, REST และ JSON

## Part 2: REST API

6. สร้าง Controller แรก
7. CRUD actions
8. Routing และ parameters
9. Request, response และ status code
10. ทดสอบ API ด้วย REST Client หรือ Postman

## Part 3: Architecture

11. Controller, Service, Repository
12. Dependency Injection
13. DTO
14. Mapping
15. Response format

## Part 4: Database

16. ติดตั้ง EF Core
17. สร้าง User entity
18. สร้าง DbContext
19. ตั้งค่า connection string
20. ใช้ migration
21. CRUD กับ database
22. Seed data

## Part 5: Validation and Errors

23. Data Annotations
24. Custom validation
25. Global exception handler
26. Error response format
27. HTTP status codes

## Part 6: Authentication

28. Register/login design
29. Hash password
30. Login API
31. JWT token
32. Current user
33. Authorize endpoints

## Part 7: Admin

34. Role and permission design
35. Admin endpoint
36. Admin user list
37. Change role/status
38. Admin self-protection
39. Audit log
40. Pagination, filtering และ sorting

## Part 8: Production Ready

41. Logging
42. Configuration and environment variables
43. Appsettings environments
44. OpenAPI docs
45. Unit test
46. Integration test
47. Dockerfile
48. Docker Compose
49. Build and publish
50. Deploy checklist

## Part 9: Production Hardening

51. Production user model
52. Refresh token rotation
53. Account lockout and password policy
54. Email verification and reset password
55. CORS, rate limiting and security headers
56. Health checks and readiness
57. Secrets and CI/CD
58. Observability, backup and runbook

