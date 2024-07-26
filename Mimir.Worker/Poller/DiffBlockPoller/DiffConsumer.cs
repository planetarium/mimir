using System.Collections.Concurrent;
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
    protected readonly MongoDbService _dbService;
    protected readonly string _pollerType;
    protected readonly ILogger _logger;
    private readonly Codec Codec = new();
    private readonly ConcurrentQueue<DiffContext> _queue;

    public DiffConsumer(ConcurrentQueue<DiffContext> queue, MongoDbService dbService)
    {
        _dbService = dbService;
        _pollerType = "DiffBlockPoller";
        _logger = Log.ForContext<DiffBlockPoller>();
        _queue = queue;
    }

    public async Task ConsumeAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_queue.TryDequeue(out var diffContext))
            {
                if (diffContext.Diffs.Count() == 0)
                {
                    _logger.Information(
                        "{CollectionName}: No diffs",
                        diffContext.CollectionName
                    );
                    await _dbService.UpdateLatestBlockIndex(
                        new MetadataDocument
                        {
                            PollerType = _pollerType,
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
                            PollerType = _pollerType,
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
            else
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);
            }
        }
    }

    private async Task ProcessStateDiff(
        IStateHandler handler,
        Address accountAddress,
        IEnumerable<IGetAccountDiffs_AccountDiffs> diffs
    )
    {
        List<MongoDbCollectionDocument> documents = new List<MongoDbCollectionDocument>();
        foreach (var diff in diffs)
        {
            if (diff.ChangedState is not null)
            {
                var address = new Address(diff.Path);

                var state = handler.ConvertToState(
                    new()
                    {
                        Address = address,
                        RawState = Codec.Decode(Convert.FromHexString(diff.ChangedState))
                    }
                );

                documents.Add(new MongoDbCollectionDocument(address, state));
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
