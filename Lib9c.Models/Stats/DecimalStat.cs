using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.Stat;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Stats;

/// <summary>
/// <see cref="Nekoyume.Model.Stat.DecimalStat"/>.
/// </summary>
public record DecimalStat : IBencodable
{
    public decimal BaseValue { get; init; }

    public decimal AdditionalValue { get; init; }

    public StatType StatType { get; }

    public IValue Bencoded => Dictionary.Empty
        .Add("statType", StatType.Serialize())
        .Add("value", BaseValue.Serialize())
        .Add("additionalValue", AdditionalValue.Serialize());

    public IValue BencodedWithoutAdditionalValue => Dictionary.Empty
        .Add("statType", StatType.Serialize())
        .Add("value", BaseValue.Serialize());

    public IValue BencodedAsLegacy => Dictionary.Empty
        .Add("type", StatType.Serialize())
        .Add("value", BaseValue.Serialize());

    public DecimalStat(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
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
