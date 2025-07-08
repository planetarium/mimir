using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mimir.Worker.Client;

public class GraphQLRequest
{
    [JsonPropertyName("query")]
    public required string Query { get; set; }

    [JsonPropertyName("variables")]
    public object? Variables { get; set; }
}

public class GraphQLResponse<T>
{
    [JsonPropertyName("data")]
    public T? Data { get; set; }

    public JsonElement[]? Errors { get; set; }
}

public class GetAccountDiffsResponse
{
    [JsonPropertyName("accountDiffs")]
    public List<AccountDiff> AccountDiffs { get; set; }
}

public class GetBlocksResponse
{
    [JsonPropertyName("blockQuery")]
    public BlockQuery BlockQuery { get; set; }
}

public class BlockQuery
{
    [JsonPropertyName("blocks")]
    public List<Block> Blocks { get; set; }
}

public class Block
{
    [JsonPropertyName("index")]
    public long Index { get; set; }

    [JsonPropertyName("hash")]
    public string Hash { get; set; }

    [JsonPropertyName("miner")]
    public string Miner { get; set; }

    [JsonPropertyName("stateRootHash")]
    public string StateRootHash { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("transactions")]
    public List<BlockTransaction> Transactions { get; set; }
}

public class BlockTransaction
{
    [JsonPropertyName("actions")]
    public List<BlockAction> Actions { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("nonce")]
    public long Nonce { get; set; }

    [JsonPropertyName("publicKey")]
    public string PublicKey { get; set; }

    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    [JsonPropertyName("signer")]
    public string Signer { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; }

    [JsonPropertyName("updatedAddresses")]
    public List<string> UpdatedAddresses { get; set; }
}

public class BlockAction
{
    [JsonPropertyName("raw")]
    public string Raw { get; set; }
}

public class AccountDiff
{
    [JsonPropertyName("path")]
    public string Path { get; set; }

    [JsonPropertyName("baseState")]
    public string BaseState { get; set; }

    [JsonPropertyName("changedState")]
    public string ChangedState { get; set; }
}

public class GetTipResponse
{
    [JsonPropertyName("nodeStatus")]
    public NodeStatus NodeStatus { get; set; }
}

public class NodeStatus
{
    [JsonPropertyName("tip")]
    public Tip Tip { get; set; }
}

public class Tip
{
    [JsonPropertyName("index")]
    public long Index { get; set; }
}

public class GetStateResponse
{
    [JsonPropertyName("state")]
    public string? State { get; set; }
}

public class GetTransactionsResponse
{
    [JsonPropertyName("transaction")]
    public TransactionResponse? Transaction { get; set; }
}

public class TransactionResponse
{
    [JsonPropertyName("ncTransactions")]
    public List<NcTransaction?>? NCTransactions { get; set; }
}

public class NcTransaction
{
    [JsonPropertyName("signer")]
    public string Signer { get; set; }

    [JsonPropertyName("id")]
    public string Id { get; set; }

    [JsonPropertyName("serializedPayload")]
    public string SerializedPayload { get; set; }

    [JsonPropertyName("actions")]
    public List<Action?> Actions { get; set; }
}

public class Action
{
    [JsonPropertyName("raw")]
    public string Raw { get; set; }
}
