@echo off
setlocal
cd /d "%~dp0"

start "StockExchange.LaToken" cmd /k dotnet run --project "src\StockExchange.LaToken\StockExchange.LaToken.csproj"
start "StockExchange.CoinBase" cmd /k dotnet run --project "src\StockExchange.CoinBase\StockExchange.CoinBase.csproj"
start "Poller" cmd /k dotnet run --project "src\Poller\Poller.csproj"

endlocal
