using System.Runtime.Serialization;
using Bencodex.Types;
using Nekoyume.Model.Item;
using Nekoyume.TableData;

namespace Lib9c.Models.Tests.Fixtures.Types.Items;

public class VanillaItemBase : ItemBase
{
    public VanillaItemBase(ItemSheet.Row data) : base(data)
    {
    }

    public VanillaItemBase(ItemBase other) : base(other)
    {
    }

    public VanillaItemBase(Dictionary serialized) : base(serialized)
    {
    }

    public VanillaItemBase(SerializationInfo info, StreamingContext _) : base(info, _)
    {
    }
}
