using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Nekoyume.Model.State;

namespace Lib9c.Models.Market;

public abstract class Product : IBencodable
{
    public Guid ProductId { get; }
    public Nekoyume.Model.Market.ProductType ProductType { get; }
    public FungibleAssetValue Price { get; }
    public long RegisteredBlockIndex { get; }
    public Address SellerAvatarAddress { get; }
    public Address SellerAgentAddress { get; }

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
