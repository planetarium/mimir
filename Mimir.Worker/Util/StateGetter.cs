using Bencodex.Types;
using Lib9c.Models.Arena;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.TableData;

namespace Mimir.Worker.Util;

public class StateGetter
{
    private readonly IStateService _service;

    public StateGetter(IStateService service)
    {
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

    public async Task<Nekoyume.Model.Arena.ArenaParticipants> GetArenaParticipantsState(
        int championshipId,
        int roundId,
        CancellationToken stoppingToken = default
    )
    {
        var arenaParticipantsAddress = Nekoyume.Model.Arena.ArenaParticipants.DeriveAddress(
            championshipId,
            roundId
        );
        var state = await _service.GetState(arenaParticipantsAddress, stoppingToken);
        return state switch
        {
            List list => new Nekoyume.Model.Arena.ArenaParticipants(list),
            _
                => throw new StateNotFoundException(
                    arenaParticipantsAddress,
                    typeof(Nekoyume.Model.Arena.ArenaParticipants)
                )
        };
    }

    public async Task<ArenaInformation> GetArenaInformationAsync(
        Address avatarAddress,
        int championshipId,
        int roundId,
        CancellationToken stoppingToken = default
    )
    {
        var arenaInfoAddress = Nekoyume.Model.Arena.ArenaInformation.DeriveAddress(
            avatarAddress,
            championshipId,
            roundId
        );
        var state = await _service.GetState(arenaInfoAddress, stoppingToken);
        return state switch
        {
            List list => new ArenaInformation(list),
            _ => throw new StateNotFoundException(arenaInfoAddress, typeof(ArenaInformation))
        };
    }

    public async Task<ArenaScore> GetArenaScoreAsync(
        Address avatarAddress,
        int championshipId,
        int roundId,
        CancellationToken stoppingToken = default
    )
    {
        var arenaScoreAddress = Nekoyume.Model.Arena.ArenaScore.DeriveAddress(
            avatarAddress,
            championshipId,
            roundId
        );
        var state = await _service.GetState(arenaScoreAddress, stoppingToken);
        return state switch
        {
            List list => new ArenaScore(list),
            _ => throw new StateNotFoundException(arenaScoreAddress, typeof(ArenaScore))
        };
    }

    public async Task<Lib9c.Models.States.AvatarState> GetAvatarState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await GetStateWithLegacyAccount(avatarAddress, Addresses.Avatar, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(
                avatarAddress,
                typeof(Lib9c.Models.States.AvatarState)
            );
        }

        var avatarState = new Lib9c.Models.States.AvatarState(state);

        return avatarState;
    }

    public async Task<Inventory> GetInventoryState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var legacyInventoryAddress = avatarAddress.Derive("inventory");
        var state = await GetAvatarStateWithLegacyAccount(
            avatarAddress,
            Addresses.Inventory,
            legacyInventoryAddress,
            stoppingToken
        );

        if (state is null)
        {
            throw new StateNotFoundException(legacyInventoryAddress, typeof(Inventory));
        }

