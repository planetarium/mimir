using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.Repositories;

namespace Mimir.GraphQL.Queries;

public class Query
{
    public async Task<AgentState> GetAgentAsync(
        Address address,
        [Service] AgentRepository repo)
    {
        var doc = await repo.GetByAddressAsync(address);
        return doc.Object;
    }

    public ArenaObject GetArena() => new();
}
