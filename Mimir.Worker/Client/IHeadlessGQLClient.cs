namespace Mimir.Worker.Client;

public interface IHeadlessGQLClient
{
    Task<GetAccountDiffsResponse> GetAccountDiffsAsync(
        long baseIndex,
        long changedIndex,
        string accountAddress
    );
    Task<GetAccountDiffsResponse> GetAccountDiffsAsync(
        long baseIndex,
        long changedIndex,
        string accountAddress,
        CancellationToken stoppingToken
    );
    Task<GetTipResponse> GetTipAsync();
    Task<GetTipResponse> GetTipAsync(CancellationToken stoppingToken);
    Task<GetStateResponse> GetStateAsync(string accountAddress, string address);
    Task<GetStateResponse> GetStateAsync(
        string accountAddress,
        string address,
        CancellationToken stoppingToken
    );
    Task<GetTransactionsResponse> GetTransactionsAsync(long blockIndex, long limit);
    Task<GetTransactionsResponse> GetTransactionsAsync(
        long blockIndex,
        long limit,
        CancellationToken stoppingToken
    );
}
