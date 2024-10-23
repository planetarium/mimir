using Bencodex.Types;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.Model.EnumType;

namespace Mimir.Worker.CollectionUpdaters;

public static class ItemSlotCollectionUpdater
{
    public static async Task<IEnumerable<WriteModel<BsonDocument>>> UpdateAsync(
        IStateService stateService,
        BattleType battleType,
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var itemSlotAddress = Nekoyume.Model.State.ItemSlotState.DeriveAddress(
            avatarAddress,
            battleType
        );
        if (await stateService.GetState(itemSlotAddress, stoppingToken) is not List serialized)
        {
            return [];
        }

        var itemSlotState = new ItemSlotState(serialized);
        var itemSlotDocument = new ItemSlotDocument(itemSlotAddress, itemSlotState);
        return [itemSlotDocument.ToUpdateOneModel()];
    }
}
