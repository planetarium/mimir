using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.MongoDB.Bson;

/// <summary>
/// Not MongoDB collection, it is a NineChronicles' collection state.
/// </summary>
/// <param name="Object"></param>
public record CollectionDocument(
    Address Address,
    Nekoyume.Model.State.CollectionState Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
