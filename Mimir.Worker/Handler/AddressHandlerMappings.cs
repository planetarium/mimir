using Libplanet.Action.State;
using Libplanet.Crypto;
using Mimir.Worker.Constants;
using Mimir.Worker.Models;
using Nekoyume;

namespace Mimir.Worker.Handler;

public static class AddressHandlerMappings
{
    public static Dictionary<Address, IStateHandler<StateData>> HandlerMappings =
        new Dictionary<Address, IStateHandler<StateData>>();

    static AddressHandlerMappings()
    {
        InitializeHandlers();

        HandlerMappings[Addresses.Avatar] = new AvatarStateHandler();
        HandlerMappings[Addresses.Inventory] = new InventoryStateHandler();
        HandlerMappings[Addresses.WorldInformation] = new WorldInformationStateHandler();
        HandlerMappings[Addresses.QuestList] = new QuestListStateHandler();
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
