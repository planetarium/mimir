using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Types.Assets;

namespace Mimir.Services;

public interface IStateService
{
    Task<IValue?> GetState(Address address);
    Task<IValue?> GetState(Address address, Address accountAddress);
    Task<IValue?[]> GetStates(Address[] addresses);
    Task<IValue?[]> GetStates(Address[] addresses, Address accountAddress);
    Task<string> GetBalance(Address address, Currency currency);
}
