using System.Threading.Tasks;
using Bencodex.Types;
using Libplanet.Crypto;

namespace NineChroniclesUtilBackend.Store.Services;


public interface IStateService
{
    Task<IValue?[]> GetStates(Address[] addresses);
    Task<IValue?> GetState(Address address);
}
