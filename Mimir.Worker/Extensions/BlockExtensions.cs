using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Block;
using Libplanet.Crypto;
using Mimir.Worker.Client;
using Mimir.Worker.Util;
using MongoDB.Bson;

namespace Mimir.Worker.Extensions;

public static class BlockExtensions
{
    private static readonly Codec Codec = new();

    public static Lib9c.Models.Block.Block ToBlockModel(this Client.Block apiBlock)
    {
        return new Lib9c.Models.Block.Block
        {
            Index = apiBlock.Index,
            Hash = apiBlock.Hash,
            Miner = new Address(apiBlock.Miner),
            StateRootHash = apiBlock.StateRootHash,
            Timestamp = apiBlock.Timestamp,
            TxCount = apiBlock.Transactions?.Count ?? 0,
            TxIds = apiBlock.Transactions?.Select(tx => tx.Id).ToList() ?? new List<string>(),
        };
    }

    public static Lib9c.Models.Block.Transaction ToTransactionModel(
        this BlockTransaction apiTransaction,
        Client.Block apiBlock
    )
    {
        return new Lib9c.Models.Block.Transaction
        {
            Actions =
                apiTransaction.Actions?.Select(action => action.ToActionModel()).ToList()
                ?? new List<Lib9c.Models.Block.Action>(),
            Id = apiTransaction.Id,
            Nonce = apiTransaction.Nonce,
            PublicKey = apiTransaction.PublicKey,
            Signature = apiTransaction.Signature,
            Signer = new Address(apiTransaction.Signer),
            Timestamp = apiTransaction.Timestamp,
            BlockTimestamp = apiBlock.Timestamp,
            UpdatedAddresses =
                apiTransaction.UpdatedAddresses?.Select(addr => new Address(addr)).ToList()
                ?? new List<Address>(),
        };
    }

    public static Lib9c.Models.Block.Action ToActionModel(this BlockAction apiAction)
    {
        var (typeId, values, _) = ActionParser.ParseAction(apiAction.Raw);

        return new Lib9c.Models.Block.Action
        {
            Raw = apiAction.Raw,
            TypeId = typeId,
            Values = values,
        };
    }

    public static List<Lib9c.Models.Block.Block> ToBlockModels(this GetBlocksResponse apiResponse)
    {
        return apiResponse.BlockQuery?.Blocks?.Select(block => block.ToBlockModel()).ToList()
            ?? new List<Lib9c.Models.Block.Block>();
    }
}
