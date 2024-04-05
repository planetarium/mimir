using Microsoft.AspNetCore.Mvc;
using NineChroniclesUtilBackend.Models.Agent;
using Bencodex.Types;
using Libplanet.Crypto;
using Microsoft.AspNetCore.Mvc;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using NineChroniclesUtilBackend.Models.Arena;
using NineChroniclesUtilBackend.Services;
using NineChroniclesUtilBackend.Arena;


namespace NineChroniclesUtilBackend.Controllers;

[ApiController]
[Route("agent")]
public class AccountController : ControllerBase
{
    [HttpGet("{agentAddress}/avatars")]
    public async Task<AvatarsResponse> GetAvatars(string agentAddress, IStateService stateService)
    {
        async Task<List<AvatarState>> GetAvatarsState(Address agentAddress)
        {
            var rawState = await stateService.GetState(agentAddress, Addresses.Agent) ??
                           await stateService.GetState(agentAddress);
            var agentState = rawState switch
            {
                Dictionary agentStateDictionary => new AgentState(agentStateDictionary),
                List agentStateList => new AgentState(agentStateList),
                _ => throw new ArgumentException(nameof(agentAddress)),
            };

            List<AvatarState> avatars = new List<AvatarState>();

            foreach(var avatarAddressKey in agentState.avatarAddresses.Keys)
            {
                var avatarAddress = agentState.avatarAddresses[avatarAddressKey];
                var rawAvatarState =
                    await stateService.GetState(avatarAddress, Addresses.Avatar) ??
                    await stateService.GetState(avatarAddress);

                var avatarState = rawAvatarState switch
                {
                    Dictionary avatarStateDictionary => new AvatarState(avatarStateDictionary),
                    List avatarStateList => new AvatarState(avatarStateList),
                    _ => throw new ArgumentException(nameof(avatarAddress))
                };
                avatars.Add(avatarState);
            }

            return avatars;
        }
        var avatars = await GetAvatarsState(new Address(agentAddress));

        return new AvatarsResponse(avatars.Select(a => new Avatar(a.address.ToString(), a.name, a.level)).ToList());
    }
}
