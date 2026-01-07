using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Mimir.Worker.ActionHandler;
using Mimir.Worker.Handler;
using Mimir.Worker.Handler.Balance;
using Mimir.Worker.Initializer;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Constants;
using Mimir.MongoDB.Services;
using Nekoyume.TableData;

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

                // InfiniteTowerInfo Handlers (동적 등록)
                RegisterInfiniteTowerInfoHandlers(builder);
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

    private static void RegisterInfiniteTowerInfoHandlers(HostApplicationBuilder builder)
    {
        // 서비스 빌드 후 실행 시점에 ScheduleSheet를 가져와서 핸들러 등록
        // InitializerManager가 TableSheet 초기화를 보장하므로 안전
        builder.Services.AddSingleton<InfiniteTowerInfoHandlerManager>();
        builder.Services.AddHostedService(serviceProvider =>
        {
            var manager = serviceProvider.GetRequiredService<InfiniteTowerInfoHandlerManager>();
            return manager;
        });
    }
}

internal class InfiniteTowerInfoHandlerManager : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInitializerManager _initializerManager;
    private readonly IMongoDbService _dbService;
    private readonly IStateService _stateService;
    private readonly IHeadlessGQLClient _headlessGqlClient;
    private readonly IStateGetterService _stateGetter;
    private readonly Serilog.ILogger _logger;
    private readonly List<InfiniteTowerInfoStateHandler> _handlers = new();

    public InfiniteTowerInfoHandlerManager(
        IServiceProvider serviceProvider,
        IInitializerManager initializerManager,
        IMongoDbService dbService,
        IStateService stateService,
        IHeadlessGQLClient headlessGqlClient,
        IStateGetterService stateGetter)
    {
        _serviceProvider = serviceProvider;
        _initializerManager = initializerManager;
        _dbService = dbService;
        _stateService = stateService;
        _headlessGqlClient = headlessGqlClient;
        _stateGetter = stateGetter;
        _logger = Serilog.Log.ForContext<InfiniteTowerInfoHandlerManager>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Initializer가 완료될 때까지 대기
        await _initializerManager.WaitInitializers(stoppingToken);

        try
        {
            // 현재 플래닛의 ScheduleSheet 가져오기 (자동으로 올바른 DB에서 가져옴)
            var scheduleSheet = await _dbService.GetSheetAsync<InfiniteTowerScheduleSheet>(stoppingToken);
            if (scheduleSheet is null)
            {
                _logger.Warning("InfiniteTowerScheduleSheet not found. InfiniteTowerInfo handlers will not be registered.");
                return;
            }

            // 모든 infiniteTowerId 추출
            var infiniteTowerIds = scheduleSheet.Values.Select(row => row.InfiniteTowerId).Distinct().ToArray();
            _logger.Information(
                "Registering InfiniteTowerInfo handlers for {Count} tower IDs: {Ids}",
                infiniteTowerIds.Length,
                string.Join(", ", infiniteTowerIds)
            );

            // 각 ID마다 핸들러 생성 및 실행
            foreach (var infiniteTowerId in infiniteTowerIds)
            {
                var handler = new InfiniteTowerInfoStateHandler(
                    infiniteTowerId,
                    _dbService,
                    _stateService,
                    _headlessGqlClient,
                    _initializerManager,
                    _stateGetter
                );
                _handlers.Add(handler);

                // 백그라운드에서 실행
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await handler.StartAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error in InfiniteTowerInfoStateHandler for ID {InfiniteTowerId}", infiniteTowerId);
                    }
                }, stoppingToken);
            }

            // 핸들러들이 실행되는 동안 대기
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error registering InfiniteTowerInfo handlers");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        // 모든 핸들러 중지
        foreach (var handler in _handlers)
        {
            try
            {
                await handler.StopAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error stopping InfiniteTowerInfoStateHandler");
            }
        }

        await base.StopAsync(cancellationToken);
    }
}
