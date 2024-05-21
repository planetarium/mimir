using Lib9c;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Agent;
using Mimir.Services;
using Mimir.Util;

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
    public async Task<List<string>> GetBalances(
        string network,
        string address,
        IStateService stateService)
    {
        Address agentAddress;
        try
        {
            agentAddress = new Address(address);
        }
        catch (FormatException)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return [];
        }

        var stateGetter = new StateGetter(stateService);
        var ncg = await stateGetter.GetBalanceAsync(agentAddress, _ncg);
        var crystal = await stateGetter.GetBalanceAsync(agentAddress, Currencies.Crystal);
        return
        [
            $"{ncg} {_ncg.Ticker}",
            $"{crystal} {Currencies.Crystal.Ticker}",
        ];
    }

    [HttpGet("balances/{currency}")]
    public async Task<string> GetBalances(
        string network,
        string address,
        string currency,
        IStateService stateService)
    {
        Address agentAddress;
        try
        {
            agentAddress = new Address(address);
        }
        catch (FormatException)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return string.Empty;
        }

        Currency? c = currency switch
        {
            "NCG" => _ncg,
            "CRYSTAL" => Currencies.Crystal,
            _ => null,
        };
        if (c is null)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return string.Empty;
        }

        var stateGetter = new StateGetter(stateService);
        var balance = await stateGetter.GetBalanceAsync(agentAddress, c.Value);
        return $"{balance} {c.Value.Ticker}";
    }

    [HttpGet("avatars")]
    public async Task<AvatarsResponse> GetAvatars(
        string network,
        string address,
        IStateService stateService)
    {
        Address agentAddress;
        try
        {
            agentAddress = new Address(address);
        }
        catch (FormatException)
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
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
