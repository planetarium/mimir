namespace Mimir.Worker;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundService<T>(this IServiceCollection services)
        where T : BackgroundService
    {
        return services.AddSingleton<T>()
            .AddHostedService<T>(provider => provider.GetRequiredService<T>());
    }
}
