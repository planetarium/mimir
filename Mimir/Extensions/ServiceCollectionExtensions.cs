using Hangfire;
using Hangfire.Redis;
using Microsoft.Extensions.DependencyInjection;
using Mimir.Options;
using StackExchange.Redis;

namespace Mimir.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHangfireServices(
        this IServiceCollection services,
        HangfireOption hangfireOption)
    {
        var redisConfig = new ConfigurationOptions 
        { 
            DefaultDatabase = hangfireOption.RedisDatabase 
        };

        redisConfig.EndPoints.Add(hangfireOption.RedisHost, hangfireOption.RedisPort);

        if (!string.IsNullOrEmpty(hangfireOption.RedisUsername))
        {
            redisConfig.User = hangfireOption.RedisUsername;
        }

        if (!string.IsNullOrEmpty(hangfireOption.RedisPassword))
        {
            redisConfig.Password = hangfireOption.RedisPassword;
        }

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConfig);
        services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);

        services.AddHangfire(
            (provider, config) =>
            {
                config.UseRedisStorage(
                    connectionMultiplexer,
                    new RedisStorageOptions
                    {
                        Prefix = hangfireOption.RedisPrefix,
                        Db = hangfireOption.RedisDatabase,
                    }
                );
            }
        );

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = hangfireOption.WorkerCount;
        });

        return services;
    }
}
