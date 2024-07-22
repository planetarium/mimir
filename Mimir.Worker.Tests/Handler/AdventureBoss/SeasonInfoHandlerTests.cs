using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Handler.AdventureBoss;
using Mimir.Worker.Models.State.AdventureBoss;
using Nekoyume.Model.AdventureBoss;

namespace Mimir.Worker.Tests.Handler.AdventureBoss;

public class SeasonInfoHandlerTests
{
    private readonly SeasonInfoHandler _handler = new();

    [Theory]
    [InlineData(0, 0, 0, 0)]
    // Why use `int.MaxValue` not use `long.MaxValue`?
    // https://github.com/planetarium/lib9c/issues/2701
    [InlineData(int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
    public void ConvertToStateData(
        int season, // FIXME long
        int startBlockIndex, // FIXME long
        int activeInterval, // FIXME long
        int inactiveInterval, // FIXME long
        int? endBlockIndex = null, // FIXME long?
        int? nextStartBlockIndex = null, // FIXME long?
        int? bossId = null)
    {
        var address = new PrivateKey().Address;
        var state = new SeasonInfo(
            season,
            startBlockIndex,
            activeInterval,
            inactiveInterval,
            endBlockIndex,
            nextStartBlockIndex)
        {
            BossId = bossId ?? default,
        };
        var context = new StateDiffContext
        {
            Address = address,
            RawState = state.Bencoded,
        };
        var stateData = _handler.ConvertToStateData(context);

        Assert.IsType<SeasonInfoState>(stateData.State);
        var dataState = (SeasonInfoState)stateData.State;
        var obj = dataState.Object;
        endBlockIndex ??= startBlockIndex + activeInterval;
        nextStartBlockIndex ??= endBlockIndex.Value + inactiveInterval;
        bossId ??= default;
        Assert.Equal(address, obj.Address);
        Assert.Equal(season, obj.Season);
        Assert.Equal(startBlockIndex, obj.StartBlockIndex);
        Assert.Equal(endBlockIndex.Value, obj.EndBlockIndex);
        Assert.Equal(nextStartBlockIndex.Value, obj.NextStartBlockIndex);
        Assert.Equal(bossId, obj.BossId);
    }
}
