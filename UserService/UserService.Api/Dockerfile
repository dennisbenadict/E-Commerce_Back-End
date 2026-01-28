# -----------------------------
# Build Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files
COPY UserService/UserService.Api/UserService.Api.csproj UserService.Api/
COPY UserService/UserService.Application/UserService.Application.csproj UserService.Application/
COPY UserService/UserService.Infrastructure/UserService.Infrastructure.csproj UserService.Infrastructure/
COPY UserService/UserService.Domain/UserService.Domain.csproj UserService.Domain/

RUN dotnet restore UserService.Api/UserService.Api.csproj

# Copy full source of UserService ONLY
COPY UserService/UserService.Api/ UserService.Api/
COPY UserService/UserService.Application/ UserService.Application/
COPY UserService/UserService.Infrastructure/ UserService.Infrastructure/
COPY UserService/UserService.Domain/ UserService.Domain/

WORKDIR /src/UserService.Api
RUN dotnet publish -c Release -o /app/publish

# -----------------------------
# Runtime Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "UserService.Api.dll"]
