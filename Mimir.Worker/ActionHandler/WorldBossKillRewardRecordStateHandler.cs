using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Nekoyume;
using Nekoyume.Extensions;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class WorldBossKillRewardRecordStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler(stateService, store, "^raid[0-9]*$", Log.ForContext<WorldBossKillRewardRecordStateHandler>())
{
    protected override async Task HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (actionPlainValueInternal is null)
        {
            throw new ArgumentNullException(nameof(actionPlainValueInternal));
        }

        if (actionPlainValueInternal is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(actionPlainValueInternal),
                [ValueKind.Dictionary],
                actionPlainValueInternal.Kind);
        }

        var avatarAddress = d["a"].ToAddress();
        var worldBossListSheet = await Store.GetSheetAsync<WorldBossListSheet>(stoppingToken);
        if (worldBossListSheet is null)
        {
            throw new InvalidOperationException($"{nameof(WorldBossKillRewardRecordStateHandler)} requires ${nameof(WorldBossListSheet)}");
        }

        var row = worldBossListSheet.FindRowByBlockIndex(blockIndex);
        var raidId = row.Id;

        var worldBossKillRewardRecordAddress = Addresses.GetWorldBossKillRewardRecordAddress(
            avatarAddress,
            raidId);
        var worldBossKillRewardRecordState = await StateGetter.GetWorldBossKillRewardRecordStateAsync(
            worldBossKillRewardRecordAddress,
            stoppingToken);
        await Store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<WorldBossKillRewardRecordDocument>(),
            [
                new WorldBossKillRewardRecordDocument(
                    worldBossKillRewardRecordAddress,
                    avatarAddress,
                    worldBossKillRewardRecordState)
            ],
            session,
            stoppingToken);
    }
}
