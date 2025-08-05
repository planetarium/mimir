using Libplanet.Crypto;
using Libplanet.Types.Assets;

namespace Mimir.Shared.Constants;

public static class NCGCurrency
{
    public static readonly Currency OdinNCGCurrency = Currency.Legacy(
        "NCG",
        2,
        new Address("0x47d082a115c63e7b58b1532d20e631538eafadde")
    );

    public static readonly Currency HeimdallNCGCurrency = Currency.Legacy(
        "NCG",
        2,
        null
    );
} 