using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Stat;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Stats;

/// <summary>
/// <see cref="Nekoyume.Model.Stat.StatMap"/>.
/// <see cref="Nekoyume.Model.Stat.StatsMap"/>'s serialize and deserialize logic is same with <see cref="Nekoyume.Model.Stat.StatMap"/>.
/// </summary>
[BsonIgnoreExtraElements]
public record StatMap : IBencodable
{
    public Dictionary<StatType, DecimalStat> Value { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => new Dictionary(Value
        .Where(x => x.Value.BaseValue > 0 || x.Value.AdditionalValue > 0)
        .Select(kv => new KeyValuePair<IKey, IValue>(
            kv.Key.Serialize(),
            kv.Value.Bencoded)));

    public StatMap()
    {
    }

    public StatMap(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Value = new Dictionary<StatType, DecimalStat>();
        foreach (var kv in d)
        {
            var statType = StatTypeExtension.Deserialize((Binary)kv.Key);
            Value[statType] = new DecimalStat(kv.Value);
        }
    }
}