        return new Inventory(state);
    }

    public async Task<ItemSlotState> GetItemSlotState(
        Address avatarAddress,
        BattleType battleType,
        CancellationToken stoppingToken = default
    )
    {
        var itemSlotAddress = Nekoyume.Model.State.ItemSlotState.DeriveAddress(
            avatarAddress,
            battleType
        );
        var state = await _service.GetState(itemSlotAddress, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(itemSlotAddress, typeof(ItemSlotState));
        }

        return new ItemSlotState(state);
    }

    public async Task<List<RuneState>> GetRuneStates(
        Address avatarAddress,
        BattleType battleType,
        CancellationToken stoppingToken = default
    )
    {
        var runeSlotAddress = Nekoyume.Model.State.RuneSlotState.DeriveAddress(
            avatarAddress,
            battleType
        );
        var state = await _service.GetState(runeSlotAddress, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(runeSlotAddress, typeof(RuneState));
        }

        var runeSlotState = new RuneSlotState(state);

        var runes = new List<RuneState>();
        foreach (
            var runeStateAddress in runeSlotState
                .Slots.Where(slot => !slot.IsLock && slot.RuneId.HasValue)
                .Select(slot =>
                    Nekoyume.Model.State.RuneState.DeriveAddress(avatarAddress, slot.RuneId.Value)
                )
        )
        {
            if (await _service.GetState(runeStateAddress, stoppingToken) is List list)
            {
                runes.Add(new RuneState(list));
            }
        }

        return runes;
    }

    public async Task<ProductsState> GetProductsState(
        Address avatarAddress,
        CancellationToken stoppingToken = default)
    {
        var productAddress = Nekoyume.Model.Market.ProductsState.DeriveAddress(avatarAddress);
        var state = await _service.GetState(productAddress, stoppingToken);
        if (state is null)
        {
            throw new StateNotFoundException(productAddress, typeof(ProductsState));
        }

        return new ProductsState(state);
    }

    public async Task<Product> GetProductState(
        Guid productId,
        CancellationToken stoppingToken = default)
    {
        var productAddress = Nekoyume.Model.Market.Product.DeriveAddress(productId);
        var state = await _service.GetState(productAddress, stoppingToken);
        return state switch
        {
            null => throw new StateNotFoundException(productAddress, typeof(Product)),
            Null => throw new StateIsNullException(productAddress, typeof(Product)),
            _ => Lib9c.Models.Factories.ProductFactory.DeserializeProduct(state)
        };
    }

    public async Task<Nekoyume.Model.State.MarketState> GetMarketState(
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(Addresses.Market, stoppingToken);
        return state switch
        {
            List list => new Nekoyume.Model.State.MarketState(list),
            _
                => throw new StateNotFoundException(
                    Addresses.Market,
                    typeof(Nekoyume.Model.State.MarketState)
                )
        };
    }

    public async Task<WorldBossState> GetWorldBossState(
        Address worldBossAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(worldBossAddress, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(worldBossAddress, typeof(WorldBossState));
        }

        return new WorldBossState(state);
    }

    public async Task<RaiderState> GetRaiderState(
        Address raiderAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(raiderAddress, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(raiderAddress, typeof(RaiderState));
        }

        return new RaiderState(state);
    }

    public async Task<WorldBossKillRewardRecord> GetWorldBossKillRewardRecordState(
        Address worldBossKillRewardRecordAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(worldBossKillRewardRecordAddress, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(
                worldBossKillRewardRecordAddress,
                typeof(WorldBossKillRewardRecord)
            );
        }

        return new WorldBossKillRewardRecord(state);
    }

    public async Task<CombinationSlotState> GetCombinationSlotStateAsync(
        Address slotAddress,
        CancellationToken stoppingToken = default)
    {
        var state = await _service.GetState(slotAddress, stoppingToken);
        if (state is null)
        {
            throw new StateNotFoundException(slotAddress, typeof(CombinationSlotState));
        }

        return new CombinationSlotState(state);
    }

    public async Task<PetState> GetPetState(
        Address petStateAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(petStateAddress, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(petStateAddress, typeof(PetState));
        }

        return new PetState(state);
    }

    public async Task<IValue?> GetAvatarStateWithLegacyAccount(
        Address avatarAddress,
        Address accountAddress,
        Address legacyAddress,
        CancellationToken stoppingToken = default
    ) =>
        await _service.GetState(avatarAddress, accountAddress, stoppingToken)
        ?? await _service.GetState(legacyAddress, stoppingToken);

    public async Task<IValue?> GetStateWithLegacyAccount(
        Address address,
        Address accountAddress,
        CancellationToken stoppingToken = default
    ) =>
        await _service.GetState(address, accountAddress, stoppingToken)
        ?? await _service.GetState(address, stoppingToken);
}
