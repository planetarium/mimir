using HotChocolate.AspNetCore;
using Lib9c.GraphQL.Extensions;
using Lib9c.GraphQL.InputObjects;
using Lib9c.Models.Block;
using Lib9c.Models.Items;
using Lib9c.Models.Market;
using Lib9c.Models.States;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Types;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;
using Mimir.Services;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Extensions;
using Nekoyume.Model.EnumType;
using Nekoyume.TableData;

namespace Mimir.GraphQL.Queries;

// NOTE: Sort methods in alphabetical order.
public class Query
{
    /// <summary>
    /// Get an action point by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The action point.</returns>
    public async Task<int> GetActionPointAsync(
        Address address,
        [Service] IActionPointRepository repo
    ) => (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get all action types.
    /// </summary>
    /// <returns>All action types.</returns>
    public async Task<IEnumerable<ActionTypeDocument>> GetActionTypesAsync(
        [Service] IActionTypeRepository repo
    ) => (await repo.GetAllAsync()).OrderBy(x => x.Id);

    /// <summary>
    /// Get an agent state by address.
    /// </summary>
    /// <param name="address">The address of the agent.</param>
    /// <returns>The agent state</returns>
    public async Task<AgentState> GetAgentAsync(Address address, [Service] IAgentRepository repo)
    {
        try
        {
            return (await repo.GetByAddressAsync(address)).Object;
        }
        catch (Mimir.MongoDB.Exceptions.DocumentNotFoundInMongoCollectionException)
        {
            HangfireJobs.EnqueueAgentStateRecovery(address);
            throw;
        }
    }

    /// <summary>
    /// Get an avatar state by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The avatar state</returns>
    public async Task<AvatarState> GetAvatarAsync(Address address, [Service] IAvatarRepository repo)
    {
        try
        {
            return (await repo.GetByAddressAsync(address)).Object;
        }
        catch (Mimir.MongoDB.Exceptions.DocumentNotFoundInMongoCollectionException)
        {
            HangfireJobs.EnqueueAvatarStateRecovery(address);
            throw;
        }
    }

    /// <summary>
    /// Get the balance of a specific currency for a given address.
    /// Choose one of the following parameters to specify the currency: currency, currencyTicker
    /// </summary>
    /// <param name="currency">The currency object.</param>
    /// <param name="currencyTicker">The ticker of the currency.</param>
    /// <param name="address">The address of the balance.</param>
    /// <exception cref="GraphQLRequestException"></exception>
    public async Task<string> GetBalanceAsync(
        CurrencyInput? currency,
        string? currencyTicker,
        Address address,
        [Service] IBalanceRepository repo
    )
    {
        if (currency is not null)
        {
            return (await repo.GetByAddressAsync(currency.ToCurrency(), address)).Object;
        }

        if (currencyTicker is not null)
        {
            return (await repo.GetByAddressAsync(currencyTicker.ToCurrency(), address)).Object;
        }

        throw new GraphQLRequestException("Either currency or currencyTicker must be provided.");
    }

    /// <summary>
    /// Get a collection state by avatar address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The collection state for the specified avatar address.</returns>
    public async Task<CollectionState> GetCollectionAsync(
        Address address,
        [Service] ICollectionRepository repo
    ) => (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get combination slot states for a specific avatar address.
    /// </summary>
    /// <param name="avatarAddress">The address of the avatar</param>
    /// <returns>Combination slot states for the specified avatar address.</returns>
    public async Task<Dictionary<int, CombinationSlotState>> GetCombinationSlotsAsync(
        Address avatarAddress,
        [Service] IAllCombinationSlotStateRepository repo
    ) => (await repo.GetByAddressAsync(avatarAddress)).Object.CombinationSlots;

    /// <summary>
    /// Get the daily reward received block index by address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The daily reward received block index.</returns>
    public async Task<long> GetDailyRewardReceivedBlockIndexAsync(
        Address address,
        [Service] IDailyRewardRepository repo
    ) => (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get the inventory state by avatar address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The inventory state for the specified avatar address.</returns>
    public async Task<Inventory> GetInventoryAsync(
        Address address,
        [Service] IInventoryRepository repo
    ) => (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get metadata by collection name.
    /// </summary>
    /// <param name="collectionName">The name of the collection.</param>
    /// <returns>The metadata</returns>
    public async Task<MetadataDocument> GetMetadataAsync(
        string collectionName,
        [Service] IMetadataRepository repo
    ) => await repo.GetByCollectionAsync(collectionName);

    /// <summary>
    /// Get Block by index.
    /// </summary>
    /// <param name="index">Block index.</param>
    /// <returns>The Block Information</returns>
    public async Task<BlockDocument> GetBlockAsync(long index, [Service] IBlockRepository repo) =>
        await repo.GetByIndexAsync(index);

    /// <summary>
    /// Get Transaction by transaction ID.
    /// </summary>
    /// <param name="txId">Transaction ID.</param>
    /// <returns>The Transaction Information</returns>
    public async Task<TransactionDocument> GetTransactionAsync(
        string txId,
        [Service] ITransactionRepository repo
    ) => await repo.GetByTxIdAsync(txId);

    /// <summary>
    /// Get an pet state by avatar address.
    /// </summary>
    /// <param name="avatarAddress">The address of the avatar.</param>
    /// <returns>The agent state</returns>
    public async Task<PetState> GetPetAsync(Address avatarAddress, [Service] IPetRepository repo) =>
        (await repo.GetByAvatarAddressAsync(avatarAddress)).Object;

    /// <summary>
    /// Get the pledge state for a given agent address.
    /// </summary>
    public async Task<PledgeDocument> GetPledgeAsync(
        Address agentAddress,
        [Service] IPledgeRepository repo
    ) => await repo.GetByAddressAsync(agentAddress.GetPledgeAddress());

    /// <summary>
    /// Get the product by product ID.
    /// </summary>
    /// <param name="productId">The product ID</param>
    /// <returns>The product.</returns>
    public async Task<Product> GetProductAsync(Guid productId, [Service] IProductRepository repo) =>
        (await repo.GetByProductIdAsync(productId)).Object;

    /// <summary>
    /// Get the product ids that are contained in the products state for a specific avatar address.
    /// </summary>
    /// <param name="avatarAddress">The address of the avatar.</param>
    /// <returns>The product ids that contained in the products state for the specified avatar address.</returns>
    public async Task<List<Guid>> GetProductIdsAsync(
        Address avatarAddress,
        [Service] IProductsRepository repo
    ) => (await repo.GetByAvatarAddressAsync(avatarAddress)).Object.ProductIds;

    /// <summary>
    /// Get the runes for a specific avatar address.
    /// </summary>
    /// <param name="avatarAddress">The address of the avatar.</param>
    /// <returns>The runes for a specific avatar address.</returns>
    public async Task<RuneState[]> GetRunesAsync(
        Address avatarAddress,
        [Service] AllRuneRepository repo
    ) => (await repo.GetByAddressAsync(avatarAddress)).Object.Runes.Values.ToArray();

    /// <summary>
    /// Get a stake state by agent address.
    /// </summary>
    /// <param name="address">The address of the agent.</param>
    /// <returns>The stake state.</returns>
    public async Task<StakeState?> GetStakeAsync(
        Address address,
        [Service] IStakeRepository repo
    ) => (await repo.GetByAgentAddressAsync(address)).Object;

    /// <summary>
    /// Get a itemSlot state by avatar address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <param name="battleType">The battleType.</param>
    /// <returns>The stake state.</returns>
    public async Task<ItemSlotState?> GetItemSlotAsync(
        Address address,
        BattleType battleType,
        [Service] IItemSlotRepository repo
    ) => (await repo.GetByAddressAsync(address, battleType)).Object;

    /// <summary>
    /// Get a runeSlot state by avatar address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <param name="battleType">The battleType.</param>
    /// <returns>The rune slot state.</returns>
    public async Task<RuneSlotState?> GetRuneSlotAsync(
        Address address,
        BattleType battleType,
        [Service] IRuneSlotRepository repo
    ) => (await repo.GetByAddressAsync(address, battleType)).Object;

    /// <summary>
    /// Get the world boss.
    /// </summary>
    public async Task<WorldBossState> GetWorldBossAsync(
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [Service] WorldBossRepository worldBossRepo
    )
    {
        var collectionName = CollectionNames.GetCollectionName<WorldBossStateDocument>();
        var metadataDocument = await metadataRepo.GetByCollectionAsync(collectionName);
        var blockIndex = metadataDocument.LatestBlockIndex;
        var worldBossListSheet = await tableSheetsRepo.GetSheetAsync<WorldBossListSheet>();
        WorldBossListSheet.Row row;
        try
        {
            row = worldBossListSheet.FindRowByBlockIndex(blockIndex);
        }
        catch (InvalidOperationException)
        {
            throw new GraphQLException(
                $"Failed to find the world boss row by block index, {blockIndex}"
            );
        }

        var raidId = row.Id;
        var worldBossAddress = Addresses.GetWorldBossAddress(raidId);
        return (await worldBossRepo.GetByAddressAsync(worldBossAddress)).Object;
    }

    /// <summary>
    /// Get the kill reward record of world boss.
    /// </summary>
    public async Task<WorldBossKillRewardRecord> GetWorldBossKillRewardRecordAsync(
        Address avatarAddress,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [Service] WorldBossKillRewardRecordRepository worldBossKillRewardRecordRepo
    )
    {
        var collectionName = CollectionNames.GetCollectionName<WorldBossStateDocument>();
        var metadataDocument = await metadataRepo.GetByCollectionAsync(collectionName);
        var blockIndex = metadataDocument.LatestBlockIndex;
        var worldBossListSheet = await tableSheetsRepo.GetSheetAsync<WorldBossListSheet>();
        WorldBossListSheet.Row row;
        try
        {
            row = worldBossListSheet.FindRowByBlockIndex(blockIndex);
        }
        catch (InvalidOperationException)
        {
            throw new GraphQLException(
                $"Failed to find the world boss row by block index, {blockIndex}"
            );
        }

        var raidId = row.Id;
        var worldBossKillRewardRecordAddress = Addresses.GetWorldBossKillRewardRecordAddress(
            avatarAddress,
            raidId
        );
        return (
            await worldBossKillRewardRecordRepo.GetByAddressAsync(worldBossKillRewardRecordAddress)
        ).Object;
    }

    /// <summary>
    /// Get the raider of world boss.
    /// </summary>
    public async Task<RaiderState> GetWorldBossRaiderAsync(
        Address avatarAddress,
        [Service] MetadataRepository metadataRepo,
        [Service] TableSheetsRepository tableSheetsRepo,
        [Service] WorldBossRaiderRepository worldBossRaiderRepo
    )
    {
        var collectionName = CollectionNames.GetCollectionName<WorldBossStateDocument>();
        var metadataDocument = await metadataRepo.GetByCollectionAsync(collectionName);
        var blockIndex = metadataDocument.LatestBlockIndex;
        var worldBossListSheet = await tableSheetsRepo.GetSheetAsync<WorldBossListSheet>();
        WorldBossListSheet.Row row;
        try
        {
            row = worldBossListSheet.FindRowByBlockIndex(blockIndex);
        }
        catch (InvalidOperationException)
        {
            throw new GraphQLException(
                $"Failed to find the world boss row by block index, {blockIndex}"
            );
        }

        var raidId = row.Id;
        var raiderAddress = Addresses.GetRaiderAddress(avatarAddress, raidId);
        return (await worldBossRaiderRepo.GetByAddressAsync(raiderAddress)).Object;
    }

    /// <summary>
    /// Get a world information state by avatar address.
    /// </summary>
    /// <param name="address">The address of the avatar.</param>
    /// <returns>The world information state.</returns>
    public async Task<WorldInformationState> GetWorldInformationAsync(
        Address address,
        [Service] IWorldInformationRepository repo
    ) => (await repo.GetByAddressAsync(address)).Object;

    /// <summary>
    /// Get WNCG price from CoinMarketCap API.
    /// </summary>
    /// <returns>The WNCG price information.</returns>
    public async Task<WncgPriceType?> GetWncgPriceAsync(
        [Service] IWncgPriceService wncgPriceService
    ) => await wncgPriceService.GetWncgPriceAsync();
}
