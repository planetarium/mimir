using Bencodex.Types;
using Lib9c.Models.States;
using Libplanet.Crypto;

namespace Lib9c.Models.Tests.States;

public class InfiniteTowerInfoTest
{
    [Fact]
    public void Test()
    {
        var avatarAddress = new Address("0x1234567890123456789012345678901234567890");
        var infiniteTowerId = 1;
        var initialTickets = 5;
        var target = new Nekoyume.Model.InfiniteTower.InfiniteTowerInfo(
            avatarAddress,
            infiniteTowerId,
            initialTickets
        );
        var serialized = target.Serialize();
        var paired = new InfiniteTowerInfo(serialized);
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
        var target2 = new Nekoyume.Model.InfiniteTower.InfiniteTowerInfo((List)bencoded);
        var serialized2 = target2.Serialize();
        Assert.Equal(serialized, serialized2);
    }
}
