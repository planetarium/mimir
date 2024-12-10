using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Mimir.Initializer.Initializer;
using Mimir.Initializer.Migrators;

namespace Mimir.Initializer;

public static class HostExtensions
{
    public static async Task RunSnapShotInitializerAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<SnapshotInitializer>();

        var stoppingToken = new CancellationTokenSource().Token;

        await initializer.RunAsync(stoppingToken);
    }

    public static async Task RunProductMigratorAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var productMigrator = scope.ServiceProvider.GetRequiredService<ProductMigrator>();

        var stoppingToken = new CancellationTokenSource().Token;

        await productMigrator.AddCpAndCrystalsToProduct(stoppingToken);
    }
}