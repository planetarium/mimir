using HeadlessGQL;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Microsoft.Extensions.DependencyInjection;
using MimirGQL;
using Nekoyume;

namespace Mimir.E2ETests;

public class AvatarTests : IClassFixture<GraphQLClientFixture>
{
    private readonly IMimirClient mimirClient;
    private readonly IHeadlessClient headlessClient;

    public AvatarTests(GraphQLClientFixture fixture)
    {
        mimirClient = fixture.ServiceProvider.GetRequiredService<IMimirClient>();
        headlessClient = fixture.ServiceProvider.GetRequiredService<IHeadlessClient>();
    }

    [Theory]
    [InlineData("0xD292eA111A72cCB0c95aE9D122fD95b16e1142B8")]
    public async Task CompareAvatarDataFromDifferentServices_ShouldMatch(string address)
    {
        var avatarDataFromMimir = await GetMimirAvatarData(new Address(address));
        var avatarDataFromHeadless = await GetHeadlessAvatarData(new Address(address));

        Assert.Equal(avatarDataFromMimir.Address, avatarDataFromHeadless.Address.ToString());
    }

    public async Task<IGetAvatar_Avatar> GetMimirAvatarData(Address address)
    {
        var avatarResponse = await mimirClient.GetAvatar.ExecuteAsync(address.ToString());    
        var avatarData = avatarResponse.Data.Avatar;

        return avatarData;
    }

    public async Task<AvatarState> GetHeadlessAvatarData(Address address)
    {
        var stateResponse = await headlessClient.GetState.ExecuteAsync(
            Addresses.Avatar.ToString(),
            address.ToString()
        );
        var result = CodecUtil.DecodeState(stateResponse.Data.State);
        return new AvatarState(result);
    }
}
