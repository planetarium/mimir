using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Stat;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Stats;

/// <summary>
/// <see cref="Nekoyume.Model.Stat.DecimalStat"/>.
/// </summary>
public record DecimalStat : IBencodable
{
    public StatType StatType { get; init; }
    public decimal BaseValue { get; init; }
    public decimal AdditionalValue { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public IValue Bencoded => Dictionary.Empty
        .Add("statType", StatType.Serialize())
        .Add("value", BaseValue.Serialize())
        .Add("additionalValue", AdditionalValue.Serialize());

    [BsonIgnore, GraphQLIgnore]
    public IValue BencodedWithoutAdditionalValue => Dictionary.Empty
        .Add("statType", StatType.Serialize())
        .Add("value", BaseValue.Serialize());

    [BsonIgnore, GraphQLIgnore]
    public IValue BencodedAsLegacy => Dictionary.Empty
        .Add("type", StatType.Serialize())
        .Add("value", BaseValue.Serialize());

    public DecimalStat()
    {
    }

    public DecimalStat(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        if (d.TryGetValue((Text)"type", out var type))
        {
            StatType = StatTypeExtension.Deserialize((Binary)type);
        }
        else if (d.TryGetValue((Text)"statType", out var statType))
        {
            StatType = StatTypeExtension.Deserialize((Binary)statType);
        }
        else
        {
            throw new KeyNotFoundException(
                "The required key 'type' or 'statType' was not found.");
        }

        BaseValue = d["value"].ToDecimal();

        if (d.TryGetValue((Text)"additionalValue", out var additionalValue))
        {
            AdditionalValue = additionalValue.ToDecimal();
        }
    }
}
