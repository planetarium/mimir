using Bencodex.Types;
using Lib9c.Models.Arena;
using Lib9c.Models.Tests.Fixtures.States;

namespace Lib9c.Models.Tests.States;

public class ArenaParticipantTest
{
    [Fact]
    public void Test()
    {
        // Prepare target state
        var value = StateReader.ReadState("ArenaParticipant");
        var target = new Nekoyume.Model.Arena.ArenaParticipant((List)value);

        // serialize target state and deserialize as paired state
        var targetBencoded = target.Bencoded;
        var paired = new ArenaParticipant(targetBencoded);
        Assert.Equal(target.AvatarAddr, paired.AvatarAddr);
        Assert.Equal(target.Name, paired.Name);
        Assert.Equal(target.PortraitId, paired.PortraitId);
        Assert.Equal(target.Level, paired.Level);
        Assert.Equal(target.Cp, paired.Cp);
        Assert.Equal(target.Score, paired.Score);
        Assert.Equal(target.Ticket, paired.Ticket);
        Assert.Equal(target.TicketResetCount, paired.TicketResetCount);
        Assert.Equal(target.PurchasedTicketCount, paired.PurchasedTicketCount);
        Assert.Equal(target.Win, paired.Win);
        Assert.Equal(target.Lose, paired.Lose);
        Assert.Equal(target.LastBattleBlockIndex, paired.LastBattleBlockIndex);

        // serialize paired state and verify
        var pairedBencoded = paired.Bencoded;
        Assert.Equal(targetBencoded, pairedBencoded);

        // deserialize bencoded state as target2 and verify
        var target2 = new Nekoyume.Model.Arena.ArenaParticipant((List)pairedBencoded);
        var target2Bencoded = target2.Bencoded;
        Assert.Equal(targetBencoded, target2Bencoded);
    }
}
