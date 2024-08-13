using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.Arena;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.Market;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace Mimir.Worker.Util;

public class StateGetter
{
    private readonly ILogger<StateGetter> _logger;
    private readonly IStateService _service;

    public StateGetter(IStateService service)
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StateGetter>();
        _service = service;
    }

    public async Task<T> GetSheet<T>(CancellationToken stoppingToken = default)
        where T : ISheet, new()
    {
        var sheetState = await _service.GetState(
            Addresses.TableSheet.Derive(typeof(T).Name),
            stoppingToken
        );
        if (sheetState is not Text sheetValue)
        {
            throw new InvalidCastException(nameof(T));
        }

        var sheet = new T();
        sheet.Set(sheetValue.Value);
        return sheet;
    }

    public async Task<ArenaParticipants> GetArenaParticipantsState(
        int championshipId,
        int roundId,
        CancellationToken stoppingToken = default
    )
    {
        var arenaParticipantsAddress = ArenaParticipants.DeriveAddress(championshipId, roundId);
        var state = await _service.GetState(arenaParticipantsAddress, stoppingToken);
        return state switch
        {
            List list => new ArenaParticipants(list),
            _
                => throw new StateNotFoundException(
                    arenaParticipantsAddress,
                    typeof(ArenaParticipants)
                )
        };
    }

    public async Task<Lib9c.Models.Arena.ArenaInformation> GetArenaInformationAsync(
        Address avatarAddress,
        int championshipId,
        int roundId,
        CancellationToken stoppingToken = default)
    {
        var arenaInfoAddress = ArenaInformation.DeriveAddress(
            avatarAddress,
            championshipId,
            roundId);
        var state = await _service.GetState(arenaInfoAddress, stoppingToken);
        return state switch
        {
            List list => new Lib9c.Models.Arena.ArenaInformation(list),
            _ => throw new StateNotFoundException(arenaInfoAddress, typeof(ArenaInformation))
        };
    }

    public async Task<Lib9c.Models.Arena.ArenaScore> GetArenaScoreAsync(
        Address avatarAddress,
        int championshipId,
        int roundId,
        CancellationToken stoppingToken = default)
    {
        var arenaScoreAddress = ArenaScore.DeriveAddress(avatarAddress, championshipId, roundId);
        var state = await _service.GetState(arenaScoreAddress, stoppingToken);
        return state switch
        {
            List list => new Lib9c.Models.Arena.ArenaScore(list),
            _ => throw new StateNotFoundException(arenaScoreAddress, typeof(ArenaScore))
        };
    }

    public async Task<AvatarState> GetAvatarState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await GetStateWithLegacyAccount(avatarAddress, Addresses.Avatar, stoppingToken);
        var inventory = await GetInventoryState(avatarAddress, stoppingToken);
        var avatarState = state switch
        {
            Dictionary dictionary => new AvatarState(dictionary) { inventory = inventory },
            List list => new AvatarState(list) { inventory = inventory },
            _ => throw new StateNotFoundException(avatarAddress, typeof(AvatarState))
        };

        if (
            await _service.GetState(avatarAddress, Addresses.ActionPoint, stoppingToken)
            is Integer actionPoint
        )
        {
            avatarState.actionPoint = actionPoint;
        }

        if (
            await _service.GetState(avatarAddress, Addresses.DailyReward, stoppingToken)
            is Integer dailyRewardReceivedIndex
        )
        {
            avatarState.dailyRewardReceivedIndex = dailyRewardReceivedIndex;
        }

        return avatarState;
    }

    public async Task<Inventory> GetInventoryState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var legacyInventoryAddress = avatarAddress.Derive("inventory");
        var rawState = await GetAvatarStateWithLegacyAccount(
            avatarAddress,
            Addresses.Inventory,
            legacyInventoryAddress,
            stoppingToken
        );

        if (rawState is not List list)
        {
            throw new InvalidCastException(nameof(avatarAddress));
        }

        return new Inventory(list);
    }

    public async Task<ItemSlotState> GetItemSlotState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(
            ItemSlotState.DeriveAddress(avatarAddress, BattleType.Arena), stoppingToken
        );
        return state switch
        {
            List list => new ItemSlotState(list),
            null => new ItemSlotState(BattleType.Arena),
            _
                => throw new StateNotFoundException(
                    ItemSlotState.DeriveAddress(avatarAddress, BattleType.Arena),
                    typeof(ItemSlotState)
                )
        };
    }

    public async Task<List<RuneState>> GetRuneStates(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(
            RuneSlotState.DeriveAddress(avatarAddress, BattleType.Arena),
            stoppingToken
        );
        var runeSlotState = state switch
        {
            List list => new RuneSlotState(list),
            null => new RuneSlotState(BattleType.Arena),
            _
                => throw new StateNotFoundException(
                    RuneSlotState.DeriveAddress(avatarAddress, BattleType.Arena),
                    typeof(RuneSlotState)
                )
        };

        var runes = new List<RuneState>();
        foreach (
            var runeStateAddress in runeSlotState
                .GetEquippedRuneSlotInfos()
                .Select(info => RuneState.DeriveAddress(avatarAddress, info.RuneId))
        )
        {
            if (await _service.GetState(runeStateAddress, stoppingToken) is List list)
            {
                runes.Add(new RuneState(list));
            }
        }

        return runes;
    }

    public async Task<ProductsState?> GetProductsState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(ProductsState.DeriveAddress(avatarAddress), stoppingToken);
        return state switch
        {
            List list => new ProductsState(list),
            null => null,
            _
                => throw new StateNotFoundException(
                    ProductsState.DeriveAddress(avatarAddress),
                    typeof(ProductsState)
                ),
        };
    }

    public async Task<Product> GetProductState(
        Guid productId,
        CancellationToken stoppingToken = default
    )
    {
        var productAddress = Product.DeriveAddress(productId);
        var state = await _service.GetState(productAddress, stoppingToken);

        return state switch
        {
            List list => ProductFactory.DeserializeProduct(list),
            _ => throw new StateNotFoundException(productAddress, typeof(Product))
        };
    }

    public async Task<MarketState> GetMarketState(CancellationToken stoppingToken = default)
    {
        var state = await _service.GetState(Addresses.Market, stoppingToken);
        return state switch
        {
            List list => new MarketState(list),
            _ => throw new StateNotFoundException(Addresses.Market, typeof(MarketState))
        };
    }

    public async Task<WorldBossState> GetWorldBossState(
        Address worldBossAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(worldBossAddress, stoppingToken);
        return state switch
        {
            List list => new WorldBossState(list),
            _ => throw new StateNotFoundException(worldBossAddress, typeof(WorldBossState))
        };
    }

    public async Task<RaiderState> GetRaiderState(
        Address raiderAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(raiderAddress, stoppingToken);
        return state switch
        {
            List list => new RaiderState(list),
            _ => throw new StateNotFoundException(raiderAddress, typeof(RaiderState))
        };
    }

    public async Task<WorldBossKillRewardRecord> GetWorldBossKillRewardRecordState(
        Address worldBossKillRewardRecordAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(worldBossKillRewardRecordAddress, stoppingToken);
        return state switch
        {
            List list => new WorldBossKillRewardRecord(list),
            _
                => throw new StateNotFoundException(
                    worldBossKillRewardRecordAddress,
                    typeof(WorldBossKillRewardRecord)
                )
        };
    }

    public async Task<IValue?> GetStateWithLegacyAccount(
        Address address,
        Address accountAddress,
        CancellationToken stoppingToken = default
    ) => await _service.GetState(address, accountAddress, stoppingToken) ??
         await _service.GetState(address, stoppingToken);

    public async Task<CombinationSlotState> GetCombinationSlotState(
        Address slotAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(slotAddress, stoppingToken);

        return state switch
        {
            Dictionary dict => new CombinationSlotState(dict),
            _ => throw new StateNotFoundException(slotAddress, typeof(CombinationSlotState))
        };
    }

    public async Task<PetState> GetPetState(
        Address petStateAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(petStateAddress, stoppingToken);

        return state switch
        {
            List list => new PetState(list),
            _ => throw new StateNotFoundException(petStateAddress, typeof(CombinationSlotState))
        };
    }

    public async Task<IValue?> GetAvatarStateWithLegacyAccount(
        Address avatarAddress,
        Address accountAddress,
        Address legacyAddress,
        CancellationToken stoppingToken = default
    ) =>
        await _service.GetState(avatarAddress, accountAddress, stoppingToken)
        ?? await _service.GetState(legacyAddress, stoppingToken);

    public async Task<ArenaSheet.RoundData> GetArenaRoundData(
        long index,
        CancellationToken stoppingToken = default
    )
    {
        var arenaSheet = await GetSheet<ArenaSheet>(stoppingToken);
        return arenaSheet.GetRoundByBlockIndex(index);
    }
}
