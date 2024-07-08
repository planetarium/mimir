using System.Security.Cryptography;
using Bencodex;
using Bencodex.Types;
using Libplanet.Common;
using Mimir.Models.Stat;
using Nekoyume.Model.State;

namespace Mimir.Models.Item;

public class TradableMaterial : Material, IBencodable
{
    public Guid TradableId { get; private set; }

    public long RequiredBlockIndex { get; private set; }

    public TradableMaterial(Dictionary bencoded)
        : base(bencoded)
    {
        RequiredBlockIndex = bencoded.ContainsKey("rbi") ? bencoded["rbi"].ToLong() : default;
        TradableId = DeriveTradableId(ItemId);
    }

    new public IValue Bencoded => ((Dictionary)base.Bencoded).Add("item_id", ItemId.Serialize());

    public static Guid DeriveTradableId(HashDigest<SHA256> fungibleId) =>
        new Guid(HashDigest<MD5>.DeriveFrom(fungibleId.ToByteArray()).ToByteArray());
}
