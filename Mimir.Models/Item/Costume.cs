using Bencodex;
using Bencodex.Types;
using Mimir.Models.Stat;
using Nekoyume.Model.State;

namespace Mimir.Models.Item;

public class Costume : ItemBase, IBencodable
{
    public bool Equipped { get; private set; }
    public string SpineResourcePath { get; private set; }
    public Guid ItemId { get; private set; }
    public long RequiredBlockIndex { get; private set; }

    public Costume(Dictionary bencoded)
        : base(bencoded)
    {
        ItemId = bencoded["item_id"].ToGuid();
        Equipped = bencoded["equipped"].ToBoolean();
        SpineResourcePath = bencoded["spine_resource_path"].ToDotnetString();
        RequiredBlockIndex = bencoded["rbi"].ToLong();
    }

    public IValue Bencoded => Serialize();

    public IValue Serialize()
    {
        var dict = ((Dictionary)base.Bencoded)
            .Add("equipped", Equipped.Serialize())
            .Add("item_id", ItemId.Serialize())
            .Add("rbi", RequiredBlockIndex.Serialize())
            .Add("spine_resource_path", SpineResourcePath.Serialize());

        return dict;
    }
}
