using HeadlessGQL;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using MimirGQL;
using Nekoyume;

namespace Mimir.E2ETests;

public class AgentTests : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;

    public AgentTests(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
    }

    [Theory]
    [InlineData("0x6880c5B8EFccce10E955aA5F9aa102eE90c658e8")]
    public async Task CompareAgentDataFromDifferentServices_ShouldMatch(string address)
    {
        var agentDataFromMimir = await GetMimirAgentData(new Address(address));
        var agentDataFromHeadless = await GetHeadlessAgentData(new Address(address));

        Assert.Equal(agentDataFromMimir.Address, agentDataFromHeadless.Address.ToString());
    }

    public async Task<IGetAgent_Agent> GetMimirAgentData(Address address)
    {
        var agentResponse = await mimirClient.GetAgent.ExecuteAsync(address.ToString());
        var agentData = agentResponse.Data.Agent;

        return agentData;
    }

    public async Task<AgentState> GetHeadlessAgentData(Address address)
    {
        var stateResponse = await headlessClient.GetState.ExecuteAsync(
            Addresses.Agent.ToString(),
            address.ToString()
        );
        var result = CodecUtil.DecodeState(stateResponse.Data.State);
        return new AgentState(result);
    }
}
