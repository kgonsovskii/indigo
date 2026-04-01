# Indigo — тестовое задание

Константин Гонсовский — тестовое задание для Indigo Software.

**Как запустить**

Mock-биржи и воркер, который к ним подключается и пишет тики в SQLite.

LaToken — порт **5051**, CoinBase — **5052**. WebSocket — путь **`/ws`**, как в `Poller/appsettings.json`.

Из корня репозитория:

```bash
dotnet run --project src/StockExchange.LaToken/StockExchange.LaToken.csproj
```

```bash
dotnet run --project src/StockExchange.CoinBase/StockExchange.CoinBase.csproj
```

```bash
dotnet run --project src/Poller/Poller.csproj
```
