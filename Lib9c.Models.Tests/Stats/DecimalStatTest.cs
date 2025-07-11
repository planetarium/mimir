using Bencodex.Types;
using Lib9c.Models.Extensions;
using Nekoyume.Model.Stat;

namespace Lib9c.Models.Tests.Stats;

public class DecimalStatTest
{
    [Fact]
    public void Serialize_With_Bencoded()
    {
        // Prepare target state
        var dictSerialized = new Dictionary(new Dictionary<IKey, IValue>
        {
            [(Text)"statType"] = StatType.HP.Serialize(),
            [(Text)"value"] = 100m.Serialize(),
            [(Text)"additionalValue"] = 200m.Serialize(),
        });

        var target = new DecimalStat(dictSerialized);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new Models.Stats.DecimalStat(serialized);

        // Check Deserialize from List
        Assert.Equal(target.StatType, paired.StatType);
        Assert.Equal(target.BaseValue, paired.BaseValue);
        Assert.Equal(target.AdditionalValue, paired.AdditionalValue);

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        var target2 = bencoded.ToDecimalStat();
        Assert.Equal(target.StatType, target2.StatType);
        Assert.Equal(target.BaseValue, target2.BaseValue);
        Assert.Equal(target.AdditionalValue, target2.AdditionalValue);
    }

    [Fact]
    public void SerializeWithoutAdditional_With_BencodedWithoutAdditionalValue()
    {
        // Prepare target state
        var target = new DecimalStat(StatType.HP);

        // serialize target state and deserialize as paired state
        var serialized = target.SerializeWithoutAdditional();
        var paired = new Models.Stats.DecimalStat(serialized);
        Assert.Equal(target.StatType, paired.StatType);
        Assert.Equal(target.BaseValue, paired.BaseValue);
        Assert.Equal(target.AdditionalValue, paired.AdditionalValue);

        // serialize paired state and verify
        var bencoded = paired.BencodedWithoutAdditionalValue;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = bencoded.ToDecimalStat();
        var serialized2 = target2.SerializeWithoutAdditional();
        Assert.Equal(serialized, serialized2);
    }

    [Fact]
    public void SerializeForLegacyEquipmentStat_With_BencodedAsLegacy()
    {
        // Prepare target state
        var target = new DecimalStat(StatType.HP);

        // serialize target state and deserialize as paired state
        var serialized = target.SerializeForLegacyEquipmentStat();
        var paired = new Models.Stats.DecimalStat(serialized);
        Assert.Equal(target.StatType, paired.StatType);
        Assert.Equal(target.BaseValue, paired.BaseValue);
        Assert.Equal(target.AdditionalValue, paired.AdditionalValue);

        // serialize paired state and verify
        var bencoded = paired.BencodedAsLegacy;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = bencoded.ToDecimalStat();
        var serialized2 = target2.SerializeForLegacyEquipmentStat();
        Assert.Equal(serialized, serialized2);
    }
}
