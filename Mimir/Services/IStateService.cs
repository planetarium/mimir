using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.Services;

public interface IStateService
{
    Task<IValue?> GetState(Address address, long? index);
    Task<IValue?> GetState(Address address, Address accountAddress, long? index);
    Task<IValue?[]> GetStates(Address[] addresses, long? index);
    Task<IValue?[]> GetStates(Address[] addresses, Address accountAddress, long? index);
}
