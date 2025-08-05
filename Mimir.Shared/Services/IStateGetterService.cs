using Bencodex.Types;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Nekoyume.Model.EnumType;
using Nekoyume.TableData;

namespace Mimir.Shared.Services;

public interface IStateGetterService
{
    Task<T> GetSheet<T>(CancellationToken stoppingToken = default)
        where T : ISheet, new();
    Task<string> GetNCGBalanceAsync(
        Address agentAddress,
        CancellationToken stoppingToken = default
    );
    Task<long> GetDailyRewardAsync(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    );
    Task<AvatarState> GetAvatarStateAsync(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    );
    Task<AgentState> GetAgentStateAccount(
        Address agentAddress,
        CancellationToken stoppingToken = default
    );
    Task<Inventory> GetInventoryState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    );
    Task<ItemSlotState> GetItemSlotState(
        Address avatarAddress,
        BattleType battleType,
        CancellationToken stoppingToken = default
    );
    Task<ProductsState> GetProductsState(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    );
    Task<Product> GetProductState(Guid productId, CancellationToken stoppingToken = default);
    Task<Nekoyume.Model.State.MarketState> GetMarketState(
        CancellationToken stoppingToken = default
    );
    Task<WorldBossState> GetWorldBossStateAsync(
        Address worldBossAddress,
        CancellationToken stoppingToken = default
    );
    Task<RaiderState> GetRaiderStateAsync(
        Address raiderAddress,
        CancellationToken stoppingToken = default
    );
    Task<WorldBossKillRewardRecord> GetWorldBossKillRewardRecordStateAsync(
        Address worldBossKillRewardRecordAddress,
        CancellationToken stoppingToken = default
    );
    Task<AllCombinationSlotState> GetAllCombinationSlotStateAsync(
        Address avatarAddress,
        CancellationToken stoppingToken = default
    );
    Task<PetState> GetPetState(Address petStateAddress, CancellationToken stoppingToken = default);
    Task<IEnumerable<PetState>> GetPetStates(
        Address[] petStateAddresses,
        CancellationToken stoppingToken = default
    );
    Task<IValue?> GetAvatarStateWithLegacyAccount(
        Address avatarAddress,
        Address accountAddress,
        Address legacyAddress,
        CancellationToken stoppingToken = default
    );
    Task<IValue?> GetStateWithLegacyAccount(
        Address address,
        Address accountAddress,
        CancellationToken stoppingToken = default
    );
}
