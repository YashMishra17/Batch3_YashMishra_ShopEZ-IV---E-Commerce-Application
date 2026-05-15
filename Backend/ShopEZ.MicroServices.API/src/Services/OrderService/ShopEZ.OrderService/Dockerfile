FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/OrderService/ShopEZ.OrderService/ShopEZ.OrderService.csproj", "src/Services/OrderService/ShopEZ.OrderService/"]

RUN dotnet restore "src/Services/OrderService/ShopEZ.OrderService/ShopEZ.OrderService.csproj" --runtime linux-x64

COPY . .

WORKDIR /src/src/Services/OrderService/ShopEZ.OrderService

RUN dotnet publish "ShopEZ.OrderService.csproj" -c Release -r linux-x64 --self-contained false -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

WORKDIR /app

RUN groupadd --system appgroup && useradd --system --gid appgroup appuser

COPY --from=build --chown=appuser:appgroup /app/publish .

USER appuser

EXPOSE 5004

ENV ASPNETCORE_URLS=http://+:5004
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ShopEZ.OrderService.dll"]