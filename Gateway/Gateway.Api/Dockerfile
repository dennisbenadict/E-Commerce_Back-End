# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY Gateway/Gateway.Api/Gateway.Api.csproj Gateway/Gateway.Api/
RUN dotnet restore Gateway/Gateway.Api/Gateway.Api.csproj

# Copy everything else and build
COPY Gateway/Gateway.Api/ Gateway/Gateway.Api/
WORKDIR /src/Gateway/Gateway.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Gateway.Api.dll"]

