using Bencodex.Types;
using Libplanet.Types.Assets;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public record BalanceHandler(Currency Currency) : IStateHandler
{
    public MimirBsonDocument ConvertToDocument(StateDiffContext context)
    {
        if (context.RawState is not Integer value)
        {
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. " +
                $"Expected {nameof(Integer)}, got {context.RawState.GetType().Name}.");
        }

        return new BalanceDocument(
            context.Address,
            FungibleAssetValue.FromRawValue(Currency, value).GetQuantityString());
    }
}
