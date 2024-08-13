using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.MongoDB.Bson;
using Mimir.Repositories;

namespace Mimir.GraphQL.Queries;

public class Query
{
    public async Task<AgentState> GetAgentAsync(Address address, [Service] AgentRepository repo)
    {
        var doc = await repo.GetByAddressAsync(address);
        return doc.Object;
    }

    public async Task<AvatarState> GetAvatarAsync(Address address, [Service] AvatarRepository repo)
    {
        var doc = await repo.GetByAddressAsync(address);
        return doc.Object;
    }

    public async Task<int> GetActionPointAsync(
        Address address,
        [Service] ActionPointRepository repo
    ) => await repo.GetByAddressAsync(address);

    public async Task<long> GetDailyRewardAsync(
        Address address,
        [Service] DailyRewardRepository repo
    ) => await repo.GetByAddressAsync(address);

    public ArenaObject GetArena() => new();

    public async Task<MetadataDocument> GetMetadataAsync(
        string collectionName,
        [Service] MetadataRepository repo
    ) => await repo.GetByCollectionAsync(collectionName);
}
