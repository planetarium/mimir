using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.Worker.StateDocumentConverter;

namespace Mimir.Worker.Tests.StateDocumentConverter;

public class ArenaParticipantDocumentConverterTests
{
    private readonly ArenaParticipantDocumentConverter _converter = new();

    [Fact]
    public void ConvertToStateData()
    {
        var address = new PrivateKey().Address;
        var state = new Nekoyume.Model.Arena.ArenaParticipant(address)
        {
            Name = "Test Participant",
        };
        var bencoded = state.Bencoded;
        var context = new AddressStatePair
        {
            Address = address,
            RawState = bencoded,
        };
        var doc = _converter.ConvertToDocument(
            context,
            1,
            1,
            null);
        Assert.IsType<ArenaParticipantDocument>(doc);
        var arenaParticipantDoc = (ArenaParticipantDocument)doc;
        Assert.Equal(address, arenaParticipantDoc.Address);
        Assert.Equal(bencoded, arenaParticipantDoc.Object.Bencoded);
        Assert.Equal(1, arenaParticipantDoc.ChampionshipId);
        Assert.Equal(1, arenaParticipantDoc.Round);
        Assert.Null(arenaParticipantDoc.SimpleAvatar);
    }
}
