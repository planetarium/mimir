using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Initializer.Manager;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ItemSlotStateHandler(
    IStateService stateService,
    IMongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetterService
)
    : BaseActionHandler<ItemSlotDocument>(
        stateService,
        store,
        headlessGqlClient,
        initializerManager,
        "^hack_and_slash[0-9]*$|^hack_and_slash_sweep[0-9]*$|^battle_arena[0-9]*$|^event_dungeon_battle[0-9]*$|^join_arena[0-9]*$|^raid[0-9]*$",
        Log.ForContext<ItemSlotStateHandler>(),
        stateGetterService
    )
{
    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (Regex.IsMatch(actionType, "^hack_and_slash[0-9]*$"))
        {
            var action = new HackAndSlash();
            action.LoadPlainValue(actionPlainValue);
            return await ItemSlotCollectionUpdater.UpdateAsync(
                StateService,
                blockIndex,
                BattleType.Adventure,
                action.AvatarAddress,
                stoppingToken
            );
        }

        if (Regex.IsMatch(actionType, "^hack_and_slash_sweep[0-9]*$"))
        {
            var action = new HackAndSlashSweep();
            action.LoadPlainValue(actionPlainValue);
            return await ItemSlotCollectionUpdater.UpdateAsync(
                StateService,
                blockIndex,
                BattleType.Adventure,
                action.avatarAddress,
                stoppingToken
            );
        }

        if (Regex.IsMatch(actionType, "^battle_arena[0-9]*$"))
        {
            var action = new BattleArena();
            action.LoadPlainValue(actionPlainValue);
            return await ItemSlotCollectionUpdater.UpdateAsync(
                StateService,
                blockIndex,
                BattleType.Arena,
                action.myAvatarAddress,
                stoppingToken
            );
        }

        if (Regex.IsMatch(actionType, "^event_dungeon_battle[0-9]*$"))
        {
            var action = new EventDungeonBattle();
            action.LoadPlainValue(actionPlainValue);
            return await ItemSlotCollectionUpdater.UpdateAsync(
                StateService,
                blockIndex,
                BattleType.Adventure,
                action.AvatarAddress,
                stoppingToken
            );
        }

        if (Regex.IsMatch(actionType, "^join_arena[0-9]*$"))
        {
            var action = new JoinArena();
            action.LoadPlainValue(actionPlainValue);
            return await ItemSlotCollectionUpdater.UpdateAsync(
                StateService,
                blockIndex,
                BattleType.Arena,
                action.avatarAddress,
                stoppingToken
            );
        }

        if (Regex.IsMatch(actionType, "^raid[0-9]*$"))
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
                    actionPlainValueInternal.Kind
                );
            }

            if (d["e"] is not List equipmentIdsList)
            {
                throw new UnsupportedArgumentTypeException<ValueKind>(
                    $"{nameof(actionPlainValueInternal)}[\"e\"]",
                    [ValueKind.List],
                    d["e"].Kind
                );
            }

            if (d["c"] is not List costumeIdsList)
            {
                throw new UnsupportedArgumentTypeException<ValueKind>(
                    $"{nameof(actionPlainValueInternal)}[\"c\"]",
                    [ValueKind.List],
                    d["c"].Kind
                );
            }

            var avatarAddress = new Address(d["a"]);
            var equipmentIds = equipmentIdsList
                .Cast<Binary>()
                .Select(x => new Guid(x.ToByteArray()));
            var costumeIds = costumeIdsList.Cast<Binary>().Select(x => new Guid(x.ToByteArray()));
            return await ItemSlotCollectionUpdater.UpdateAsync(
                StateService,
                blockIndex,
                BattleType.Raid,
                avatarAddress,
                stoppingToken
            );
        }

        return [];
    }
}
