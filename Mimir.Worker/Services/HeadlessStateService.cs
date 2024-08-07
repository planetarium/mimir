using Bencodex;
using Bencodex.Types;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Blocks;
using Mimir.Worker.Client;

namespace Mimir.Worker.Services;

public class HeadlessStateService(HeadlessGQLClient client) : IStateService
{
    private static readonly Codec Codec = new();

    private (long, BlockHash) TipInfo { get; set; }

    public long TipIndex => TipInfo.Item1;

    public Task<IValue?[]> GetStates(Address[] addresses)
    {
        return Task.WhenAll(addresses.Select(GetState));
    }

    public Task<IValue?[]> GetStates(Address[] addresses, Address accountAddress)
    {
        return Task.WhenAll(addresses.Select(addr => GetState(addr, accountAddress)));
    }

    public async Task<IValue?> GetState(Address address)
    {
        return await GetState(address, ReservedAddresses.LegacyAccount);
    }

    public async Task<IValue?> GetState(Address address, Address accountAddress)
    {
        var result = await client.GetStateAsync(accountAddress.ToString(), address.ToString());

        return Codec.Decode(Convert.FromHexString(result.state));
    }

    public async Task<long> GetLatestIndex()
    {
        var result = await client.GetTipAsync();

        return result.nodeStatus.tip.index;
    }
}
