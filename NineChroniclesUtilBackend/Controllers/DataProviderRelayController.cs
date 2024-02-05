using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NineChroniclesUtilBackend.Options;

namespace NineChroniclesUtilBackend.Controllers;

[ApiController]
[Route("dp")]
public class DataProviderRelayController(IOptions<DataProviderOption> options) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromServices] IHttpClientFactory clientFactory)
    {
        var body = await new StreamReader(Request.Body).ReadToEndAsync();

        var client = clientFactory.CreateClient();
    
        try
        {
            var response = await client.PostAsync(options.Value.Endpoint,
                new StringContent(body, Encoding.UTF8, "application/json"));

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            return Content(responseString, "application/json");
        }
        catch (HttpRequestException e)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, e.Message);
        }
    }
}
