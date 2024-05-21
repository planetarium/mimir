using Libplanet.Action.State;
using Libplanet.Crypto;
using Mimir.Worker.Constants;
using Nekoyume;
using Nekoyume.Model.State;

namespace Mimir.Worker.Handler;

public static class AddressHandlerMappings
{
    public static Dictionary<Address, IStateHandler<AvatarState>> HandlerMappings =
        new Dictionary<Address, IStateHandler<AvatarState>>();

    static AddressHandlerMappings()
    {
        InitializeHandlers();

        HandlerMappings[Addresses.Avatar] = new AvatarStateHandler();
    }

    private static void InitializeHandlers()
    {
        foreach (var address in CollectionNames.CollectionMappings.Keys)
        {
            HandlerMappings.Add(address, null);
        }
        HandlerMappings.Add(ReservedAddresses.LegacyAccount, null);
    }
}
