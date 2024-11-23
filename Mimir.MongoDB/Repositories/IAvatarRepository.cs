using Libplanet.Crypto;
using Mimir.MongoDB.Bson;

namespace Mimir.MongoDB.Repositories;

public interface IAvatarRepository
{
    public Task<AvatarDocument> GetByAddressAsync(Address address);

}