using Bencodex.Types;
using Lib9c.Models.Extensions;
using Nekoyume.Model.Stat;
using StatMap = Lib9c.Models.Stats.StatMap;

namespace Lib9c.Models.Tests.Stats;

public class StatMapTest
{
    [Fact]
    public void Test()
    {
        // Prepare legacy state
        var legacyStatsMapDict = (Dictionary) Dictionary.Empty
            .Add(
                StatType.HP.Serialize(),
                Dictionary.Empty
                    .Add("statType", StatType.HP.Serialize())
                    .Add("value", 100m.Serialize())
                    .Add("additionalValue", 0m.Serialize()))
            .Add(
                StatType.ATK.Serialize(),
                Dictionary.Empty
                    .Add("statType", StatType.ATK.Serialize())
                    .Add("value", 50m.Serialize())
                    .Add("additionalValue", 10m.Serialize()));

        // Prepare target state
        var target = new Nekoyume.Model.Stat.StatMap(legacyStatsMapDict);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new StatMap(serialized);

        // Check Deserialize from List
        Assert.Contains(StatType.HP, paired.Value);
        var hpStat = paired.Value[StatType.HP];
        Assert.Equal(100m, hpStat.BaseValue);
        Assert.Equal(0m, hpStat.AdditionalValue);

        Assert.Contains(StatType.ATK, paired.Value);
        var atkStat = paired.Value[StatType.ATK];
        Assert.Equal(50m, atkStat.BaseValue);
        Assert.Equal(10m, atkStat.AdditionalValue);

        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;

        // deserialize bencoded state as target2 and verify
        var target2 = new Nekoyume.Model.Stat.StatMap(bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
