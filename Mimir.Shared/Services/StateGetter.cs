using System.Numerics;
using Bencodex.Types;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Lib9c.Models.States;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Assets;
using Mimir.Shared.Constants;
using Mimir.Shared.Exceptions;
using Mimir.Shared.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.TableData;

namespace Mimir.Shared.Services;

public class StateGetterService : IStateGetterService
{
    private readonly IStateService _service;
    private readonly PlanetType _planetType;

    public StateGetterService(IStateService service, PlanetType planetType)
    {
        _service = service;
        _planetType = planetType;
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

    public async Task<string> GetNCGBalanceAsync(
        Address address,
        CancellationToken stoppingToken = default
    )
    {
        var currency = _planetType switch
        {
            PlanetType.Odin => NCGCurrency.OdinNCGCurrency,
            PlanetType.Heimdall => NCGCurrency.HeimdallNCGCurrency,
            _ => throw new ArgumentOutOfRangeException(),
        };

        var state = await _service.GetNCGBalance(address, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(address, typeof(long));
        }
        var stateString = state.ToString();
        BigInteger value;
        if (stateString.Contains('.'))
        {
            value = BigInteger.Parse(stateString.Replace(".", ""));
        }
        else
        {
            value = BigInteger.Parse(stateString) * BigInteger.Pow(10, currency.DecimalPlaces);
        }
        return FungibleAssetValue.FromRawValue(currency, value).GetQuantityString();
    }

    public async Task<long> GetDailyRewardAsync(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await GetStateWithLegacyAccount(
            avatarAddress,
            Addresses.DailyReward,
            stoppingToken
        );

        if (state is null)
        {
            throw new StateNotFoundException(avatarAddress, typeof(long));
        }

        if (state is not Integer value)
            throw new ArgumentException(
                $"Invalid state type. Expected {nameof(Integer)}, got {state.GetType().Name}.",
                nameof(state)
            );

        return value;
    }

    public async Task<AvatarState> GetAvatarStateAsync(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await GetStateWithLegacyAccount(avatarAddress, Addresses.Avatar, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(avatarAddress, typeof(AvatarState));
        }

        var avatarState = new AvatarState(state);

        return avatarState;
    }

    public async Task<AgentState> GetAgentStateAccount(
        Address agentAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await GetStateWithLegacyAccount(agentAddress, Addresses.Agent, stoppingToken);

        if (state is null)
        {
            throw new StateNotFoundException(agentAddress, typeof(AgentState));
        }

        var agentState = new AgentState(state);

        return agentState;
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

    public async Task<ProductsState> GetProductsState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
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
        CancellationToken stoppingToken = default
    )
    {
        var productAddress = Nekoyume.Model.Market.Product.DeriveAddress(productId);
        var state = await _service.GetState(productAddress, stoppingToken);
        return state switch
        {
            null => throw new StateNotFoundException(productAddress, typeof(Product)),
            Null => throw new StateIsNullException(productAddress, typeof(Product)),
            _ => Lib9c.Models.Factories.ProductFactory.DeserializeProduct(state),
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
            _ => throw new StateNotFoundException(
                Addresses.Market,
                typeof(Nekoyume.Model.State.MarketState)
            ),
        };
    }

    public async Task<WorldBossState> GetWorldBossStateAsync(
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

    public async Task<RaiderState> GetRaiderStateAsync(
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

    public async Task<WorldBossKillRewardRecord> GetWorldBossKillRewardRecordStateAsync(
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

    public async Task<AllCombinationSlotState> GetAllCombinationSlotStateAsync(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    )
    {
        var state = await _service.GetState(
            avatarAddress,
            Addresses.CombinationSlot,
            stoppingToken
        );
        if (state is not null)
        {
            return new AllCombinationSlotState(state);
        }

        var allCombinationSlotState = new AllCombinationSlotState();
        for (var i = 0; i < Nekoyume.Model.State.AvatarState.DefaultCombinationSlotCount; i++)
        {
            var slotAddress = Nekoyume.Model.State.CombinationSlotState.DeriveAddress(
                avatarAddress,
                i
            );
            var combinationSlotState = await _service.GetState(slotAddress, stoppingToken);
            if (combinationSlotState is null)
            {
                allCombinationSlotState.CombinationSlots[i] = new CombinationSlotState
                {
                    Address = slotAddress,
                    Index = i,
                };
            }
            else
            {
                allCombinationSlotState.CombinationSlots[i] = new CombinationSlotState(
                    combinationSlotState
                )
                {
                    Index = i,
                };
            }
        }

        return allCombinationSlotState;
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

    public async Task<IEnumerable<PetState>> GetPetStates(
        Address[] petStateAddresses,
        CancellationToken stoppingToken = default
    )
    {
        var states = await _service.GetStates(petStateAddresses, stoppingToken);
        return states.Select(e => new PetState(e));
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
