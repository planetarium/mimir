using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Nekoyume.Action;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class InfiniteTowerInfoStateDocumentConverterTests
{
    private readonly InfiniteTowerInfoStateDocumentConverter _converter = new();

    [Fact]
    public void ConvertToDocument_ConvertsAddressStatePairToInfiniteTowerInfoDocument()
    {
        // Arrange
        var avatarAddress = new Address("0x1234567890123456789012345678901234567890");
        var infiniteTowerId = 1;
        var accountAddress = Addresses.InfiniteTowerInfo.Derive($"{infiniteTowerId}");
        var blockIndex = 1000L;

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

        var rawState = infiniteTowerInfo.Bencoded;
        var context = new AddressStatePair
        {
            BlockIndex = blockIndex,
            Address = accountAddress,
            RawState = rawState,
        };

        // Act
        var document = _converter.ConvertToDocument(context);

        // Assert
        Assert.IsType<InfiniteTowerInfoDocument>(document);
        var infiniteTowerInfoDocument = (InfiniteTowerInfoDocument)document;
        Assert.Equal(blockIndex, infiniteTowerInfoDocument.StoredBlockIndex);
        Assert.Equal(accountAddress, infiniteTowerInfoDocument.Address);
        Assert.Equal(avatarAddress, infiniteTowerInfoDocument.AvatarAddress);
        Assert.Equal(infiniteTowerId, infiniteTowerInfoDocument.InfiniteTowerId);
        Assert.Equal(infiniteTowerInfo.Address, infiniteTowerInfoDocument.Object.Address);
        Assert.Equal(infiniteTowerInfo.InfiniteTowerId, infiniteTowerInfoDocument.Object.InfiniteTowerId);
        Assert.Equal(infiniteTowerInfo.ClearedFloor, infiniteTowerInfoDocument.Object.ClearedFloor);
        Assert.Equal(infiniteTowerInfo.RemainingTickets, infiniteTowerInfoDocument.Object.RemainingTickets);
        Assert.Equal(infiniteTowerInfo.TotalTicketsUsed, infiniteTowerInfoDocument.Object.TotalTicketsUsed);
        Assert.Equal(infiniteTowerInfo.NumberOfTicketPurchases, infiniteTowerInfoDocument.Object.NumberOfTicketPurchases);
        Assert.Equal(infiniteTowerInfo.LastResetBlockIndex, infiniteTowerInfoDocument.Object.LastResetBlockIndex);
        Assert.Equal(infiniteTowerInfo.LastTicketRefillBlockIndex, infiniteTowerInfoDocument.Object.LastTicketRefillBlockIndex);
    }
}
