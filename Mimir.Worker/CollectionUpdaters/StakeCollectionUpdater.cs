using System.Numerics;
using Bencodex.Types;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.Worker.CollectionUpdaters;

public static class StakeCollectionUpdater
{
    public static async Task<IEnumerable<WriteModel<BsonDocument>>> UpdateAsync(
        IStateService stateService,
        long blockIndex,
        Address agentAddress,
        BigInteger amount, 
        CancellationToken stoppingToken = default
    )
    {
        var stakeAddress = Nekoyume.Model.Stake.StakeStateV2.DeriveAddress(agentAddress);
        var stakeState = await stateService.GetState(stakeAddress, stoppingToken);
        StakeDocument document;
        if (stakeState is null ||
            stakeState.Kind == ValueKind.Null)
        {
            document = new StakeDocument(blockIndex, stakeAddress, agentAddress, null, amount);
        }
        else
        {
            document = new StakeDocument(blockIndex, stakeAddress, agentAddress, new StakeState(stakeState), amount);
        }

        return [document.ToUpdateOneModel()];
    }
}
