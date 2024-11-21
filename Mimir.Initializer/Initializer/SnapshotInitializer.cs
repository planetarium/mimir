using Bencodex.Types;
using Libplanet.Action.State;
using Libplanet.Blockchain;
using Libplanet.Common;
using Libplanet.Crypto;
using Libplanet.Store;
using Libplanet.Store.Trie;
using Mimir.Initializer.Util;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Initializer.Initializer;

public class SnapshotInitializer
{
    private readonly MongoDbService _dbService;
    private readonly ILogger _logger;
    private readonly string _chainStorePath;
    private Address[] _targetAccounts;

    public SnapshotInitializer(
        MongoDbService dbService,
        string chainStorePath,
        Address[] targetAccounts
    )
    {
        _dbService = dbService;
        _chainStorePath = chainStorePath;
        _targetAccounts = targetAccounts;
        _logger = Log.ForContext<SnapshotInitializer>();
    }

    public async Task RunAsync(CancellationToken stoppingToken)
    {
        var started = DateTime.UtcNow;

        (BlockChain blockChain, IStore store, IStateStore stateStore) = ChainUtil.LoadBlockChain(
            _chainStorePath
        );

        foreach (var address in _targetAccounts)
        {
            await ProcessByAccountAddress(blockChain, stateStore, address, stoppingToken);

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        store.Dispose();
        stateStore.Dispose();

        _logger.Information(
            "Finished SnapshotInitializer. Elapsed {TotalElapsedMinutes} minutes",
            DateTime.UtcNow.Subtract(started).Minutes
        );
    }

    private async Task ProcessByAccountAddress(
        BlockChain blockChain,
        IStateStore stateStore,
        Address accountAddress,
        CancellationToken stoppingToken
    )
    {
        int predicateLength = Address.Size * 2;

        ITrie worldTrie = ChainUtil.GetWorldTrie(blockChain);
        IWorldState world = new WorldBaseState(worldTrie, stateStore);
        IAccountState account = world.GetAccountState(accountAddress);
        ITrie accountTrie = account.Trie;
        _logger.Information(
            "Iterating over trie with state root hash {StateRootHash}",
            accountTrie.Hash
        );

        long addressCount = 0;
        string? currentAddress = null;

        foreach ((KeyBytes keyBytes, IValue value) in accountTrie.IterateValues())
        {
            if (keyBytes.Length == predicateLength)
            {
                addressCount++;
                Address address = ChainUtil.ToAddress(keyBytes);
                currentAddress = ByteUtil.Hex(address.ByteArray);
            }

            if (currentAddress is string hex)
            {
                await HandleByAccount(
                    accountAddress,
                    new Address(currentAddress),
                    value,
                    blockChain.Tip.Index
                );
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.Information("Total address count: {AddressCount}", addressCount);
    }

    private async Task HandleByAccount(
        Address accountAddress,
        Address address,
        IValue state,
        long blockIndex
    )
    {
        var collectionName = CollectionNames.GetCollectionName(accountAddress);
        var documents = new List<MimirBsonDocument>();
        var document = ConverterMappings
            .GetConverter(accountAddress)
            .ConvertToDocument(
                new AddressStatePair
                {
                    BlockIndex = blockIndex,
                    Address = address,
                    RawState = state
                }
            );

        documents.Add(document);

        if (documents.Count > 0)
            await _dbService.UpsertStateDataManyAsync(collectionName, documents, false, null, default);

        _logger.Information(
            "{DocumentCount} Handled, {CollectionName} - {Address}",
            documents.Count,
            collectionName,
            address
        );
    }
}
