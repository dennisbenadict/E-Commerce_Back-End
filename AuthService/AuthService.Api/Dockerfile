# -----------------------------
# Build Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files
COPY AuthService/AuthService.Api/AuthService.Api.csproj AuthService.Api/
COPY AuthService/AuthService.Application/AuthService.Application.csproj AuthService.Application/
COPY AuthService/AuthService.Infrastructure/AuthService.Infrastructure.csproj AuthService.Infrastructure/
COPY AuthService/AuthService.Domain/AuthService.Domain.csproj AuthService.Domain/

RUN dotnet restore AuthService.Api/AuthService.Api.csproj

# Copy full source (ONLY AuthService)
COPY AuthService/AuthService.Api/ AuthService.Api/
COPY AuthService/AuthService.Application/ AuthService.Application/
COPY AuthService/AuthService.Infrastructure/ AuthService.Infrastructure/
COPY AuthService/AuthService.Domain/ AuthService.Domain/

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
