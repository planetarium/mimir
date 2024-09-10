using Lib9c;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.Worker.Constants;
using Nekoyume;

namespace Mimir.Worker.Handler;

public static class AddressHandlerMappings
{
    public static readonly Dictionary<Address, IStateHandler> HandlerMappings = new();

    public static readonly Currency OdinNCGCurrency = Currency.Legacy(
        "NCG",
        2,
        new Address("0x47d082a115c63e7b58b1532d20e631538eafadde")
    );
    public static readonly Currency HeimdallNCGCurrency = Currency.Legacy(
        "NCG",
        2,
        new Address("0x47d082a115c63e7b58b1532d20e631538eafadde")
    );

    static AddressHandlerMappings()
    {
        HandlerMappings.Add(Addresses.Agent, new AgentStateHandler());
        HandlerMappings.Add(Addresses.Avatar, new AvatarStateHandler());
        HandlerMappings.Add(Addresses.ActionPoint, new ActionPointStateHandler());
        HandlerMappings.Add(Addresses.DailyReward, new DailyRewardStateHandler());
        HandlerMappings.Add(Addresses.Inventory, new InventoryStateHandler());
        // HandlerMappings.Add(Addresses.WorldInformation, new WorldInformationStateHandler());
        // HandlerMappings.Add(Addresses.QuestList, new QuestListStateHandler());
        HandlerMappings.Add(Addresses.RuneState, new AllRuneStateHandler());
        HandlerMappings.Add(Addresses.Collection, new CollectionStateHandler());
        // // HandlerMappings.Add(Addresses.AdventureBoss, new SeasonInfoHandler());
        // // HandlerMappings.Add(Addresses.BountyBoard, new BountyBoardHandler());
        // // HandlerMappings.Add(Addresses.ExploreBoard, new ExploreBoardHandler());
        // // HandlerMappings.Add(Addresses.ExplorerList, new ExplorerListHandler());

        RegisterBalanceHandler(Currencies.Crystal);
        RegisterBalanceHandler(Currencies.StakeRune);
        RegisterBalanceHandler(Currencies.DailyRewardRune);
        RegisterBalanceHandler(Currencies.Garage);
        RegisterBalanceHandler(Currencies.Mead);
        RegisterBalanceHandler(Currencies.FreyaBlessingRune);
        RegisterBalanceHandler(Currencies.FreyaLiberationRune);
    }

    public static void RegisterCurrencyHandler(PlanetType planet)
    {
        switch (planet.Value)
        {
            case var _ when planet == PlanetType.ODIN:
                RegisterBalanceHandler(OdinNCGCurrency);
                RegisterBalanceHandler(Currencies.OdinWeaknessRune);
                RegisterBalanceHandler(Currencies.OdinWisdomRune);
                break;
            case var _ when planet == PlanetType.HEIMDALL:
                RegisterBalanceHandler(HeimdallNCGCurrency);
                break;
            default:
                throw new ArgumentException("Invalid planet");
        }
    }

    private static void RegisterBalanceHandler(Currency currency)
    {
        HandlerMappings.Add(new Address(currency.Hash.ToByteArray()),
#pragma warning disable CS0618 // Type or member is obsolete
            new BalanceHandler(currency)
#pragma warning restore CS0618 // Type or member is obsolete
        );
    }
}
