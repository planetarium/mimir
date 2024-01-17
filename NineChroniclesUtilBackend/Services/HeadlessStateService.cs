using System.Text;
using System.Text.Json;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;

namespace NineChroniclesUtilBackend.Services;

public class HeadlessStateService(IOptions<HeadlessStateServiceOptions> options) : IStateService
{
    private readonly Uri _headlessEndpoint = options.Value.HeadlessEndpoint;
    private readonly HttpClient _httpClient = new();

    private static readonly Codec Codec = new();

    public Task<IValue?[]> GetStates(Address[] addresses)
    {
        return Task.WhenAll(addresses.Select(GetState));
    }

    public async Task<IValue?> GetState(Address address)
    {
        using StringContent jsonContent = new
        (JsonSerializer.Serialize(new
        {
            query = @"query GetState($address: Address!) { state(address: $address) }",
            variables = new {
                address = address.ToString(),
            },
            operationName = "GetState",
        }),
            Encoding.UTF8,
            "application/json");
        using var response = await _httpClient.PostAsync(_headlessEndpoint, jsonContent);
        var resp = await response.Content.ReadFromJsonAsync<GetStateResponse>();

        if (resp.Data.State is null)
        {
            return null;
        }

        return Codec.Decode(Convert.FromHexString(resp.Data.State));
    }
}

record GetStateResponse(GetStateResponseData Data, object Errors);
record GetStateResponseData(string? State);
