using Bencodex.Types;
using Nekoyume.Model.Quest;

namespace Mimir.MongoDB.Bson;

public record QuestListDocument(QuestList Object) : IMimirBsonDocument
{
    public IValue Bencoded => Object.Serialize();
}
