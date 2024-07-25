using Bencodex.Types;
using Lib9c.Models.States;
using Nekoyume.Model.EnumType;

namespace Lib9c.Models.Tests.States;

public class RuneSlotStateTest
{
    [Fact]
    public void Test()
    {
        var target = new Nekoyume.Model.State.RuneSlotState(BattleType.Arena);
        var serialized = target.Serialize();
        var paired = new RuneSlotState(serialized);
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
        var target2 = new Nekoyume.Model.State.RuneSlotState((List)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
