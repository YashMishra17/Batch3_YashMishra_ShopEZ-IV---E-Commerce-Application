FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ShopEZ.ProductService/ShopEZ.ProductService.csproj", "ShopEZ.ProductService/"]

RUN dotnet restore "ShopEZ.ProductService/ShopEZ.ProductService.csproj" --runtime linux-x64

COPY . .

WORKDIR /src/ShopEZ.ProductService

RUN dotnet publish "ShopEZ.ProductService.csproj" -c Release -r linux-x64 --self-contained false -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

RUN groupadd --system appgroup && useradd --system --gid appgroup appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

EXPOSE 5002

ENV ASPNETCORE_URLS=http://+:5002
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ShopEZ.ProductService.dll"]