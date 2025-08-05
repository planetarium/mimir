using Bencodex.Types;
using Libplanet.Crypto;

namespace Mimir.Shared.Services;

public interface IStateService
{
    Task<long> GetLatestIndex(
        CancellationToken stoppingToken = default,
        Address? accountAddress = null
    );
    Task<IValue?> GetState(Address address, CancellationToken stoppingToken = default);
    Task<IValue?> GetState(
        Address address,
        Address accountAddress,
        CancellationToken stoppingToken = default
    );
    Task<IValue?[]> GetStates(Address[] addresses, CancellationToken stoppingToken = default);
    Task<IValue?[]> GetStates(
        Address[] addresses,
        Address accountAddress,
        CancellationToken stoppingToken = default
    );
}
