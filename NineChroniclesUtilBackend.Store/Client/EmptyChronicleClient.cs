using Newtonsoft.Json;
using NineChroniclesUtilBackend.Store.Models;

namespace NineChroniclesUtilBackend.Store.Client;

public class EmptyChroniclesClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public EmptyChroniclesClient(string baseUrl)
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient();
    }

    public async Task<StateResponse> GetStateByAddressAsync(string address, string? accountAddress = null)
    {
        var url = $"{_baseUrl}/api/states/{address}";
        if (accountAddress != null)
        {
            url += $"?account={Uri.EscapeDataString(accountAddress)}";
        }

        var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        var stateResponse = JsonConvert.DeserializeObject<StateResponse>(content);
        if (stateResponse == null)
        {
            throw new InvalidOperationException("StateResponse is null.");
        }

        return stateResponse;
    }
}
