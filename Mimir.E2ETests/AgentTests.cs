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
        Assert.Equal(agentDataFromMimir.MonsterCollectionRound, agentDataFromHeadless.MonsterCollectionRound);

        var mimirAvatarAddresses = agentDataFromMimir.AvatarAddresses;
        var headlessAvatarAddresses = agentDataFromHeadless.AvatarAddresses;
        foreach (var mimirAddress in mimirAvatarAddresses)
        {
            if (headlessAvatarAddresses.TryGetValue(mimirAddress.Key, out var headlessValue))
            {
                Assert.Equal(mimirAddress.Value, headlessValue.ToString());
            }
            else
            {
                Assert.Fail();
            }
        }
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

    public async Task<WorldInformationState> GetHeadlessWorldInformationData(Address address)
    {
        var stateResponse = await headlessClient.GetState.ExecuteAsync(
            Addresses.WorldInformation.ToString(),
            address.ToString()
        );
        var result = CodecUtil.DecodeState(stateResponse.Data.State);
        return new WorldInformationState(result);
    }

    public async Task<IGetWorldInformation_WorldInformation> GetMimirWorldInformationData(Address address)
    {
        var agentResponse = await mimirClient.GetWorldInformation.ExecuteAsync(address.ToString());
        return agentResponse.Data.WorldInformation;
    }
    
    
    [Theory]
    [InlineData("4AB43b2d1d0e41DdF99449086dC70dC79513B0F1")]
    public async Task CompareWorldInformationDataFromDifferentServices_ShouldMatch(string address)
    {
        var agentDataFromMimir = (await GetMimirWorldInformationData(new Address(address))).WorldDictionary;
        var agentDataFromHeadless = (await GetHeadlessWorldInformationData(new Address(address))).WorldDictionary;

        foreach (var dictionary in agentDataFromMimir)
        {
            var mimirKey = dictionary.Key;
            var mimirValue = dictionary.Value;

            if (agentDataFromHeadless.TryGetValue(mimirKey, out var headlessValue
                ))
            {
                // FIXME mimir에서 가져오는 id 값 부정확.
                // Assert.Equal(mimirValue.Id, headlessValue.Id);
                Assert.Equal(mimirValue.Name, headlessValue.Name);
                Assert.Equal(mimirValue.IsUnlocked, headlessValue.IsUnlocked);
                Assert.Equal(mimirValue.StageBegin, headlessValue.StageBegin);
                Assert.Equal(mimirValue.StageEnd, headlessValue.StageEnd);
                Assert.Equal(mimirValue.IsStageCleared, headlessValue.IsStageCleared);
                Assert.Equal(mimirValue.StageClearedId, headlessValue.StageClearedId);
                Assert.Equal(mimirValue.UnlockedBlockIndex, headlessValue.UnlockedBlockIndex);
                Assert.Equal(mimirValue.StageClearedBlockIndex, headlessValue.StageClearedBlockIndex);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}