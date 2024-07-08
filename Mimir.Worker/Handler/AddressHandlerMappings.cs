using Lib9c;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
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
#pragma warning disable CS0618 // Type or member is obsolete
            new BalanceHandler(Currency.Legacy("NCG", 2, new Address("0x47d082a115c63e7b58b1532d20e631538eafadde")))
#pragma warning restore CS0618 // Type or member is obsolete
        );
    }
}
