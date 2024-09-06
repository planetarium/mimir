using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Market;

public abstract class Product : IBencodable
{
    public Guid ProductId { get; }
    public Nekoyume.Model.Market.ProductType ProductType { get; }
    public FungibleAssetValue Price { get; }
    public long RegisteredBlockIndex { get; }
    public Address SellerAvatarAddress { get; }
    public Address SellerAgentAddress { get; }

    public Product(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        ProductId = l[0].ToGuid();
        ProductType = l[1].ToEnum<Nekoyume.Model.Market.ProductType>();
        Price = l[2].ToFungibleAssetValue();
        RegisteredBlockIndex = l[3].ToLong();
        SellerAgentAddress = l[4].ToAddress();
        SellerAvatarAddress = l[5].ToAddress();
    }

    public Product(List bencoded)
    {
        ProductId = bencoded[0].ToGuid();
        ProductType = bencoded[1].ToEnum<Nekoyume.Model.Market.ProductType>();
        Price = bencoded[2].ToFungibleAssetValue();
        RegisteredBlockIndex = bencoded[3].ToLong();
        SellerAgentAddress = bencoded[4].ToAddress();
        SellerAvatarAddress = bencoded[5].ToAddress();
    }

    public IValue Bencoded =>
        List
            .Empty.Add(ProductId.Serialize())
            .Add(ProductType.Serialize())
            .Add(Price.Serialize())
            .Add(RegisteredBlockIndex.Serialize())
            .Add(SellerAgentAddress.Serialize())
            .Add(SellerAvatarAddress.Serialize());
}
