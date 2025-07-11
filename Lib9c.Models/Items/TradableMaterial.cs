using System.Text.Json.Serialization;
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

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add(RequiredBlockIndexKey, RequiredBlockIndex.Serialize());

    public TradableMaterial()
    {
    }

    public TradableMaterial(IValue bencoded) : base(bencoded)
    {
        try
        {
            var tradableMaterial = (Nekoyume.Model.Item.TradableMaterial)Nekoyume.Model.Item.ItemFactory.Deserialize(bencoded);
            RequiredBlockIndex = tradableMaterial.RequiredBlockIndex;
        }
        catch (ArgumentException)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary, ValueKind.List },
                bencoded.Kind);
        }
    }
}
