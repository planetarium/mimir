using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume;
using Nekoyume.Extensions;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class WorldBossStateHandler(IStateService stateService, MongoDbService store)
    : BaseActionHandler<WorldBossStateDocument>(stateService, store, "^raid[0-9]*$", Log.ForContext<WorldBossStateHandler>())
{
    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
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

        var worldBossListSheet = await Store.GetSheetAsync<WorldBossListSheet>(stoppingToken);
        if (worldBossListSheet is null)
        {
            throw new InvalidOperationException($"{nameof(WorldBossKillRewardRecordStateHandler)} requires ${nameof(WorldBossListSheet)}");
        }

        var row = worldBossListSheet.FindRowByBlockIndex(blockIndex);
        var raidId = row.Id;
        var worldBossAddress = Addresses.GetWorldBossAddress(raidId);
        var worldBossState = await StateGetter.GetWorldBossStateAsync(worldBossAddress, stoppingToken);
        return
        [
            new WorldBossStateDocument(worldBossAddress, raidId, worldBossState).ToUpdateOneModel(),
        ];
    }
}
