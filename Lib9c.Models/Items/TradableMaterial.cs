using Bencodex.Types;
using Lib9c.Models.Exceptions;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.TradableMaterial"/>
/// </summary>
[BsonIgnoreExtraElements]
public record TradableMaterial : Material
{
    public long RequiredBlockIndex { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add(RequiredBlockIndexKey, RequiredBlockIndex.Serialize());

    public TradableMaterial()
    {
    }

    public TradableMaterial(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        RequiredBlockIndex = d.ContainsKey(RequiredBlockIndexKey)
            ? d[RequiredBlockIndexKey].ToLong()
            : default;
    }
}
