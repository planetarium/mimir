using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.TradableMaterial"/>
/// </summary>
public record TradableMaterial : Material, IBencodable
{
    public long RequiredBlockIndex { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add(RequiredBlockIndexKey, RequiredBlockIndex.Serialize());

    public TradableMaterial(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        RequiredBlockIndex = d.ContainsKey(RequiredBlockIndexKey)
            ? d[RequiredBlockIndexKey].ToLong()
            : default;
    }
}
