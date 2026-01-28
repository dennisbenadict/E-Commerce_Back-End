# -----------------------------
# Build Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only project files (paths are relative to the *root of the build context*)
# The build context is 'ProductService/ProductService.Api'
COPY ProductService.Api.csproj ProductService.Api/
COPY ../ProductService.Application/ProductService.Application.csproj ProductService.Application/
COPY ../ProductService.Infrastructure/ProductService.Infrastructure.csproj ProductService.Infrastructure/
COPY ../ProductService.Domain/ProductService.Domain.csproj ProductService.Domain/

RUN dotnet restore ProductService.Api/ProductService.Api.csproj

# Copy full source of ProductService ONLY
COPY . ./ProductService.Api/ # Copy current directory (ProductService.Api) into ProductService.Api folder in container
COPY ../ProductService.Application/ ./ProductService.Application/
COPY ../ProductService.Infrastructure/ ./ProductService.Infrastructure/
COPY ../ProductService.Domain/ ./ProductService.Domain/

WORKDIR /src/ProductService.Api
RUN dotnet publish -c Release -o /app/publish

# -----------------------------
# Runtime Stage
# -----------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProductService.Api.dll"]

