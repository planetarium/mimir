using Bencodex.Types;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Action.ItemEnhancement13.ResultModel"/>
/// </summary>
public record ItemEnhancement13Result : ItemEnhancement12Result
{
    public ItemEnhancement13Result()
    {
    }

    public ItemEnhancement13Result(IValue bencoded) : base(bencoded)
    {
    }
}
