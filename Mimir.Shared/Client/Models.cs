using Lib9c.Models.Block;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Mimir.Shared.Client;

public class GraphQLRequest
{
    [JsonProperty("query")]
    public required string Query { get; set; }

    [JsonProperty("variables")]
    public object? Variables { get; set; }
}

public class GraphQLResponse<T>
{
    [JsonProperty("data")]
    public T? Data { get; set; }

    [JsonProperty("errors")]
    public object[]? Errors { get; set; }
}

public class GetAccountDiffsResponse
{
    [JsonProperty("accountDiffs")]
    public List<AccountDiff> AccountDiffs { get; set; }
}

public class GetBlocksResponse
{
    [JsonProperty("blockQuery")]
    public BlockQuery BlockQuery { get; set; }
}

public class GetGoldBalanceResponse
{
    [JsonProperty("goldBalance")]
    public string GoldBalance { get; set; }
}

public class BlockQuery
{
    [JsonProperty("blocks")]
    public List<Block> Blocks { get; set; }
}

public class Block
{
    [JsonProperty("index")]
    public long Index { get; set; }

    [JsonProperty("hash")]
    public string Hash { get; set; }

    [JsonProperty("miner")]
    public string Miner { get; set; }

    [JsonProperty("stateRootHash")]
    public string StateRootHash { get; set; }

    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    [JsonProperty("transactions")]
    public List<BlockTransaction> Transactions { get; set; }
}

public class BlockTransaction
{
    [JsonProperty("actions")]
    public List<BlockAction> Actions { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("nonce")]
    public long Nonce { get; set; }

    [JsonProperty("publicKey")]
    public string PublicKey { get; set; }

    [JsonProperty("signature")]
    public string Signature { get; set; }

    [JsonProperty("signer")]
    public string Signer { get; set; }

    [JsonProperty("timestamp")]
    public string Timestamp { get; set; }

    [JsonProperty("updatedAddresses")]
    public List<string> UpdatedAddresses { get; set; }
}

public class BlockAction
{
    [JsonProperty("raw")]
    public string Raw { get; set; }
}

public class AccountDiff
{
    [JsonProperty("path")]
    public string Path { get; set; }

    [JsonProperty("baseState")]
    public string BaseState { get; set; }

    [JsonProperty("changedState")]
    public string ChangedState { get; set; }
}

public class GetTipResponse
{
    [JsonProperty("nodeStatus")]
    public NodeStatus NodeStatus { get; set; }
}

public class NodeStatus
{
    [JsonProperty("tip")]
    public Tip Tip { get; set; }
}

public class Tip
{
    [JsonProperty("index")]
    public long Index { get; set; }
}

public class GetStateResponse
{
    [JsonProperty("state")]
    public string? State { get; set; }
}

public class GetTransactionsResponse
{
    [JsonProperty("transaction")]
    public TransactionResponse? Transaction { get; set; }
}

public class TransactionResponse
{
    [JsonProperty("ncTransactions")]
    public List<NcTransaction?>? NCTransactions { get; set; }
}

public class NcTransaction
{
    [JsonProperty("signer")]
    public string Signer { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("serializedPayload")]
    public string SerializedPayload { get; set; }

    [JsonProperty("actions")]
    public List<Action?> Actions { get; set; }
}

public class Action
{
    [JsonProperty("raw")]
    public string Raw { get; set; }
}

public class GetTransactionStatusesResponse
{
    [JsonProperty("transaction")]
    public TransactionStatusResponse? Transaction { get; set; }
}

public class TransactionStatusResponse
{
    [JsonProperty("transactionResults")]
    public List<TransactionResult> TransactionResults { get; set; }
}

public class TransactionResult
{
    [JsonProperty("txStatus")]
    public TxStatus TxStatus { get; set; }

    [JsonProperty("exceptionNames")]
    public List<string?> ExceptionNames { get; set; }
}
