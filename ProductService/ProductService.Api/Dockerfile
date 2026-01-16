# -----------------------------
# Build Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files (relative to ProductService root)
COPY ProductService.Api/ProductService.Api.csproj ProductService.Api/
COPY ProductService.Application/ProductService.Application.csproj ProductService.Application/
COPY ProductService.Infrastructure/ProductService.Infrastructure.csproj ProductService.Infrastructure/
COPY ProductService.Domain/ProductService.Domain.csproj ProductService.Domain/

RUN dotnet restore ProductService.Api/ProductService.Api.csproj

# Copy full source
COPY ProductService.Api/ ProductService.Api/
COPY ProductService.Application/ ProductService.Application/
COPY ProductService.Infrastructure/ ProductService.Infrastructure/
COPY ProductService.Domain/ ProductService.Domain/

WORKDIR /src/ProductService.Api
RUN dotnet publish -c Release -o /app/publish

# -----------------------------
# Runtime Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProductService.Api.dll"]


