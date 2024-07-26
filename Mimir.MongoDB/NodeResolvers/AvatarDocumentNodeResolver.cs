using HotChocolate;
using HotChocolate.Types.Relay;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.MongoDB.NodeResolvers;

public class AvatarDocumentNodeResolver
{
    public Task<AvatarDocument> ResolveAsync(
        [Service] IMongoCollection<AvatarDocument> collection,
        [ID] Address address)
    {
        return collection.Find(x => x.Address == address).FirstOrDefaultAsync();
    }   
}
