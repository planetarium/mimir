using Bencodex.Types;
using HeadlessGQL;
using Lib9c.Models.Extensions;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using MimirGQL;
using Nekoyume;

namespace Mimir.E2ETests;

public class ActionPointTests : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;

    public ActionPointTests(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
    }

    [Theory]
    [InlineData("9bFA9196e93E8186A22757c367b92c74F7B0BeA3")]
    public async Task CompareActionPointData(string address)
    {
        var actionPointDataFromMimir = await GetMimirActionPointData(new Address(address));
        var actionPointDataFromHeadless = await GetHeadlessActionPointData(new Address(address));

        Assert.Equal(actionPointDataFromMimir, actionPointDataFromHeadless);
    }

    public async Task<int> GetMimirActionPointData(Address avatarAddress)
    {
        var actionPointResponse = await mimirClient.GetActionPoint.ExecuteAsync(avatarAddress.ToString());
        var actionPointData = actionPointResponse.Data.ActionPoint;

        return actionPointData;
    }

    public async Task<int> GetHeadlessActionPointData(Address avatarAddress)
    {
        var stateResponse = await headlessClient.GetState.ExecuteAsync(
            Addresses.ActionPoint.ToString(),
            avatarAddress.ToString()
        );
        var result = CodecUtil.DecodeState(stateResponse.Data.State);

        if (result is not Integer value)
            throw new Exception(
            );
            
        return value;
    }
}
