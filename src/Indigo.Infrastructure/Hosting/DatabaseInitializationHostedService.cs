using Indigo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Indigo.Infrastructure.Hosting;

public sealed class DatabaseInitializationHostedService : IHostedService
{
    private readonly IDbContextFactory<TickDbContext> _factory;

    public DatabaseInitializationHostedService(IDbContextFactory<TickDbContext> factory)
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
