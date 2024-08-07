using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace Mimir.Worker.Client;

public class HeadlessGQLClient
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

    private async Task<T> PostGraphQLRequestAsync<T>(string query, object variables)
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

        var response = await _httpClient.PostAsync(_url, content);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var graphQLResponse = JsonSerializer.Deserialize<GraphQLResponse<T>>(jsonResponse);

        if (graphQLResponse is null)
        {
            throw new HttpRequestException("Response data is null.");
        }

        return graphQLResponse.data;
    }

    public Task<GetAccountDiffsResponse> GetAccountDiffsAsync(
        long baseIndex,
        long changedIndex,
        string accountAddress,
        CancellationToken stoppingToken
    )
    {
        var query =
            @"
                query GetAccountDiffs($baseIndex: Long!, $changedIndex: Long!, $accountAddress: Address!) {
                    accountDiffs(baseIndex: $baseIndex, changedIndex: $changedIndex, accountAddress: $accountAddress) {
                        path
                        baseState
                        changedState
                    }
                }";

        var variables = new
        {
            baseIndex,
            changedIndex,
            accountAddress
        };

        return PostGraphQLRequestAsync<GetAccountDiffsResponse>(query, variables);
    }

    public Task<GetTipResponse> GetTipAsync()
    {
        var query =
            @"
                query GetTip {
                    nodeStatus {
                        tip {
                            index
                        }
                    }
                }";

        return PostGraphQLRequestAsync<GetTipResponse>(query, null);
    }

    public Task<GetStateResponse> GetStateAsync(string accountAddress, string address)
    {
        var query =
            @"
                query GetState($accountAddress: Address!, $address: Address!) {
                    state(accountAddress: $accountAddress, address: $address)
                }";

        var variables = new { accountAddress, address };

        return PostGraphQLRequestAsync<GetStateResponse>(query, variables);
    }

    public Task<GetTransactionsResponse> GetTransactionsAsync(
        long blockIndex,
        long limit,
        CancellationToken stoppingToken
    )
    {
        var query =
            @"
                query GetTransactions($blockIndex: Long!, $limit: Long!) {
                    transaction {
                        ncTransactions(startingBlockIndex: $blockIndex, limit: $limit, actionType: ""^.*$"", txStatusFilter: [SUCCESS]) {
                            signer
                            id
                            serializedPayload
                            actions {
                                raw
                            }
                        }
                    }
                }";

        var variables = new { blockIndex, limit };

        return PostGraphQLRequestAsync<GetTransactionsResponse>(query, variables);
    }
}
