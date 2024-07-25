using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.State;

namespace Lib9c.Models.Item;

public abstract class ItemUsable : ItemBase, IBencodable
{
    public Guid ItemId { get; private set; }

    // public StatsMap StatsMap { get; private set; }
    // public Skills Skills { get; private set; }
    // public BuffSkills BuffSkills { get; private set; }
    public long RequiredBlockIndex { get; }

    protected ItemUsable(Dictionary bencoded)
        : base(bencoded)
    {
        ItemId = bencoded["itemId"].ToGuid();
        RequiredBlockIndex = bencoded["requiredBlockIndex"].ToLong();
    }

    public new IValue Bencoded =>
        ((Dictionary)base.Bencoded)
            .Add("itemId", ItemId.Serialize())
            .Add("requiredBlockIndex", RequiredBlockIndex.Serialize());
}
