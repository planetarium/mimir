using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using BitFaster.Caching;
using BitFaster.Caching.Lru;
using Libplanet.Crypto;
using Libplanet.Types.Tx;
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

    private readonly IAsyncCache<long, GetTransactionsResponse> _transactionCache =
        new ConcurrentLruBuilder<long, GetTransactionsResponse>()
            .AsAsyncCache()
            .WithExpireAfterWrite(TimeSpan.FromSeconds(30))
            .WithCapacity(10)
            .Build();
    private const int RetryAttempts = 3;
    private const int DelayInSeconds = 5;

    public HeadlessGQLClient(HttpClient httpClient, Uri[] urls, string? issuer, string? secret)
    {
        _httpClient = httpClient;
        _urls = urls;
        _issuer = issuer;
        _secret = secret;
        _logger = Log.ForContext<HeadlessGQLClient>();
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
    }

    public HeadlessGQLClient(Uri[] urls, string? issuer, string? secret)
        : this(new HttpClient(), urls, issuer, secret)
    {
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

    private async Task<(T response, string jsonResponse)> PostGraphQLRequestAsync<T>(
        string query,
        object? variables,
        CancellationToken stoppingToken = default,
        ILogger? contextualLogger = null,
        bool useExplorer = false
    )
    {
        var logger = contextualLogger is null ? _logger : contextualLogger;

        for (int attempt = 0; attempt < RetryAttempts; attempt++)
        {
            foreach (var _url in _urls)
            {
                Uri url = _url;
                if (useExplorer)
                {
                    url = new Uri(_url, "graphql/explorer");
                }

                try
                {
                    logger.Debug(
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
                        Content = content,
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

                    if (
                        graphQLResponse is null
                        || graphQLResponse.Data is null
                        || graphQLResponse.Errors is not null
                    )
                    {
                        throw new HttpRequestException("Response data is null.");
                    }

                    logger.Debug(
                        "Successfully received the data: url: {Url}, query: {Query}, retry:{Retry}",
                        url,
                        query,
                        attempt + 1
                    );

                    return (graphQLResponse.Data, jsonResponse);
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
                    logger.Debug(
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

    public async Task<(GetAccountDiffsResponse response, string jsonResponse)> GetAccountDiffsAsync(
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
                accountAddress,
            },
            stoppingToken,
            contextualLogger
        );
    }

    public async Task<(GetTipResponse response, string jsonResponse)> GetTipAsync(
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

    public async Task<(GetStateResponse response, string jsonResponse)> GetStateAsync(
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
        CancellationToken stoppingToken = default
    )
    {
        return await _transactionCache.GetOrAddAsync(
            blockIndex,
            async index =>
            {
                var (response, jsonResponse) =
                    await PostGraphQLRequestAsync<GetTransactionsResponse>(
                        GraphQLQueries.GetTransactions,
                        new { blockIndex = index },
                        stoppingToken
                    );

                // Validate `GetTransactionsResponse` is valid.
                if (
                    response.Transaction?.NCTransactions is null
                    || response.Transaction.NCTransactions.Any(t =>
                        t is null || t.Actions.Any(a => a is null)
                    )
                )
                {
                    throw new InvalidOperationException(
                        "Invalid transactions response."
                            + $" blockIndex: {index}"
                            + $" response: {jsonResponse}"
                    );
                }

                return response;
            }
        );
    }

    public async Task<(GetBlocksResponse response, string jsonResponse)> GetBlocksAsync(
        int offset,
        int limit,
        CancellationToken stoppingToken
    )
    {
        return await PostGraphQLRequestAsync<GetBlocksResponse>(
            GraphQLQueries.GetBlocks,
            new { offset, limit },
            stoppingToken,
            null,
            true
        );
    }

    public async Task<(GetTransactionStatusResponse response, string jsonResponse)> GetTransactionStatusAsync(
        TxId txid,
        CancellationToken stoppingToken = default
    )
    {
        var contextualLogger = _logger.ForContext("TxId", txid);

        return await PostGraphQLRequestAsync<GetTransactionStatusResponse>(
            GraphQLQueries.GetTransactionStatus,
            new { txId = txid },
            stoppingToken,
            contextualLogger
        );
    }
}
