using Microsoft.Extensions.Options;
using Mimir.Worker.ActionHandler;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;

namespace Mimir.Worker;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder ConfigureHandlers(this  HostApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("Configuration").GetValue<PollerType>("PollerType") is { } pollerType &&
            pollerType == PollerType.TxPoller)
        {
            builder.Services.AddHostedService<ArenaStateHandler>();
            builder.Services.AddHostedService<ItemSlotStateHandler>();
            builder.Services.AddHostedService<PetStateHandler>();
            builder.Services.AddHostedService<PledgeStateHandler>();
            builder.Services.AddHostedService<PledgeStateHandler>();
            builder.Services.AddHostedService<ProductsStateHandler>();
            builder.Services.AddHostedService<ProductStateHandler>();
            builder.Services.AddHostedService<RaiderStateHandler>();
            builder.Services.AddHostedService<RuneSlotStateHandler>();
            builder.Services.AddHostedService<StakeStateHandler>();
            builder.Services.AddHostedService<TableSheetStateHandler>();
            builder.Services.AddHostedService<WorldBossKillRewardRecordStateHandler>();
            builder.Services.AddHostedService<WorldBossStateHandler>();
        }
        else
        {
            builder.Services.AddHostedService(serviceProvider =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<Configuration>>().Value;
    
                AddressHandlerMappings.RegisterCurrencyHandler(PlanetType.FromString(config.PlanetType));

                if (config.PollerType == PollerType.TxPoller)
                {
                    throw new InvalidOperationException();
                }

                return new Worker(serviceProvider, config.PollerType, config.EnableInitializing);
            });   
        }

        return builder;
    }
}
