using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace StockExchange.Base.Hosting
{
    public abstract class QuoteWebSocketServerBase
    {
        public abstract int ListenPort { get; }

        public virtual string WebSocketPath => "/ws";

        public abstract string ExchangeLabel { get; }

        protected virtual int TickDelayMinMs => 25;

        protected virtual int TickDelayMaxMs => 85;

        protected abstract string BuildQuoteJson(Random random, string symbol);

        protected virtual string[] Symbols { get; } = new[] { "BTC_USDT", "ETH_USDT", "XRP_USDT", "SOL_USDT" };

        public async Task RunAsync(string[] args, CancellationToken cancellationToken = default)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.UseSetting("urls", $"http://127.0.0.1:{ListenPort}");
            var app = builder.Build();
            app.UseWebSockets();
            app.Map(WebSocketPath, HandleAsync);
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
