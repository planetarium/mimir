using Bencodex.Types;

namespace Mimir.MongoDB.Bson;

/// <summary>
/// Not MongoDB collection, it is a NineChronicles' collection state.
/// </summary>
/// <param name="Object"></param>
public record CollectionDocument(Nekoyume.Model.State.CollectionState Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Bencoded;
}
