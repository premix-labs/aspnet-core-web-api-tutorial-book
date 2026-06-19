FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Backend.Api/Backend.Api.csproj Backend.Api/
RUN dotnet restore Backend.Api/Backend.Api.csproj

COPY . .
RUN dotnet publish Backend.Api/Backend.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
USER 1654
ENTRYPOINT ["dotnet", "Backend.Api.dll"]
