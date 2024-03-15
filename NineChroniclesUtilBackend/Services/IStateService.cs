using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Crypto;

namespace NineChroniclesUtilBackend.Services;

public interface IStateService
{
    Task<IValue?> GetState(Address address);
    Task<IValue?> GetState(Address address, Address accountAddress);
    Task<IValue?[]> GetStates(Address[] addresses);
    Task<IValue?[]> GetStates(Address[] addresses, Address accountAddress);
}
