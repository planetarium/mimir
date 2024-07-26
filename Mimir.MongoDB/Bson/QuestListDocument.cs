using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Quest;

namespace Mimir.MongoDB.Bson;

public record QuestListDocument(
    Address Address,
    QuestList Object)
    : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
