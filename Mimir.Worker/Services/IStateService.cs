using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.Worker.Services;

public interface IStateService
{
    Task<int> GetLatestIndex();
    Task<IValue?> GetState(Address address, long? index);
    Task<IValue?> GetState(Address address, Address accountAddress, long? index);
    Task<IValue?[]> GetStates(Address[] addresses, long? index);
    Task<IValue?[]> GetStates(Address[] addresses, Address accountAddress, long? index);
}
