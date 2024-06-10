using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.Arena;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.Market;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Scrapper;

public class StateGetter
{
    private readonly ILogger<StateGetter> _logger;
    private readonly IStateService _service;

    public StateGetter(IStateService service)
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StateGetter>();
        _service = service;
    }

    public async Task<T> GetSheet<T>()
        where T : ISheet, new()
    {
        var sheetState = await _service.GetState(Addresses.TableSheet.Derive(typeof(T).Name));
        if (sheetState is not Text sheetValue)
        {
            throw new ArgumentException(nameof(T));
        }

        var sheet = new T();
        sheet.Set(sheetValue.Value);
        return sheet;
    }

    public async Task<ArenaParticipants> GetArenaParticipantsState(int championshipId, int roundId)
    {
        var arenaParticipantsAddress = ArenaParticipants.DeriveAddress(championshipId, roundId);
        var state = await _service.GetState(arenaParticipantsAddress);
        return state switch
        {
            List list => new ArenaParticipants(list),
            _ => throw new ArgumentException(nameof(arenaParticipantsAddress))
        };
    }

    public async Task<ArenaScore> GetArenaScoreState(
        Address avatarAddress,
        int championshipId,
        int roundId
    )
    {
        var arenaScoreAddress = ArenaScore.DeriveAddress(avatarAddress, championshipId, roundId);
        var state = await _service.GetState(arenaScoreAddress);
        return state switch
        {
            List list => new ArenaScore(list),
            _ => throw new ArgumentException(nameof(arenaScoreAddress))
        };
    }

    public async Task<ArenaInformation> GetArenaInfoState(
        Address avatarAddress,
        int championshipId,
        int roundId
    )
    {
        var arenaInfoAddress = ArenaInformation.DeriveAddress(
            avatarAddress,
            championshipId,
            roundId
        );
        var state = await _service.GetState(arenaInfoAddress);
        return state switch
        {
            List list => new ArenaInformation(list),
            _ => throw new ArgumentException(nameof(arenaInfoAddress))
        };
    }

    public async Task<AvatarState> GetAvatarState(Address avatarAddress)
    {
        var state = await GetStateWithLegacyAccount(avatarAddress, Addresses.Avatar);
        var inventory = await GetInventoryState(avatarAddress);
        var avatarState = state switch
        {
            Dictionary dictionary => new AvatarState(dictionary) { inventory = inventory },
            List list => new AvatarState(list) { inventory = inventory },
            _ => throw new ArgumentException($"Unsupported state type for address: {avatarAddress}")
        };

        if (await _service.GetState(avatarAddress, Addresses.ActionPoint) is Integer actionPoint)
        {
            avatarState.actionPoint = actionPoint;
        }

        if (
            await _service.GetState(avatarAddress, Addresses.DailyReward)
            is Integer dailyRewardReceivedIndex
        )
        {
            avatarState.dailyRewardReceivedIndex = dailyRewardReceivedIndex;
        }

        return avatarState;
    }

    public async Task<Inventory> GetInventoryState(Address avatarAddress)
    {
        var legacyInventoryAddress = avatarAddress.Derive("inventory");
        var rawState = await GetAvatarStateWithLegacyAccount(
            avatarAddress,
            Addresses.Inventory,
            legacyInventoryAddress
        );

        if (rawState is not List list)
        {
            throw new ArgumentException(nameof(avatarAddress));
        }

        return new Inventory(list);
    }

    public async Task<ItemSlotState> GetItemSlotState(Address avatarAddress)
    {
        var state = await _service.GetState(
            ItemSlotState.DeriveAddress(avatarAddress, BattleType.Arena)
        );
        return state switch
        {
            List list => new ItemSlotState(list),
            null => new ItemSlotState(BattleType.Arena),
            _ => throw new ArgumentException(nameof(avatarAddress))
        };
    }

    public async Task<List<RuneState>> GetRuneStates(Address avatarAddress)
    {
        var state = await _service.GetState(
            RuneSlotState.DeriveAddress(avatarAddress, BattleType.Arena)
        );
        var runeSlotState = state switch
        {
            List list => new RuneSlotState(list),
            null => new RuneSlotState(BattleType.Arena),
            _ => throw new ArgumentException(nameof(avatarAddress))
        };

        var runes = new List<RuneState>();
        foreach (
            var runeStateAddress in runeSlotState
                .GetEquippedRuneSlotInfos()
                .Select(info => RuneState.DeriveAddress(avatarAddress, info.RuneId))
        )
        {
            if (await _service.GetState(runeStateAddress) is List list)
            {
                runes.Add(new RuneState(list));
            }
        }

        return runes;
    }

    public async Task<ProductsState?> GetProductsState(Address avatarAddress)
    {
        var state = await _service.GetState(ProductsState.DeriveAddress(avatarAddress));
        return state switch
        {
            List list => new ProductsState(list),
            _ => throw new ArgumentException(nameof(avatarAddress))
        };
    }

    public async Task<Product?> GetProductState(Guid productId)
    {
        var productAddress = Product.DeriveAddress(productId);
        var rawState = await _service.GetState(productAddress);

        if (rawState is not List list)
        {
            throw new ArgumentException(nameof(productId));
        }

        var product = ProductFactory.DeserializeProduct(list);
        return product;
    }

    public async Task<IValue?> GetStateWithLegacyAccount(Address address, Address accountAddress)
    {
        var state = await _service.GetState(address, accountAddress);

        if (state == null)
        {
            state = await _service.GetState(address);
        }
        return state;
    }

    public async Task<IValue?> GetAvatarStateWithLegacyAccount(
        Address avatarAddress,
        Address accountAddress,
        Address legacyAddress
    )
    {
        var state = await _service.GetState(avatarAddress, accountAddress);

        if (state == null)
        {
            state = await _service.GetState(legacyAddress);
        }
        return state;
    }

    public async Task<ArenaSheet.RoundData> GetArenaRoundData(long index)
    {
        var arenaSheet = await GetSheet<ArenaSheet>();
        var roundData = arenaSheet.GetRoundByBlockIndex(index);

        return roundData;
    }
}
