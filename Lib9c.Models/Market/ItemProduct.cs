using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Factories;
using Lib9c.Models.Items;
using Nekoyume.Model.State;

namespace Lib9c.Models.Market;

public class ItemProduct : Product, IBencodable
{
    public ItemUsable TradableItem { get; }
    public int ItemCount { get; }

    public ItemProduct(List bencoded)
        : base(bencoded)
    {
        TradableItem = (ItemUsable)ItemFactory.Deserialize((Dictionary)bencoded[6]);
        ItemCount = bencoded[7].ToInteger();
    }

    [GraphQLIgnore]
    public new IValue Bencoded => Serialize();

    public IValue Serialize()
    {
        List serialized = (List)base.Bencoded;
        return serialized.Add(TradableItem.Bencoded).Add(ItemCount.Serialize());
    }
}
