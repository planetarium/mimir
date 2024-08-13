using Libplanet.Crypto;

namespace Mimir.Worker.Client;

public interface IHeadlessGQLClient
{
    Task<GetAccountDiffsResponse> GetAccountDiffsAsync(
        long baseIndex,
        long changedIndex,
        Address accountAddress,
        CancellationToken stoppingToken
    );
    Task<GetTipResponse> GetTipAsync(CancellationToken stoppingToken, Address? accountAddress);
    Task<GetStateResponse> GetStateAsync(
        Address accountAddress,
        Address address,
        CancellationToken stoppingToken
    );
    Task<GetTransactionsResponse> GetTransactionsAsync(
        long blockIndex,
        long limit,
        CancellationToken stoppingToken
    );
}
