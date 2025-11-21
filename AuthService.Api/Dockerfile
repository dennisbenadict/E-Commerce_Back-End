# -----------------------------
# Build Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY AuthService.Domain/AuthService.Domain.csproj AuthService.Domain/
COPY AuthService.Application/AuthService.Application.csproj AuthService.Application/
COPY AuthService.Infrastructure/AuthService.Infrastructure.csproj AuthService.Infrastructure/
COPY AuthService.Api/AuthService.Api.csproj AuthService.Api/

RUN dotnet restore AuthService.Api/AuthService.Api.csproj

COPY . .
WORKDIR /src/AuthService.Api
RUN dotnet publish -c Release -o /app/publish

# -----------------------------
# Runtime Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AuthService.Api.dll"]
