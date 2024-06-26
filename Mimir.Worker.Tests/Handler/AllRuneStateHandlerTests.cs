using Bencodex;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;

namespace Mimir.Worker.Tests.Handler;

public class AllRuneStateHandlerTests
{
    private static readonly Codec Codec = new();
    private readonly AllRuneStateHandler _handler = new();

    [Theory]
    [InlineData(0, 0)]
    [InlineData(999, 999)]
    public void ConvertToStateData(int runeId, int level)
    {
        var address = new PrivateKey().Address;
        var allRuneState = new Nekoyume.Model.State.AllRuneState(runeId, level);
        var context = new StateDiffContext
        {
            Address = address,
            RawState = allRuneState.Serialize(),
        };
        var stateData = _handler.ConvertToStateData(context);

        Assert.IsType<AllRuneState>(stateData.State);
        var dataState = (AllRuneState)stateData.State;
        Assert.Equal(address, dataState.address);
        Assert.Equal(allRuneState.Runes.Count, dataState.Object.Runes.Count);
        foreach (var (key, value) in allRuneState.Runes)
        {
            Assert.Contains(dataState.Object.Runes, r => r.Key == key);
            Assert.Equal(value.Level, dataState.Object.Runes[key].Level);
        }
    }
}
