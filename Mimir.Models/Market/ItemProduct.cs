using Bencodex;
using Bencodex.Types;
using Mimir.Models.Abstractions;
using Mimir.Models.Factories;
using Mimir.Models.Item;
using Nekoyume.Model.State;

namespace Mimir.Models.Market;

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
    public IValue Bencoded => Serialize();

    public IValue Serialize()
    {
        List serialized = (List)base.Bencoded;
        return serialized.Add(TradableItem.Bencoded).Add(ItemCount.Serialize());
    }
}
