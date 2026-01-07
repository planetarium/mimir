using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Nekoyume;
using Nekoyume.Action;

namespace Mimir.MongoDB.Tests.Bson;

public class InfiniteTowerInfoDocumentTest
{
    [Fact]
    public Task JsonSnapshot()
    {
        var avatarAddress = new Address("0x1234567890123456789012345678901234567890");
        var infiniteTowerId = 1;
        var accountAddress = Nekoyume.Addresses.InfiniteTowerInfo.Derive($"{infiniteTowerId}");
        var infiniteTowerInfo = new InfiniteTowerInfo
        {
            Address = avatarAddress,
            InfiniteTowerId = infiniteTowerId,
            ClearedFloor = 10,
            RemainingTickets = 5,
            TotalTicketsUsed = 15,
            NumberOfTicketPurchases = 2,
            LastResetBlockIndex = 1000,
            LastTicketRefillBlockIndex = 2000,
        };
        var docs = new InfiniteTowerInfoDocument(
            default,
            accountAddress,
            avatarAddress,
            infiniteTowerId,
            infiniteTowerInfo
        );
        return Verify(docs.ToJson());
    }
}
