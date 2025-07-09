using Libplanet.Crypto;
using Libplanet.Types.Tx;

namespace Mimir.Worker.Client;

public interface IHeadlessGQLClient
{
    Task<(GetAccountDiffsResponse response, string jsonResponse)> GetAccountDiffsAsync(
        long baseIndex,
        long changedIndex,
        Address accountAddress,
        CancellationToken stoppingToken
    );
    Task<(GetBlocksResponse response, string jsonResponse)> GetBlocksAsync(
        int offset,
        int limit,
        CancellationToken stoppingToken);
    Task<(GetTipResponse response, string jsonResponse)> GetTipAsync(
        CancellationToken stoppingToken,
        Address? accountAddress);
    Task<(GetStateResponse response, string jsonResponse)> GetStateAsync(
        Address accountAddress,
        Address address,
        CancellationToken stoppingToken
    );
    Task<(GetTransactionStatusResponse response, string jsonResponse)> GetTransactionStatusAsync(
        TxId txid,
        CancellationToken stoppingToken
    );
    Task<GetTransactionsResponse> GetTransactionsAsync(
        long blockIndex,
        CancellationToken stoppingToken
    );
}
