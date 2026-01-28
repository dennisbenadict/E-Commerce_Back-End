# -----------------------------
# Build Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files (paths are relative to the *root of the build context*)
# The build context is 'UserService/UserService.Api'
COPY UserService.Api.csproj UserService.Api/
COPY ../UserService.Application/UserService.Application.csproj UserService.Application/
COPY ../UserService.Infrastructure/UserService.Infrastructure.csproj UserService.Infrastructure/
COPY ../UserService.Domain/UserService.Domain.csproj UserService.Domain/

RUN dotnet restore UserService.Api/UserService.Api.csproj

# Copy full source of UserService ONLY
COPY . ./UserService.Api/ # Copy current directory (UserService.Api) into UserService.Api folder in container
COPY ../UserService.Application/ ./UserService.Application/
COPY ../UserService.Infrastructure/ ./UserService.Infrastructure/
COPY ../UserService.Domain/ ./UserService.Domain/

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
