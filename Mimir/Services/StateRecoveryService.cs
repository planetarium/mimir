using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using Mimir.Options;
using Mimir.Shared.Exceptions;
using Mimir.Shared.Services;
using Serilog;
using StackExchange.Redis;
using ILogger = Serilog.ILogger;

namespace Mimir.Services;

public class StateRecoveryService : IStateRecoveryService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IStateGetterService _stateGetter;
    private readonly IMongoDbService _dbService;
    private readonly ILogger _logger;
    private readonly HangfireOption _hangfireOption;

    public StateRecoveryService(
        IConnectionMultiplexer redis,
        IStateService stateService,
        IMongoDbService dbService,
        IOptions<HangfireOption> hangfireOption,
        IStateGetterService stateGetterService
    )
    {
        _redis = redis;
        _stateGetter = stateGetterService;
        _dbService = dbService;
        _hangfireOption = hangfireOption.Value;
        _logger = Log.ForContext<StateRecoveryService>();
    }

    public async Task<bool> TryRecoverAgentStateAsync(Address agentAddress)
    {
        var cacheKey = $"{_hangfireOption.RedisPrefix}:agent_not_exists:{agentAddress.ToHex()}";

        if (await IsStateExistsInCacheAsync(cacheKey))
        {
            _logger.Information(
                "Agent {AgentAddress} is cached as not exists, skipping recovery",
                agentAddress
            );
            return false;
        }

        try
        {
            var agentState = await _stateGetter.GetAgentStateAccount(agentAddress);
            var document = new AgentDocument(0, agentState.Address, agentState);

            await _dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                [document]
            );

            _logger.Information(
                "Successfully recovered agent state for {AgentAddress}",
                agentAddress
            );
            return true;
        }
        catch (StateNotFoundException)
        {
            await SetStateExistsInCacheAsync(cacheKey);
            _logger.Information(
                "Agent {AgentAddress} does not exist in blockchain, cached for future",
                agentAddress
            );
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to recover agent state for {AgentAddress}", agentAddress);
            return false;
        }
    }

    public async Task<bool> TryRecoverAvatarStateAsync(Address avatarAddress)
    {
        var cacheKey = $"{_hangfireOption.RedisPrefix}:avatar_not_exists:{avatarAddress.ToHex()}";

        if (await IsStateExistsInCacheAsync(cacheKey))
        {
            _logger.Information(
                "Avatar {AvatarAddress} is cached as not exists, skipping recovery",
                avatarAddress
            );
            return false;
        }

        try
        {
            var avatarState = await _stateGetter.GetAvatarStateAsync(avatarAddress);
            var inventoryState = await _stateGetter.GetInventoryState(
                avatarAddress,
                CancellationToken.None
            );
            var armorId = inventoryState.GetArmorId();
            var portraitId = inventoryState.GetPortraitId();

            var document = new AvatarDocument(
                0,
                avatarState.Address,
                avatarState,
                armorId,
                portraitId
            );

            await _dbService.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AvatarDocument>(),
                [document]
            );

            _logger.Information(
                "Successfully recovered avatar state for {AvatarAddress}",
                avatarAddress
            );
            return true;
        }
        catch (StateNotFoundException)
        {
            await SetStateExistsInCacheAsync(cacheKey);
            _logger.Information(
                "Avatar {AvatarAddress} does not exist in blockchain, cached for future",
                avatarAddress
            );
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to recover avatar state for {AvatarAddress}", avatarAddress);
            return false;
        }
    }

    public async Task<bool> IsStateExistsInCacheAsync(string cacheKey)
    {
        var db = _redis.GetDatabase();
        return await db.KeyExistsAsync(cacheKey);
    }

    public async Task SetStateExistsInCacheAsync(string cacheKey)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(
            cacheKey,
            "1",
            TimeSpan.FromDays(_hangfireOption.CacheExpirationDays)
        );
    }
}
