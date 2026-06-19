---
title: 47 - Dockerfile
description: สร้าง Docker image สำหรับ ASP.NET Core Web API ด้วย multi-stage build
---

Dockerfile ใช้กำหนดขั้นตอน build และ run API ใน container

สำหรับ ASP.NET Core เราจะใช้ multi-stage build โดยใช้ image `sdk` สำหรับ build/publish และใช้ image `aspnet` สำหรับ runtime

## สร้าง .dockerignore

ในบทนี้ให้สร้างไฟล์ Docker สำหรับ API project โดยตรง ดังนั้นไฟล์ `.dockerignore` และ `Dockerfile` จะอยู่ในโฟลเดอร์ `Backend.Api/`

สร้างไฟล์

```text
Backend.Api/.dockerignore
```

เพิ่มรายการนี้

```text
bin/
obj/
.git/
.vs/
.vscode/
TestResults/
publish/
.env
.env.*
```

ไฟล์นี้ช่วยลด build context ไม่ให้ Docker copy ไฟล์ที่ไม่จำเป็นเข้าไปตอน build image

## สร้าง Dockerfile

สร้างไฟล์

```text
Backend.Api/Dockerfile
```

เพิ่ม code นี้

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Backend.Api.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish Backend.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER 1654

ENTRYPOINT ["dotnet", "Backend.Api.dll"]
```

## อธิบายแต่ละ stage

`build` stage ใช้ `mcr.microsoft.com/dotnet/sdk:10.0` เพราะต้องใช้ SDK สำหรับ restore, build และ publish

`runtime` stage ใช้ `mcr.microsoft.com/dotnet/aspnet:10.0` เพราะตอนรันต้องการแค่ ASP.NET Core runtime ไม่ต้องมี SDK

การแยก stage ทำให้ final image เล็กลงและเหมาะกับการ deploy มากกว่าเอา SDK image ไปรัน production

`USER 1654` ทำให้ container runtime ไม่รันด้วย root user ลดผลกระทบถ้า process ถูกเจาะ สำคัญคือต้องตั้งหลัง `COPY --from=build` เพื่อให้ build stage ยังทำงานได้ตามปกติ และแอปต้องไม่พึ่งการเขียนไฟล์ลง `/app` ตอน runtime

## Build image

รันคำสั่งจากโฟลเดอร์ `Backend.Api`

```powershell
cd Backend.Api
docker build -t backend-api:dev .
```

## Run container

ถ้ายังไม่เชื่อม database ให้ลองรันเพื่อดูว่า app start ได้หรือไม่

```powershell
$env:LOCAL_SQL_PASSWORD="Replace_With_Strong_Local_Password_123!"
$env:JWT_SIGNING_KEY="replace-with-local-development-signing-key-at-least-32-bytes"

docker run --rm -p 18080:8080 `
  -e ASPNETCORE_ENVIRONMENT=Production `
  -e "ConnectionStrings__DefaultConnection=Server=host.docker.internal,1433;Database=BackendApiDb;User Id=sa;Password=$($env:LOCAL_SQL_PASSWORD);TrustServerCertificate=True;" `
  -e DataSeeding__Enabled=false `
  -e Jwt__Issuer=Backend.Api `
  -e Jwt__Audience=Backend.ApiClient `
  -e "Jwt__SigningKey=$env:JWT_SIGNING_KEY" `
  -e Jwt__ExpirationMinutes=60 `
  backend-api:dev
```

ตัวอย่างนี้ปิด `DataSeeding` เพราะยังไม่ได้รัน migration และยังไม่มี SQL Server ใน container ชุดเดียวกัน จุดประสงค์คือทดสอบว่า image start ได้ก่อน

ถ้า application ต้องเชื่อม database จริง ให้ใช้ Docker Compose ในบทถัดไป เพราะต้องมีทั้ง API และ SQL Server

## Checkpoint

ก่อนอ่านบทต่อไป ให้ตรวจว่าทำได้ครบตามนี้

- มี `.dockerignore`
- มี `Dockerfile`
- เข้าใจว่า Dockerfile ในบทนี้อยู่ใน `Backend.Api/`
- ใช้ multi-stage build
- runtime image ใช้ `mcr.microsoft.com/dotnet/aspnet:10.0`
- runtime container ไม่รันด้วย root user
- build image ด้วย `docker build` ได้
- run container ด้วย environment variables ที่จำเป็นได้
