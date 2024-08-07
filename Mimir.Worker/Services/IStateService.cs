using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.Worker.Services;

public interface IStateService
{
    Task<long> GetLatestIndex();
    Task<IValue?> GetState(Address address);
    Task<IValue?> GetState(Address address, Address accountAddress);
    Task<IValue?[]> GetStates(Address[] addresses);
    Task<IValue?[]> GetStates(Address[] addresses, Address accountAddress);
}
