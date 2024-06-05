using Libplanet.Action.State;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Nekoyume;

namespace Mimir.Worker.Handler;

public static class AddressHandlerMappings
{
    public static readonly Dictionary<Address, IStateHandler<StateData>> HandlerMappings = new();

    static AddressHandlerMappings()
    {
        HandlerMappings.Add(ReservedAddresses.LegacyAccount, new LegacyAccountHandler());
        HandlerMappings.Add(Addresses.Avatar, new AvatarStateHandler());
        HandlerMappings.Add(Addresses.Inventory, new InventoryStateHandler());
        HandlerMappings.Add(Addresses.WorldInformation, new WorldInformationStateHandler());
        HandlerMappings.Add(Addresses.ActionPoint, new ActionPointStateHandler());
        HandlerMappings.Add(Addresses.QuestList, new QuestListStateHandler());
        HandlerMappings.Add(Addresses.RuneState, new AllRuneStateHandler());
    }
}
