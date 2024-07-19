using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Handler.AdventureBoss;
using Mimir.Worker.Models.State.AdventureBoss;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.Worker.Tests.Handler.AdventureBoss;

public class ExplorerListHandlerTests
{
    private readonly ExplorerListHandler _handler = new();

    [Theory]
    [InlineData(0, false)]
    [InlineData(int.MaxValue, true)]
    public void ConvertToStateData(long season, bool initOthers)
    {
        var address = new PrivateKey().Address;
        var state = new ExplorerList(season);
        if (initOthers)
        {
            state.AddExplorer(new PrivateKey().Address, "name");
        }

        var context = new StateDiffContext
        {
            Address = address,
            RawState = state.Bencoded,
        };
        var stateData = _handler.ConvertToStateData(context);

        Assert.IsType<ExplorerListState>(stateData.State);
        var dataState = (ExplorerListState)stateData.State;
        var obj = dataState.Object;
        Assert.Equal(season, obj.Season);

        if (initOthers)
        {
            Assert.Single(obj.Explorers);
            var actual = obj.Explorers.First();
            var expected = state.Explorers.First();
            Assert.Equal(expected.Item1, actual.Item1);
            Assert.Equal(expected.Item2, actual.Item2);
        }
        else
        {
            Assert.Empty(obj.Explorers);
        }
    }
}
