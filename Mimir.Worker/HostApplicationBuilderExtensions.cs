using Microsoft.Extensions.Options;
using Mimir.Worker.ActionHandler;
using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Initializer;
using Mimir.Worker.Services;

namespace Mimir.Worker;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder ConfigureHandlers(this  HostApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("Configuration").GetValue<PollerType>("PollerType") is { } pollerType &&
            pollerType == PollerType.TxPoller)
        {
            builder.Services.AddBackgroundService<ArenaStateHandler>();
            builder.Services.AddBackgroundService<ItemSlotStateHandler>();
            builder.Services.AddBackgroundService<PetStateHandler>();
            builder.Services.AddBackgroundService<PledgeStateHandler>();
            builder.Services.AddBackgroundService<PledgeStateHandler>();
            builder.Services.AddBackgroundService<ProductsStateHandler>();
            builder.Services.AddBackgroundService<ProductStateHandler>();
            builder.Services.AddBackgroundService<RaiderStateHandler>();
            builder.Services.AddBackgroundService<RuneSlotStateHandler>();
            builder.Services.AddBackgroundService<StakeStateHandler>();
            builder.Services.AddBackgroundService<TableSheetStateHandler>();
            builder.Services.AddBackgroundService<WorldBossKillRewardRecordStateHandler>();
            builder.Services.AddBackgroundService<WorldBossStateHandler>();
        }
        else
        {
            builder.Services.AddBackgroundService<Worker>();   
        }

        return builder;
    }
    
    public static HostApplicationBuilder ConfigureInitializers(this  HostApplicationBuilder builder)
    {
        builder.Services.AddBackgroundService<TableSheetInitializer>();
        builder.Services.AddBackgroundService<ArenaInitializer>();

        builder.Services.AddSingleton<InitializerManager>();

        return builder;
    }
}
