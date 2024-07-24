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
            RawState = collectionState.Bencoded,
        };
        var state = _handler.ConvertToState(context);

        Assert.IsType<CollectionState>(state);
        var dataState = (CollectionState)state;
        Assert.Equal(collectionState.Ids, dataState.Object.Ids);
    }
}
