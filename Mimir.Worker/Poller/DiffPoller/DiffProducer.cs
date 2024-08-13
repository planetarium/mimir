using System.Threading.Channels;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
using Mimir.Worker.Constants;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class DiffProducer
{
    protected readonly MongoDbService _dbService;
    protected readonly IStateService _stateService;
    protected readonly ILogger _logger;
    private readonly IHeadlessGQLClient _headlessGqlClient;

    public DiffProducer(
        IStateService stateService,
        IHeadlessGQLClient headlessGqlClient,
        MongoDbService dbService,
        Address accountAddress
    )
    {
        _headlessGqlClient = headlessGqlClient;
        _stateService = stateService;
        _logger = Log.ForContext<DiffProducer>()
            .ForContext("AccountAddress", accountAddress.ToHex());
        _dbService = dbService;
    }

    public async Task ProduceByAccount(
        ChannelWriter<DiffContext> writer,
        Address accountAddress,
        CancellationToken stoppingToken
    )
    {
        var collectionName = CollectionNames.GetCollectionName(accountAddress);

        var syncedBlockIndex = await GetSyncedBlockIndex(collectionName, stoppingToken);
        var currentTargetIndex = syncedBlockIndex;
        _logger.Information(
            "{CollectionName} Synced BlockIndex: {SyncedBlockIndex}",
            collectionName,
            syncedBlockIndex
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex(stoppingToken);
            var currentBaseIndex = currentTargetIndex;
            long indexDifference = Math.Abs(currentBaseIndex - currentBlockIndex);
            int limit =
                collectionName == CollectionNames.GetCollectionName<InventoryDocument>()
                || collectionName == CollectionNames.GetCollectionName<AvatarDocument>()
                    ? 1
                    : 15;
            currentTargetIndex =
                currentTargetIndex + (indexDifference > limit ? limit : indexDifference);

            if (currentBaseIndex >= currentBlockIndex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                continue;
            }

            _logger.Information(
                "{CollectionName} Request diff data, current: {CurrentBlockIndex}, gap: {IndexDiff}, base: {CurrentBaseIndex} target: {CurrentTargetIndex}",
                collectionName,
                currentBlockIndex,
                indexDifference,
                currentBaseIndex,
                currentTargetIndex
            );

            var result = await _headlessGqlClient.GetAccountDiffsAsync(
                currentBaseIndex,
                currentTargetIndex,
                accountAddress.ToString(),
                stoppingToken
            );

            await writer.WriteAsync(
                new DiffContext()
                {
                    DiffResponse = result,
                    AccountAddress = accountAddress,
                    TargetBlockIndex = currentTargetIndex,
                    CollectionName = collectionName
                },
                stoppingToken
            );
        }
    }

    public async Task<long> GetSyncedBlockIndex(
        string collectionName,
        CancellationToken stoppingToken
    )
    {
        try
        {
            var syncedBlockIndex = await _dbService.GetLatestBlockIndex(
                nameof(DiffPoller),
                collectionName,
                stoppingToken
            );
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex(stoppingToken);
            _logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex
            );
            await _dbService.UpdateLatestBlockIndex(
                new MetadataDocument
                {
                    PollerType = nameof(DiffPoller),
                    CollectionName = collectionName,
                    LatestBlockIndex = currentBlockIndex - 1
                }
            );
            return currentBlockIndex - 1;
        }
    }
}
