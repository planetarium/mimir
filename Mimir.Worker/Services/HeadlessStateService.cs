using Bencodex;
using Bencodex.Types;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Blocks;
using Mimir.Worker.Client;

namespace Mimir.Worker.Services;

public class HeadlessStateService(IHeadlessGQLClient client) : IStateService
{
    private static readonly Codec Codec = new();

    private (long, BlockHash) TipInfo { get; set; }

    public long TipIndex => TipInfo.Item1;

    public Task<IValue?[]> GetStates(Address[] addresses, CancellationToken stoppingToken = default)
    {
        return Task.WhenAll(addresses.Select((a) => GetState(a, stoppingToken)));
    }

    public Task<IValue?[]> GetStates(
        Address[] addresses,
        Address accountAddress,
        CancellationToken stoppingToken = default
    )
    {
        return Task.WhenAll(addresses.Select(addr => GetState(addr, accountAddress)));
    }

    public async Task<IValue?> GetState(Address address, CancellationToken stoppingToken = default)
    {
        return await GetState(address, ReservedAddresses.LegacyAccount, stoppingToken);
    }

    public async Task<IValue?> GetState(
        Address address,
        Address accountAddress,
        CancellationToken stoppingToken = default
    )
    {
        var result = await client.GetStateAsync(
            accountAddress.ToString(),
            address.ToString(),
            stoppingToken
        );

        return Codec.Decode(Convert.FromHexString(result.state));
    }

    public async Task<long> GetLatestIndex(CancellationToken stoppingToken = default)
    {
        var result = await client.GetTipAsync(stoppingToken);

        return result.nodeStatus.tip.index;
    }
}
