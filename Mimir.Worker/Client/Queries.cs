namespace Mimir.Worker.Client;

public static class GraphQLQueries
{
    public const string GetBlocks =
        @"
            query getBlock($offset: Int!, $limit: Int!) {
                blockQuery {
                    blocks(offset: $offset, limit: $limit) {
                    index
                    hash
                    miner
                    stateRootHash
                    timestamp
                    transactions {
                        actions {
                            raw
                        }
                        id
                        nonce
                        publicKey
                        signature
                        signer
                        timestamp
                        updatedAddresses
                    }
                    }
                }
            }";

    public const string GetAccountDiffs =
        @"
            query GetAccountDiffs($baseIndex: Long!, $changedIndex: Long!, $accountAddress: Address!) {
                accountDiffs(baseIndex: $baseIndex, changedIndex: $changedIndex, accountAddress: $accountAddress) {
                    path
                    baseState
                    changedState
                }
            }";

    public const string GetTransactionStatus =
        @"
            query GetTransactionStatus($txId: TxId!) {
                transaction {
                        transactionResult(txId: $txId) {
                            txStatus
                            exceptionNames
                    }
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
            query GetTransactions($blockIndex: Long!) {
                transaction {
                    ncTransactions(startingBlockIndex: $blockIndex, limit: 1, actionType: ""^.*$"", txStatusFilter: [SUCCESS]) {
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
