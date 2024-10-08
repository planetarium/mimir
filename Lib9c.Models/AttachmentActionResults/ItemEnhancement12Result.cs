using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.ItemEnhancement12.ResultModel"/>
/// </summary>
[BsonIgnoreExtraElements]
public record ItemEnhancement12Result : ItemEnhancement11Result
{
    public ItemEnhancement12Result()
    {
    }

    public ItemEnhancement12Result(IValue bencoded) : base(bencoded)
    {
    }
}
