using System.Collections.Concurrent;
using System.Threading.Channels;
using Bencodex;
using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Constants;
using Mimir.Worker.Handler;
using Mimir.Worker.Services;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Poller;

public class DiffConsumer
{
    private readonly Channel<DiffContext> _channel;
    protected readonly MongoDbService _dbService;
    protected readonly ILogger _logger;
    private readonly Codec Codec = new();

    public DiffConsumer(Channel<DiffContext> channel, MongoDbService dbService)
    {
        _dbService = dbService;
        _logger = Log.ForContext<DiffBlockPoller>();
        _channel = channel;
    }

    public async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        await foreach (var diffContext in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            if (diffContext.Diffs.Count() == 0)
            {
                _logger.Information("{CollectionName}: No diffs", diffContext.CollectionName);
                await _dbService.UpdateLatestBlockIndex(
                    new MetadataDocument
                    {
                        PollerType = "DiffBlockPoller",
                        CollectionName = diffContext.CollectionName,
                        LatestBlockIndex = diffContext.TargetBlockIndex
                    }
                );
                continue;
            }

            if (
                AddressHandlerMappings.HandlerMappings.TryGetValue(
                    diffContext.AccountAddress,
                    out var handler
                )
            )
            {
                await ProcessStateDiff(handler, diffContext.AccountAddress, diffContext.Diffs);

                await _dbService.UpdateLatestBlockIndex(
                    new MetadataDocument
                    {
                        PollerType = "DiffBlockPoller",
                        CollectionName = diffContext.CollectionName,
                        LatestBlockIndex = diffContext.TargetBlockIndex
                    }
                );
            }
            else
            {
                _logger.Error(
                    "No handler for {AccountAddress} address",
                    diffContext.AccountAddress
                );
            }
        }
    }

    private async Task ProcessStateDiff(
        IStateHandler handler,
        Address accountAddress,
        IEnumerable<IGetAccountDiffs_AccountDiffs> diffs
    )
    {
        List<MimirBsonDocument> documents = new List<MimirBsonDocument>();
        foreach (var diff in diffs)
        {
            if (diff.ChangedState is not null)
            {
                var address = new Address(diff.Path);

                var document = handler.ConvertToState(
                    new()
                    {
                        Address = address,
                        RawState = Codec.Decode(Convert.FromHexString(diff.ChangedState))
                    }
                );

                documents.Add(document);
            }
        }

        _logger.Information(
            "{DiffCOunt} Handle in {Handler} Converted {Count} States",
            diffs.Count(),
            handler.GetType().Name,
            documents.Count
        );

        if (documents.Count > 0)
        {
            await _dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName(accountAddress),
                documents
            );
        }
    }
}
