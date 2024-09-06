using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Market;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Factories;

public static class ProductFactory
{
    public static Product DeserializeProduct(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        if (
            l[1].ToEnum<Nekoyume.Model.Market.ProductType>()
            == Nekoyume.Model.Market.ProductType.FungibleAssetValue
        )
        {
            return new FavProduct(l);
        }

        return new ItemProduct(l);
    }
}
