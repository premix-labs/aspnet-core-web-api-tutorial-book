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
