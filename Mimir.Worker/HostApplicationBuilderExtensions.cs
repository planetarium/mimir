using Mimir.Worker.ActionHandler;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Handler.Balance;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;

namespace Mimir.Worker;

public static class HostApplicationBuilderExtensions
{
    public static HostApplicationBuilder ConfigureHandlers(this HostApplicationBuilder builder)
    {
        switch (
            builder.Configuration.GetSection("Configuration").GetValue<PollerType>("PollerType")
        )
        {
            case PollerType.BlockPoller:
                builder.Services.AddBackgroundService<BlockHandler>();
                break;
            case PollerType.TxPoller:
                builder.Services.AddBackgroundService<ItemSlotStateHandler>();
                builder.Services.AddBackgroundService<PetStateHandler>();
                builder.Services.AddBackgroundService<PledgeStateHandler>();
                builder.Services.AddBackgroundService<ProductsStateHandler>();
                builder.Services.AddBackgroundService<ProductStateHandler>();
                builder.Services.AddBackgroundService<RaiderStateHandler>();
                builder.Services.AddBackgroundService<StakeStateHandler>();
                builder.Services.AddBackgroundService<RuneSlotStateHandler>();
                builder.Services.AddBackgroundService<TableSheetStateHandler>();
                builder.Services.AddBackgroundService<WorldBossKillRewardRecordStateHandler>();
                builder.Services.AddBackgroundService<WorldBossStateHandler>();
                break;
            case PollerType.DiffPoller:
                builder.Services.AddBackgroundService<ActionPointStateHandler>();
                builder.Services.AddBackgroundService<AgentStateHandler>();
                builder.Services.AddBackgroundService<AllCombinationSlotStateHandler>();
                builder.Services.AddBackgroundService<AllRuneStateHandler>();
                builder.Services.AddBackgroundService<AvatarStateHandler>();
                builder.Services.AddBackgroundService<CollectionStateHandler>();
                builder.Services.AddBackgroundService<DailyRewardStateHandler>();
                builder.Services.AddBackgroundService<AdventureCpStateHandler>();
                builder.Services.AddBackgroundService<ArenaCpStateHandler>();
                // builder.Services.AddBackgroundService<RaidCpStateHandler>();
                // builder.Services.AddBackgroundService<InventoryStateHandler>();
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
                break;
        }

        return builder;
    }

    public static HostApplicationBuilder ConfigureInitializers(this HostApplicationBuilder builder)
    {
        if (
            builder.Configuration.GetSection("Configuration").GetValue<bool?>("EnableInitializing")
            is true
        )
        {
            builder.Services.AddBackgroundService<TableSheetInitializer>();

            builder.Services.AddSingleton<IInitializerManager, DefaultInitializerManager>();
        }
        else
        {
            builder.Services.AddSingleton<IInitializerManager, BypassInitializerManager>();
        }

        return builder;
    }
}
