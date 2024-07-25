using System.Runtime.Serialization;
using Bencodex.Types;
using Nekoyume.Model.Item;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Fixtures.Types.Items;

public class VanillaItemUsable : ItemUsable
{
    public VanillaItemUsable(ItemSheet.Row data, Guid id, long requiredBlockIndex) : base(data, id, requiredBlockIndex)
    {
    }

    public VanillaItemUsable(Dictionary serialized) : base(serialized)
    {
    }

    public VanillaItemUsable(SerializationInfo info, StreamingContext _) : base(info, _)
    {
    }
}
