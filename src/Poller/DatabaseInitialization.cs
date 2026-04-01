using Microsoft.EntityFrameworkCore;
using Poller.Storage;

namespace Poller;

public sealed class DatabaseInitialization : IHostedService
{
    private readonly IDbContextFactory<TickDbContext> _factory;

    public DatabaseInitialization(IDbContextFactory<TickDbContext> factory)
    {
        _factory = factory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var db = await _factory.CreateDbContextAsync(cancellationToken);
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
