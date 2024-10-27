using Bencodex.Types;
using HeadlessGQL;
using Lib9c.Models.States;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using MimirGQL;

namespace Mimir.E2ETests;

public class StakeTests : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;

    public StakeTests(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
    }

    [Theory]
    [InlineData("0x08eB36BB2B46073149fE9DaCB9706d2b49Fa6115")]
    [InlineData("0x2EB4c1C19E5664feC2eb722FB01df6eBdf5014e2")]
    public async Task CompareStakeDataFromDifferentServices_ShouldMatch(string address)
    {
        var agentDataFromMimir = await GetMimirStakeData(new Address(address));

        var stakeAddress = Nekoyume.Model.State.StakeState.DeriveAddress(new Address(address));
        var agentDataFromHeadless = await GetHeadlessStakeData(stakeAddress);
        if (agentDataFromMimir == null)
        {
            // FIXME; if agentDataFromMimir is null, stakeAddress should be null as well, but now it isn't
            return;
        }

        Assert.Equal(agentDataFromMimir.StartedBlockIndex, agentDataFromHeadless.StartedBlockIndex);
    }

    private async Task<IGetStake_Stake> GetMimirStakeData(Address address)
    {
        var agentResponse = await mimirClient.GetStake.ExecuteAsync(address.ToString());
        var agentData = agentResponse.Data.Stake;

        return agentData;
    }

    private async Task<StakeState?> GetHeadlessStakeData(Address address)
    {
        var stateResponse = await headlessClient.GetState.ExecuteAsync(
            ReservedAddresses.LegacyAccount.ToString(),
            address.ToString()
        );
        var result = CodecUtil.DecodeState(stateResponse.Data.State);
        if (result.Kind == ValueKind.Null)
        {
            return null;
        }

        return new StakeState(result);
    }
}