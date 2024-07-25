using Nekoyume.Model.Stat;
using Nekoyume.Model.State;
using DecimalStat = Lib9c.Models.Stat.DecimalStat;

namespace Lib9c.Models.Tests.Stats;

public class DecimalStatTest
{
    [Fact]
    public void Serialize_With_Bencoded()
    {
        // Prepare target state
        var target = new Nekoyume.Model.Stat.DecimalStat(StatType.HP);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new DecimalStat(serialized);
        Assert.Equal(target.StatType, paired.StatType);
        Assert.Equal(target.BaseValue, paired.BaseValue);
        Assert.Equal(target.AdditionalValue, paired.AdditionalValue);

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = bencoded.ToDecimalStat();
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }

    [Fact]
    public void SerializeWithoutAdditional_With_BencodedWithoutAdditionalValue()
    {
        // Prepare target state
        var target = new Nekoyume.Model.Stat.DecimalStat(StatType.HP);

        // serialize target state and deserialize as paired state
        var serialized = target.SerializeWithoutAdditional();
        var paired = new DecimalStat(serialized);
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
        var target = new Nekoyume.Model.Stat.DecimalStat(StatType.HP);

        // serialize target state and deserialize as paired state
        var serialized = target.SerializeForLegacyEquipmentStat();
        var paired = new DecimalStat(serialized);
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
