using System.Security.Cryptography;
using Bencodex;
using Bencodex.Types;
using Libplanet.Common;
using Nekoyume.Model.State;

namespace Lib9c.Models.Item;

public class Material : ItemBase, IBencodable
{
    public HashDigest<SHA256> ItemId { get; private set; }

    public Material(Dictionary bencoded)
        : base(bencoded)
    {
        ItemId = bencoded["item_id"].ToItemId();
    }

    public new IValue Bencoded => ((Dictionary)base.Bencoded).Add("item_id", ItemId.Serialize());
}
