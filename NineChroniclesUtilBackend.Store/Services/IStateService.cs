using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Crypto;

namespace NineChroniclesUtilBackend.Store.Services;


public interface IStateService
{
    Task<IValue?> GetState(Address address);
    Task<IValue?> GetState(Address address, Address accountAddress);
}
