using Mimir.Worker.Client;

namespace Mimir.Worker.ActionHandler;

public interface IActionHandler
{
    public Task HandleTransactionsAsync(
        long blockIndex,
        TransactionResponse transactionResponse,
        CancellationToken cancellationToken);
}
