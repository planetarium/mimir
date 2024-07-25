using System.Security.Cryptography;
using Bencodex;
using Bencodex.Types;
using Libplanet.Common;
using Nekoyume.Model.State;

namespace Lib9c.Models.Item;

public class TradableMaterial : Material, IBencodable
{
    public Guid TradableId { get; }

    public long RequiredBlockIndex { get; }

    public TradableMaterial(Dictionary bencoded)
        : base(bencoded)
    {
        RequiredBlockIndex = bencoded.ContainsKey("rbi") ? bencoded["rbi"].ToLong() : default;
        TradableId = DeriveTradableId(ItemId);
    }

    public new IValue Bencoded => ((Dictionary)base.Bencoded).Add("item_id", ItemId.Serialize());

    public static Guid DeriveTradableId(HashDigest<SHA256> fungibleId) =>
        new Guid(HashDigest<MD5>.DeriveFrom(fungibleId.ToByteArray()).ToByteArray());
}
