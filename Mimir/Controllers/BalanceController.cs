using Lib9c;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Microsoft.AspNetCore.Mvc;
using Mimir.Models.Assets;
using Mimir.Services;
using Mimir.Util;
using Mimir.Validators;

namespace Mimir.Controllers;

[ApiController]
[Route("{network}/balances/{address}")]
public class BalanceController : ControllerBase
{
#pragma warning disable CS0618 // Type or member is obsolete
    private readonly Currency _ncg = Currency.Legacy(
        "NCG",
        2,
        new Address("0x47D082a115c63E7b58B1532d20E631538eaFADde"));
#pragma warning restore CS0618 // Type or member is obsolete

    [HttpGet("{currencyTicker}")]
    public async Task<Balance?> GetBalance(
        string network,
        string address,
        string currencyTicker,
        IStateService stateService)
    {
        if (!AddressValidator.TryValidate(
                address,
                out var balanceAddress,
                out var errorMessage))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            Response.ContentType = "application/json";
            await Response.WriteAsJsonAsync(new { message = errorMessage });
            return null;
        }

        Currency? c = currencyTicker switch
        {
            "NCG" => _ncg,
            "CRYSTAL" => Currencies.Crystal,
            _ => null,
        };
        if (c is null)
        {
            try
            {
                c = Currencies.GetMinterlessCurrency(currencyTicker);
            }
            catch (ArgumentNullException)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return null;
            }
            catch (ArgumentException)
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
                return null;
            }
        }

        var stateGetter = new StateGetter(stateService);
        var balance = await stateGetter.GetBalanceAsync(balanceAddress, c.Value);
        return new Balance(c.Value, balance);
    }
}
