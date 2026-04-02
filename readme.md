### Константин Гонсовский — тестовое задание для Indigo Software

**Как запустить**

X Mock'и бирж и Y фид грабберов, которые к ним подключаются и пишут тики в хранилище.

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

**Потраченные часы**
7 часов + 1 час на следующий день (эффект свежего взгляда)

**От себя**
В качестве тест фреймворка используется паблик форк XUnit в моем авторстве обеспечивающий параллельное выполнение всех фактов и теорий.

