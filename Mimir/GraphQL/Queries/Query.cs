using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.MongoDB.Bson;
using Mimir.Repositories;

namespace Mimir.GraphQL.Queries;

public class Query
{
    /// <summary>
    /// Get a agent state by address.
    /// </summary>
    /// <param name="address">The address of the agent.</param>
    /// <param name="repo"></param>
    /// <returns>The agent state</returns>
    public async Task<AgentState> GetAgentAsync(Address address, [Service] AgentRepository repo)
    {
        var doc = await repo.GetByAddressAsync(address);
        return doc.Object;
    }

    /// <summary>
    /// Get a avatar state by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <param name="repo"></param>
    /// <returns>The avatar state</returns>
    public async Task<AvatarState> GetAvatarAsync(Address address, [Service] AvatarRepository repo)
    {
        var doc = await repo.GetByAddressAsync(address);
        return doc.Object;
    }

    /// <summary>
    /// Get an action point by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <param name="repo"></param>
    /// <returns>The action point.</returns>
    public async Task<int> GetActionPointAsync(
        Address address,
        [Service] ActionPointRepository repo
    ) => await repo.GetByAddressAsync(address);

    /// <summary>
    /// Get the daily reward received block index by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <param name="repo"></param>
    /// <returns>The daily reward received block index.</returns>
    public async Task<long> GetDailyRewardReceivedBlockIndexAsync(
        Address address,
        [Service] DailyRewardRepository repo
    ) => await repo.GetByAddressAsync(address);

    /// <summary>
    /// Get arena sub-fields.
    /// </summary>
    public ArenaObject GetArena() => new();

    /// <summary>
    /// Get metadata by collection name.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="repo"></param>
    /// <returns>The metadata</returns>
    public async Task<MetadataDocument> GetMetadataAsync(
        string collectionName,
        [Service] MetadataRepository repo
    ) => await repo.GetByCollectionAsync(collectionName);
}
