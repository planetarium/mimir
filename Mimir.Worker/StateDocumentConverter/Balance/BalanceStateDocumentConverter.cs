using Bencodex.Types;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter.Balance;

public record BalanceStateDocumentConverter(Currency Currency) : IStateDocumentConverter
{
    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        if (context.RawState is not Integer value)
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. " +
                $"Expected {nameof(Integer)}, got {context.RawState.GetType().Name}.");

        return new BalanceDocument(
            context.BlockIndex,
            context.Address,
            FungibleAssetValue.FromRawValue(Currency, value).GetQuantityString());
    }
}