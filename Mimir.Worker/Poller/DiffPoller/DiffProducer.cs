using System.Threading.Channels;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Client;
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
    private Address _accountAddress;

    public DiffProducer(
        IStateService stateService,
        IHeadlessGQLClient headlessGqlClient,
        MongoDbService dbService,
        Address accountAddress
    )
    {
        _headlessGqlClient = headlessGqlClient;
        _stateService = stateService;
        _accountAddress = accountAddress;
        _logger = Log.ForContext<DiffProducer>()
            .ForContext("AccountAddress", _accountAddress.ToHex());
        _dbService = dbService;
    }

    public async Task ProduceByAccount(
        ChannelWriter<DiffContext> writer,
        CancellationToken stoppingToken
    )
    {
        var collectionName = CollectionNames.GetCollectionName(_accountAddress);

        var syncedIndex = await GetSyncedBlockIndex(collectionName, stoppingToken);
        var currentBaseIndex = syncedIndex;
        _logger.Information(
            "{CollectionName} Synced BlockIndex: {SyncedBlockIndex}",
            collectionName,
            syncedIndex
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            var currentIndexOnChain = await _stateService.GetLatestIndex(
                stoppingToken,
                _accountAddress
            );

            long indexDifference = currentIndexOnChain - currentBaseIndex;
            int limit =
                collectionName == CollectionNames.GetCollectionName<InventoryDocument>()
                || collectionName == CollectionNames.GetCollectionName<AvatarDocument>()
                    ? 1
                    : 15;
            var currentTargetIndex =
                currentBaseIndex + (indexDifference > limit ? limit : indexDifference);

            if (currentBaseIndex >= currentTargetIndex)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
                continue;
            }

            _logger.Information(
                "{CollectionName} Request diff data, current: {CurrentBlockIndex}, gap: {IndexDiff}, base: {CurrentBaseIndex} target: {CurrentTargetIndex}",
                collectionName,
                currentIndexOnChain,
                indexDifference,
                currentBaseIndex,
                currentTargetIndex
            );

            var result = await _headlessGqlClient.GetAccountDiffsAsync(
                currentBaseIndex,
                currentTargetIndex,
                _accountAddress,
                stoppingToken
            );

            await writer.WriteAsync(
                new DiffContext()
                {
                    DiffResponse = result,
                    AccountAddress = _accountAddress,
                    TargetBlockIndex = currentTargetIndex,
                    CollectionName = collectionName
                },
                stoppingToken
            );
            currentBaseIndex = currentTargetIndex;
        }
    }

    public async Task<long> GetSyncedBlockIndex(
        string collectionName,
        CancellationToken stoppingToken
    )
    {
        try
        {
            var syncedBlockIndex = await _dbService.GetLatestBlockIndexAsync(
                nameof(DiffPoller),
                collectionName,
                stoppingToken
            );
            return syncedBlockIndex;
        }
        catch (InvalidOperationException)
        {
            var currentBlockIndex = await _stateService.GetLatestIndex(
                stoppingToken,
                _accountAddress
            );
            _logger.Information(
                "Metadata collection is not found, set block index to {BlockIndex} - 1",
                currentBlockIndex
            );
            await _dbService.UpdateLatestBlockIndexAsync(
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
