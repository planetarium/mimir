using Bencodex;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;

namespace Mimir.Worker.Tests.Handler;

public class CollectionStateHandlerTests
{
    private static readonly Codec Codec = new();
    private readonly CollectionStateHandler _handler = new();

    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    public void ConvertToStateData(int idCount)
    {
        var address = new PrivateKey().Address;
        var collectionState = new Nekoyume.Model.State.CollectionState();
        for (var i = 0; i < idCount; i++)
        {
            collectionState.Ids.Add(i);
        }

        var context = new StateDiffContext
        {
            Address = address,
            RawState = Codec.Decode(Codec.Encode(collectionState.Bencoded)),
        };
        var stateData = _handler.ConvertToStateData(context);

        Assert.IsType<CollectionState>(stateData.State);
        var dataState = (CollectionState)stateData.State;
        Assert.Equal(address, dataState.address);
        Assert.Equal(collectionState.Ids, dataState.Object.Ids);
    }
}
