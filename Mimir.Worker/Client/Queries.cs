namespace Mimir.Worker.Client;

public static class GraphQLQueries
{
    public const string GetAccountDiffs =
        @"
            query GetAccountDiffs($baseIndex: Long!, $changedIndex: Long!, $accountAddress: Address!) {
                accountDiffs(baseIndex: $baseIndex, changedIndex: $changedIndex, accountAddress: $accountAddress) {
                    path
                    baseState
                    changedState
                }
            }";

    public const string GetTip =
        @"
            query GetTip {
                nodeStatus {
                    tip {
                        index
                    }
                }
            }";

    public const string GetState =
        @"
            query GetState($accountAddress: Address!, $address: Address!) {
                state(accountAddress: $accountAddress, address: $address)
            }";

    public const string GetTransactions =
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
}
