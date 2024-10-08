using Bencodex.Types;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.ItemEnhancement13.ResultModel"/>
/// </summary>
[BsonIgnoreExtraElements]
public record ItemEnhancement13Result : ItemEnhancement12Result
{
    public ItemEnhancement13Result()
    {
    }

    public ItemEnhancement13Result(IValue bencoded) : base(bencoded)
    {
    }
}
