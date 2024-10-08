using Bencodex.Types;
using Lib9c.Models.Items;
using Lib9c.Models.Tests.Fixtures.States;

namespace Lib9c.Models.Tests.Items;

public class EquipmentTest
{
    [Fact(Skip = "Failed")]
    public void Test()
    {
        // Prepare target state
        var value = StateReader.ReadState("Equipment");
        var target = new Nekoyume.Model.Item.Equipment((Dictionary)value);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new Equipment(serialized);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new Nekoyume.Model.Item.Equipment((Dictionary)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
