using Bencodex;
using Bencodex.Types;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Blocks;
using Mimir.Shared.Client;

namespace Mimir.Shared.Services;

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
        var (result, _) = await client.GetStateAsync(accountAddress, address, stoppingToken);
        return result.State is null ? null : Codec.Decode(Convert.FromHexString(result.State));
    }

    public async Task<long> GetLatestIndex(
        CancellationToken stoppingToken = default,
        Address? accountAddress = null
    )
    {
        var (result, _) = await client.GetTipAsync(stoppingToken, accountAddress);
        return result.NodeStatus.Tip.Index;
    }

    public async Task<string> GetNCGBalance(
        Address address,
        CancellationToken stoppingToken = default
    )
    {
        var (result, _) = await client.GetGoldBalanceAsync(address, stoppingToken);
        return result.GoldBalance;
    }
}
