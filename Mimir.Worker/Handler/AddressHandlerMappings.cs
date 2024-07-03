using Libplanet.Crypto;
using Mimir.Worker.Models;
using Nekoyume;

namespace Mimir.Worker.Handler;

public static class AddressHandlerMappings
{
    public static readonly Dictionary<Address, IStateHandler<StateData>> HandlerMappings = new();

    public static readonly string NCGCurrencyAddress = "0xd6663d0EEC2a7c59FF3cfe3089abF8FEd383227D";

    static AddressHandlerMappings()
    {
        HandlerMappings.Add(Addresses.Agent, new AgentStateHandler());
        HandlerMappings.Add(Addresses.Avatar, new AvatarStateHandler());
        HandlerMappings.Add(Addresses.Inventory, new InventoryStateHandler());
        HandlerMappings.Add(Addresses.WorldInformation, new WorldInformationStateHandler());
        HandlerMappings.Add(Addresses.ActionPoint, new ActionPointStateHandler());
        HandlerMappings.Add(Addresses.QuestList, new QuestListStateHandler());
        HandlerMappings.Add(Addresses.RuneState, new AllRuneStateHandler());
        HandlerMappings.Add(Addresses.Collection, new CollectionStateHandler());
        HandlerMappings.Add(Addresses.DailyReward, new DailyRewardStateHandler());
        HandlerMappings.Add(
            new Address(NCGCurrencyAddress),
            new GoldBalanceHandler()
        );
    }
}
