using System.Security.Cryptography;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Common;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Material"/>
/// </summary>
public record Material : ItemBase, IBencodable
{
    public HashDigest<SHA256> ItemId { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("item_id", ItemId.Serialize());

    public Material(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        if (d.TryGetValue((Text) "item_id", out var itemId))
        {
            ItemId = itemId.ToItemId();
        }
    }
}
