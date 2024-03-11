using Libplanet.Crypto;
using NineChroniclesUtilBackend.Store.Models;

namespace NineChroniclesUtilBackend.Store.Services;

public interface IStateStorage
{
    Task AddArenaData(ArenaData arenaData);
    Task AddArenaData(List<ArenaData> arenaData);
    Task AddAvatarData(AvatarData avatarData);
    Task AddAvatarData(List<AvatarData> avatarData);
    Task LinkAvatarWithArenaAsync(Address address);
    Task UpdateLatestBlockIndex(long blockIndex);
    Task<long> GetLatestBlockIndex();
}
