using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.Worker.Models;
using Mimir.Worker.Services;

namespace Mimir.Worker.Handler;

public record BalanceHandler(Currency Currency) : IStateHandler
{
    public IBencodable ConvertToState(StateDiffContext context)
    {
        if (context.RawState is not Integer value)
        {
            throw new InvalidCastException(
                $"{nameof(context.RawState)} Invalid state type. Expected {nameof(Integer)}, got {context.RawState.GetType().Name}."
            );
        }

        return new BalanceState(FungibleAssetValue.FromRawValue(Currency, value));
    }
}
