# -----------------------------
# Build Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files (paths are relative to the *root of the build context*)
# The build context is 'AuthService/AuthService.Api' due to 'cd AuthService/AuthService.Api'
# So, to copy AuthService.Api.csproj, we just need its name.
# For other projects like AuthService.Application, we need to go up one level and then down.
COPY AuthService.Api.csproj AuthService.Api/
COPY ../AuthService.Application/AuthService.Application.csproj AuthService.Application/
COPY ../AuthService.Infrastructure/AuthService.Infrastructure.csproj AuthService.Infrastructure/
COPY ../AuthService.Domain/AuthService.Domain.csproj AuthService.Domain/

RUN dotnet restore AuthService.Api/AuthService.Api.csproj

# Copy full source (ONLY AuthService)
COPY . ./AuthService.Api/ # Copy current directory (AuthService.Api) into AuthService.Api folder in container
COPY ../AuthService.Application/ ./AuthService.Application/
COPY ../AuthService.Infrastructure/ ./AuthService.Infrastructure/
COPY ../AuthService.Domain/ ./AuthService.Domain/

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
