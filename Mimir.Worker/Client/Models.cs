namespace Mimir.Worker.Client;

public class GraphQLRequest
{
    public string query { get; set; }
    public object? variables { get; set; }
}

public class GraphQLResponse<T>
{
    public T data { get; set; }
}

public class GetAccountDiffsResponse
{
    public List<AccountDiff> accountDiffs { get; set; }
}

public class AccountDiff
{
    public string path { get; set; }
    public string baseState { get; set; }
    public string changedState { get; set; }
}

public class GetTipResponse
{
    public NodeStatus nodeStatus { get; set; }
}

public class NodeStatus
{
    public Tip tip { get; set; }
}

public class Tip
{
    public long index { get; set; }
}

public class GetStateResponse
{
    public string? state { get; set; }
}

public class GetTransactionsResponse
{
    public TransactionResponse transaction { get; set; }
}

public class TransactionResponse
{
    public List<NcTransaction> ncTransactions { get; set; }
}

public class NcTransaction
{
    public string signer { get; set; }
    public string id { get; set; }
    public string serializedPayload { get; set; }
    public List<Action> actions { get; set; }
}

public class Action
{
    public string raw { get; set; }
}
