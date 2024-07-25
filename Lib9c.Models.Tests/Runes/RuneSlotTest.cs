using Bencodex.Types;
using Lib9c.Models.Runes;
using Nekoyume.Model.EnumType;

namespace Lib9c.Models.Tests.Runes;

public class RuneSlotTest
{
    [Fact]
    public void Test()
    {
        var target = new Nekoyume.Model.Rune.RuneSlot(
            1,
            RuneSlotType.Crystal,
            RuneType.Skill,
            true);
        var serialized = target.Serialize();
        var paired = new RuneSlot(serialized);
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
        var target2 = new Nekoyume.Model.Rune.RuneSlot((List)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
