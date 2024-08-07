using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.Repositories;

namespace Mimir.GraphQL.Queries;

public class Query
{
    public async Task<AgentState> GetAgentAsync(
        Address address,
        [Service] AgentRepository repo)
    {
        var doc = await repo.GetAgentAsync(address);
        return doc.Object;
    }
}
