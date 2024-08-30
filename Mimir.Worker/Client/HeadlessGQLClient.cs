using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Libplanet.Crypto;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Mimir.Worker.Client;

public class HeadlessGQLClient : IHeadlessGQLClient
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly Uri[] _urls;
    private readonly string? _issuer;
    private readonly string? _secret;
    private const int RetryAttempts = 3;
    private const int DelayInSeconds = 5;

    public HeadlessGQLClient(Uri[] urls, string? issuer, string? secret)
    {
        _httpClient = new HttpClient();
        _urls = urls;
        _issuer = issuer;
        _secret = secret;
        _logger = Log.ForContext<HeadlessGQLClient>();

        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    private string GenerateJwtToken(string secret, string issuer)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(5);

        var token = new JwtSecurityToken(
            issuer: issuer,
            expires: expiration,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<T> PostGraphQLRequestAsync<T>(
        string query,
        object? variables,
        CancellationToken stoppingToken = default,
        ILogger? contextualLogger = null
    )
    {
        var logger = contextualLogger is null ? _logger : contextualLogger;

        for (int attempt = 0; attempt < RetryAttempts; attempt++)
        {
            foreach (var url in _urls)
            {
                try
                {
                    logger.Information(
                        "Request data: url: {Url}, query: {Query}, retry:{Retry}",
                        url,
                        query,
                        attempt + 1
                    );
                    var request = new GraphQLRequest { Query = query, Variables = variables };
                    var jsonRequest = JsonSerializer.Serialize(request);
                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, url)
                    {
                        Content = content
                    };

                    if (_secret is not null && _issuer is not null)
                    {
                        var token = GenerateJwtToken(_secret, _issuer);
                        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(
                            "Bearer",
                            token
                        );
                    }

                    var response = await _httpClient.SendAsync(httpRequest, stoppingToken);
                    response.EnsureSuccessStatusCode();

                    var jsonResponse = await response.Content.ReadAsStringAsync(stoppingToken);
                    var graphQLResponse = JsonSerializer.Deserialize<GraphQLResponse<T>>(
                        jsonResponse
                    );

                    if (graphQLResponse is null || graphQLResponse.Data is null)
                    {
                        throw new HttpRequestException("Response data is null.");
                    }

                    logger.Information(
                        "Successfully received the data: url: {Url}, query: {Query}, retry:{Retry}",
                        url,
                        query,
                        attempt + 1
                    );

                    return graphQLResponse.Data;
                }
                catch (HttpRequestException ex)
                {
                    logger.Error(
                        "Failed http request url: {Url}, retry:{Retry}, error:\n{Errors}",
                        url,
                        attempt + 1,
                        ex
                    );
                }
                catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
                {
                    logger.Error(
                        "Timeout http request url: {Url}, retry: {Retry}, error:\n{Errors}",
                        url,
                        attempt + 1,
                        ex
                    );
                }
                catch (Exception ex)
                {
                    logger.Fatal(
                        "Unexpected error occurred: {Url}, retry: {Retry}, error:\n{Errors}",
                        url,
                        attempt + 1,
                        ex
                    );
                }
            }

            logger.Information(
                "Wait {DelayInSeconds} second, retry: {Retry}",
                DelayInSeconds,
                attempt + 1
            );
            await Task.Delay(TimeSpan.FromSeconds(DelayInSeconds), stoppingToken);
        }

        logger.Error("All attempts to request the GraphQL endpoint failed.");
        throw new HttpRequestException("All attempts to request the GraphQL endpoint failed.");
    }

    public async Task<GetAccountDiffsResponse> GetAccountDiffsAsync(
        long baseIndex,
        long changedIndex,
        Address accountAddress,
        CancellationToken stoppingToken = default
    )
    {
        var contextualLogger = _logger.ForContext("AccountAddress", accountAddress);

        return await PostGraphQLRequestAsync<GetAccountDiffsResponse>(
            GraphQLQueries.GetAccountDiffs,
            new
            {
                baseIndex,
                changedIndex,
                accountAddress
            },
            stoppingToken,
            contextualLogger
        );
    }

    public async Task<GetTipResponse> GetTipAsync(
        CancellationToken stoppingToken = default,
        Address? accountAddress = null
    )
    {
        ILogger? contextualLogger = null;
        if (accountAddress is not null)
        {
            contextualLogger = _logger.ForContext("AccountAddress", accountAddress);
        }

        return await PostGraphQLRequestAsync<GetTipResponse>(
            GraphQLQueries.GetTip,
            null,
            stoppingToken,
            contextualLogger
        );
    }

    public async Task<GetStateResponse> GetStateAsync(
        Address accountAddress,
        Address address,
        CancellationToken stoppingToken = default
    )
    {
        return await PostGraphQLRequestAsync<GetStateResponse>(
            GraphQLQueries.GetState,
            new { accountAddress, address },
            stoppingToken
        );
    }

    public async Task<GetTransactionsResponse> GetTransactionsAsync(
        long blockIndex,
        long limit,
        CancellationToken stoppingToken = default
    )
    {
        return await PostGraphQLRequestAsync<GetTransactionsResponse>(
            GraphQLQueries.GetTransactions,
            new { blockIndex, limit },
            stoppingToken
        );
    }
}
