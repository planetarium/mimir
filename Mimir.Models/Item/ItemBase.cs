using Bencodex;
using Bencodex.Types;
using Mimir.Models.Abstractions;
using Nekoyume.Model.State;

namespace Mimir.Models.Item;

public abstract class ItemBase : IItem, IBencodable
{
    public int Id { get; private set; }
    public int Grade { get; private set; }
    public Nekoyume.Model.Item.ItemType ItemType { get; private set; }
    public Nekoyume.Model.Item.ItemSubType ItemSubType { get; private set; }
    public Nekoyume.Model.Elemental.ElementalType ElementalType { get; private set; }

    public ItemBase(Dictionary bencoded)
    {
        Id = bencoded["id"].ToInteger();
        Grade = bencoded["grade"].ToInteger();
        ItemType = bencoded["item_type"].ToEnum<Nekoyume.Model.Item.ItemType>();
        ItemSubType = bencoded["item_sub_type"].ToEnum<Nekoyume.Model.Item.ItemSubType>();
        ElementalType = bencoded["elemental_type"].ToEnum<Nekoyume.Model.Elemental.ElementalType>();
    }

    public IValue Bencoded =>
        Dictionary
            .Empty.Add("id", Id.Serialize())
            .Add("item_type", ItemType.Serialize())
            .Add("item_sub_type", ItemSubType.Serialize())
            .Add("grade", Grade.Serialize())
            .Add("elemental_type", ElementalType.Serialize());
}
