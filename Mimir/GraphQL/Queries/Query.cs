using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using MongoDB.Driver;

namespace Mimir.GraphQL.Queries;

[QueryType]
public class Query
{
    [UsePaging]
    public IExecutable<AvatarDocument> GetAvatars(
        [Service] IMongoCollection<AvatarDocument> collection) =>
        collection.AsExecutable();

    // [UseFirstOrDefault]
    // public IExecutable<AvatarDocument> GetAvatarByAddress(
    //     [Service] IMongoCollection<AvatarDocument> collection,
    //     [ID("LibplanetAddress")] Address address)
    // {
    //     return collection.Find(x => x.Object.Address.Equals(address)).AsExecutable();
    // } // Unable to infer or resolve a schema type from the type reference `IValue (Input)`.
}
