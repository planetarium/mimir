using Bencodex.Types;
using Lib9c.Models.Market;
using Nekoyume.Model.State;

namespace Lib9c.Models.Factories;

public static class ProductFactory
{
    public static Product DeserializeProduct(List serialized)
    {
        if (
            serialized[1].ToEnum<Nekoyume.Model.Market.ProductType>()
            == Nekoyume.Model.Market.ProductType.FungibleAssetValue
        )
        {
            return new FavProduct(serialized);
        }

        return new ItemProduct(serialized);
    }
}
