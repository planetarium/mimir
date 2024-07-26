using Bencodex.Types;
using HotChocolate.Types.Relay;
using Libplanet.Crypto;
using Mimir.MongoDB.NodeResolvers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.MongoDB.Bson;

[Node(
    IdField = nameof(Address),
    NodeResolverType = typeof(AvatarDocumentNodeResolver),
    NodeResolver = nameof(AvatarDocumentNodeResolver.ResolveAsync))]
public record AvatarDocument(Lib9c.Models.States.AvatarState Object) : IMimirBsonDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public Address Address => Object.Address;
    public IValue Bencoded => Object.Bencoded;
}
