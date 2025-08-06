using Libplanet.Crypto;

namespace Mimir.Services;

public interface IStateRecoveryService
{
    Task<bool> TryRecoverAgentStateAsync(Address agentAddress);
    Task<bool> TryRecoverAvatarStateAsync(Address avatarAddress);
    Task<bool> TryRecoverNCGBalanceAsync(Address agentAddress);
    Task<bool> IsStateExistsInCacheAsync(string cacheKey);
    Task SetStateExistsInCacheAsync(string cacheKey);
} 