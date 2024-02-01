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
            var rawState = await stateService.GetState(agentAddress);
            if (rawState is not Dictionary agentStateDictionary)
            {
                throw new ArgumentException(nameof(agentAddress));
            }
            List<AvatarState> avatars = new List<AvatarState>();

            var agentState = new AgentState(agentStateDictionary);
            foreach(var avatarAddressKey in agentState.avatarAddresses.Keys)
            {
                var rawAvatarState = await stateService.GetState(agentState.avatarAddresses[avatarAddressKey]);

                if (rawAvatarState is not Dictionary avatarStateDictionary)
                {
                    throw new ArgumentException(nameof(avatarStateDictionary));
                }

                var avatarState = new AvatarState(avatarStateDictionary);
                avatars.Add(avatarState);
            }

            return avatars;
        }
        var avatars = await GetAvatarsState(new Address(agentAddress));

        return new AvatarsResponse(avatars.Select(a => new Avatar(a.address.ToString(), a.name, a.level)).ToList());
    }
}
