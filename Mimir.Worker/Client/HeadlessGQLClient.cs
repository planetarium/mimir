using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Mimir.Worker.Client;

public class HeadlessGQLClient : IHeadlessGQLClient
{
    private readonly HttpClient _httpClient;
    private readonly Uri _url;
    private readonly string? _issuer;
    private readonly string? _secret;

    public HeadlessGQLClient(Uri url, string? issuer, string? secret)
    {
        _httpClient = new HttpClient();
        _url = url;
        _issuer = issuer;
        _secret = secret;
    }

    private string GenerateJwtToken(string secret, string issuer)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            expires: DateTime.UtcNow.AddMinutes(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<T> PostGraphQLRequestAsync<T>(
        string query,
        object variables,
        CancellationToken stoppingToken = default
    )
    {
        if (_secret is not null && _issuer is not null)
        {
            var token = GenerateJwtToken(_secret, _issuer);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                token
            );
        }

        var request = new GraphQLRequest { query = query, variables = variables };

        var jsonRequest = JsonSerializer.Serialize(request);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_url, content, stoppingToken);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync(stoppingToken);
        var graphQLResponse = JsonSerializer.Deserialize<GraphQLResponse<T>>(jsonResponse);

        if (graphQLResponse is null)
        {
            throw new HttpRequestException("Response data is null.");
        }

        return graphQLResponse.data;
    }

    public async Task<GetAccountDiffsResponse> GetAccountDiffsAsync(
        long baseIndex,
        long changedIndex,
        string accountAddress,
        CancellationToken stoppingToken
    )
    {
        return await PostGraphQLRequestAsync<GetAccountDiffsResponse>(
            GraphQLQueries.GetAccountDiffs,
            new
            {
                baseIndex,
                changedIndex,
                accountAddress
            },
            stoppingToken
        );
    }

    public Task<GetAccountDiffsResponse> GetAccountDiffsAsync(
        long baseIndex,
        long changedIndex,
        string accountAddress
    )
    {
        return PostGraphQLRequestAsync<GetAccountDiffsResponse>(
            GraphQLQueries.GetAccountDiffs,
            new
            {
                baseIndex,
                changedIndex,
                accountAddress
            }
        );
    }

    public Task<GetTipResponse> GetTipAsync(CancellationToken stoppingToken)
    {
        return PostGraphQLRequestAsync<GetTipResponse>(GraphQLQueries.GetTip, null, stoppingToken);
    }

    public Task<GetTipResponse> GetTipAsync()
    {
        return PostGraphQLRequestAsync<GetTipResponse>(GraphQLQueries.GetTip, null);
    }

    public Task<GetStateResponse> GetStateAsync(
        string accountAddress,
        string address,
        CancellationToken stoppingToken
    )
    {
        return PostGraphQLRequestAsync<GetStateResponse>(
            GraphQLQueries.GetState,
            new { accountAddress, address },
            stoppingToken
        );
    }

    public Task<GetStateResponse> GetStateAsync(string accountAddress, string address)
    {
        return PostGraphQLRequestAsync<GetStateResponse>(
            GraphQLQueries.GetState,
            new { accountAddress, address }
        );
    }

    public Task<GetTransactionsResponse> GetTransactionsAsync(
        long blockIndex,
        long limit,
        CancellationToken stoppingToken
    )
    {
        return PostGraphQLRequestAsync<GetTransactionsResponse>(
            GraphQLQueries.GetTransactions,
            new { blockIndex, limit },
            stoppingToken
        );
    }

    public Task<GetTransactionsResponse> GetTransactionsAsync(long blockIndex, long limit)
    {
        return PostGraphQLRequestAsync<GetTransactionsResponse>(
            GraphQLQueries.GetTransactions,
            new { blockIndex, limit }
        );
    }
}
