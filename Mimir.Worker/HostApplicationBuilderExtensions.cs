using Mimir.Worker.ActionHandler;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Handler.Balance;
using Mimir.Worker.Initializer;

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
            builder.Services.AddBackgroundService<ActionPointStateHandler>();   
            builder.Services.AddBackgroundService<AgentStateHandler>();
            builder.Services.AddBackgroundService<AllCombinationSlotStateHandler>();
            builder.Services.AddBackgroundService<AllRuneStateHandler>();
            builder.Services.AddBackgroundService<AvatarStateHandler>();
            builder.Services.AddBackgroundService<CollectionStateHandler>();
            builder.Services.AddBackgroundService<DailyRewardStateHandler>();
            builder.Services.AddBackgroundService<InventoryStateHandler>();
            builder.Services.AddBackgroundService<WorldInformationStateHandler>();
            
            // Balance Handlers
            builder.Services.AddBackgroundService<CrystalBalanceHandler>();
            builder.Services.AddBackgroundService<FreyaBlessingRuneBalanceHandler>();
            builder.Services.AddBackgroundService<FreyaLiberationRuneBalanceHandler>();
            builder.Services.AddBackgroundService<GarageBalanceHandler>();
            builder.Services.AddBackgroundService<MeadBalanceHandler>();
            builder.Services.AddBackgroundService<NcgBalanceHandler>();
            builder.Services.AddBackgroundService<OdinWeaknessRuneBalanceHandler>();
            builder.Services.AddBackgroundService<OdinWisdomRuneBalanceHandler>();
            builder.Services.AddBackgroundService<StakeRuneBalanceHandler>();
        }

        return builder;
    }
    
    public static HostApplicationBuilder ConfigureInitializers(this  HostApplicationBuilder builder)
    {
        builder.Services.AddBackgroundService<TableSheetInitializer>();
        builder.Services.AddBackgroundService<ArenaInitializer>();

        builder.Services.AddSingleton<IInitializerManager, InitializerManager>();

        return builder;
    }
}
