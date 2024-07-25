using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.State;

namespace Lib9c.Models.Stat;

public class DecimalStat : IBencodable
{
    public decimal BaseValue { get; }

    public decimal AdditionalValue { get; }

    public Nekoyume.Model.Stat.StatType StatType { get; }

    public DecimalStat(Dictionary bencoded)
    {
        if (bencoded.TryGetValue((Text)"type", out var legacyStatType))
        {
            StatType = Nekoyume.Model.Stat.StatTypeExtension.Deserialize((Binary)legacyStatType);
            BaseValue = bencoded["value"].ToDecimal();
        }
        else if (bencoded.TryGetValue((Text)"additionalValue", out var additionalValue))
        {
            StatType = Nekoyume.Model.Stat.StatTypeExtension.Deserialize(
                (Binary)bencoded["statType"]
            );
            BaseValue = bencoded["value"].ToDecimal();
            AdditionalValue = additionalValue.ToDecimal();
        }
        else
        {
            StatType = Nekoyume.Model.Stat.StatTypeExtension.Deserialize(
                (Binary)bencoded["statType"]
            );
            BaseValue = bencoded["value"].ToDecimal();
        }
    }

    public IValue Bencoded =>
        new Dictionary(
            new Dictionary<IKey, IValue>
            {
                [(Text)"statType"] = StatType.Serialize(),
                [(Text)"value"] = BaseValue.Serialize(),
                [(Text)"additionalValue"] = AdditionalValue.Serialize(),
            }
        );
}
