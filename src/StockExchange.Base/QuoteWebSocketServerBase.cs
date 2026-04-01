using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace StockExchange.Base
{
    public abstract class QuoteWebSocketServerBase
    {
        protected abstract int ListenPort { get; }

        protected virtual string WebSocketPath => "/ws";

        public abstract string ExchangeLabel { get; }

        protected virtual int TickDelayMinMs => 25;

        protected virtual int TickDelayMaxMs => 85;

        protected abstract string BuildQuoteJson(Random random, string symbol);

        protected virtual string[] Symbols { get; } = ["BTC_USDT", "ETH_USDT", "XRP_USDT", "SOL_USDT"];

        public async Task RunAsync(string[] args, CancellationToken cancellationToken)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseSetting("urls", $"http://127.0.0.1:{ListenPort}");
            var app = builder.Build();
            app.UseWebSockets();
            app.Map(WebSocketPath, HandleAsync);

            var log = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger(GetType());
            log.LogInformation(
                "Mock exchange {Exchange} http://127.0.0.1:{Port}{Path}",
                ExchangeLabel,
                ListenPort,
                WebSocketPath);

            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            await using var _ = cancellationToken.Register(
                static state => ((IHostApplicationLifetime)state!).StopApplication(),
                lifetime);

            await app.RunAsync();
        }

        private async Task HandleAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }

            var socket = await context.WebSockets.AcceptWebSocketAsync();
            try
            {
                while (socket.State == WebSocketState.Open)
                {
                    var symbol = Symbols[Random.Shared.Next(Symbols.Length)];
                    var json = BuildQuoteJson(Random.Shared, symbol);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    await socket.SendAsync(bytes, WebSocketMessageType.Text, true, context.RequestAborted);
                    var delay = Random.Shared.Next(TickDelayMinMs, TickDelayMaxMs + 1);
                    await Task.Delay(delay, context.RequestAborted);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }
}
