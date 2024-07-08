using Bencodex;
using Bencodex.Types;
using Nekoyume.Model.State;

namespace Mimir.Models.Stat;

public class DecimalStat : IBencodable
{
    public decimal BaseValue { get; private set; }

    public decimal AdditionalValue { get; private set; }

    public Nekoyume.Model.Stat.StatType StatType { get; private set; }

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
