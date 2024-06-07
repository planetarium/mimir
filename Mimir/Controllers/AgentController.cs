using Lib9c;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Microsoft.AspNetCore.Mvc;
using Mimir.Models;
using Mimir.Models.Assets;
using Mimir.Services;
using Mimir.Util;
using Mimir.Validators;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/agent/{address}")]
public class AgentController : ControllerBase
{
#pragma warning disable CS0618 // Type or member is obsolete
    private readonly Currency _ncg = Currency.Legacy(
        "NCG",
        2,
        new Address("0x47D082a115c63E7b58B1532d20E631538eaFADde"));
#pragma warning restore CS0618 // Type or member is obsolete

    [HttpGet("balances")]
    public async Task<List<Balance>?> GetBalances(
        string network,
        string address,
        IStateService stateService)
    {
        if (!AddressValidator.TryValidate(
                address,
                out var agentAddress,
                out var errorMessage))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            Response.ContentType = "application/json";
            await Response.WriteAsJsonAsync(new { message = errorMessage });
            return null;
        }

        var stateGetter = new StateGetter(stateService);
        var ncg = await stateGetter.GetBalanceAsync(agentAddress, _ncg);
        var crystal = await stateGetter.GetBalanceAsync(agentAddress, Currencies.Crystal);
        return new List<Balance>
        {
            new(_ncg, ncg),
            new(Currencies.Crystal, crystal),
        };
    }

    [HttpGet("avatars")]
    public async Task<AvatarsResponse> GetAvatars(
        string network,
        string address,
        IStateService stateService)
    {
        if (!AddressValidator.TryValidate(
                address,
                out var agentAddress,
                out var errorMessage))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            Response.ContentType = "application/json";
            await Response.WriteAsJsonAsync(new { message = errorMessage });
            return new AvatarsResponse([]);
        }

        var stateGetter = new StateGetter(stateService);
        var avatars = await stateGetter.GetAvatarStatesAsync(agentAddress);
        if (avatars is null)
        {
            Response.StatusCode = StatusCodes.Status404NotFound;
            return new AvatarsResponse([]);
        }

        return new AvatarsResponse(avatars.Select(e => new Avatar(e)).ToList());
    }
}
