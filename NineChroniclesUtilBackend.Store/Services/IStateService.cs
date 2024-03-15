using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Crypto;

namespace NineChroniclesUtilBackend.Store.Services;


public interface IStateService
{
    Task<IValue?> GetState(Address address, long? blockIndex=null);
    Task<IValue?> GetState(Address address, Address accountAddress, long? blockIndex=null);
}
