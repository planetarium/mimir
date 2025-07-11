using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Stat;
using ValueKind = Bencodex.Types.ValueKind;
using KeyNotFoundException = System.Collections.Generic.KeyNotFoundException;

namespace Lib9c.Models.Stats;

/// <summary>
/// <see cref="Nekoyume.Model.Stat.DecimalStat"/>.
/// </summary>
[BsonIgnoreExtraElements]
public record DecimalStat : IBencodable
{
    public StatType StatType { get; init; }
    public decimal BaseValue { get; init; }
    public decimal AdditionalValue { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => Dictionary.Empty
        .Add("statType", StatType.Serialize())
        .Add("value", BaseValue.Serialize())
        .Add("additionalValue", AdditionalValue.Serialize());

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue BencodedWithoutAdditionalValue => Dictionary.Empty
        .Add("statType", StatType.Serialize())
        .Add("value", BaseValue.Serialize());

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue BencodedAsLegacy => Dictionary.Empty
        .Add("type", StatType.Serialize())
        .Add("value", BaseValue.Serialize());

    public DecimalStat()
    {
    }

    public DecimalStat(StatType statType, decimal baseValue, decimal additionalValue)
    {
        StatType = statType;
        BaseValue = baseValue;
        AdditionalValue = additionalValue;
    }

    public DecimalStat(IValue bencoded)
    {
        try
        {
            Nekoyume.Model.Stat.DecimalStat stat = bencoded switch
            {
                Dictionary => bencoded.ToDecimalStat(),
                List => new Nekoyume.Model.Stat.DecimalStat(bencoded),
                _ => throw new ArgumentException(),
            };

            StatType = stat.StatType;
            BaseValue = stat.BaseValue;
            AdditionalValue = stat.AdditionalValue;
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
