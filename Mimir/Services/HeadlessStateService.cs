using Bencodex;
using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Crypto;
using Libplanet.Action.State;
using Libplanet.Types.Blocks;
using StrawberryShake;

namespace Mimir.Services;

public class HeadlessStateService(IHeadlessGQLClient client) : IStateService
{
    private static readonly Codec Codec = new();

    private (long, BlockHash) TipInfo { get; set; }

    public long TipIndex => TipInfo.Item1;

    public Task<IValue?[]> GetStates(Address[] addresses, long? index=null)
    {
        return Task.WhenAll(addresses.Select(addr => GetState(addr, index)));
    }

    public Task<IValue?[]> GetStates(Address[] addresses, Address accountAddress, long? index)
    {
        return Task.WhenAll(addresses.Select(addr => GetState(addr, accountAddress, index)));
    }

    public async Task<IValue?> GetState(Address address, long? index)
    {
        return await GetState(address, ReservedAddresses.LegacyAccount, index);
    }

    public async Task<IValue?> GetState(Address address, Address accountAddress, long? index)
    {
        var result = await client.GetState.ExecuteAsync(accountAddress.ToString(), address.ToString(), index);
        result.EnsureNoErrors();

        if (result.Data?.State is null)
        {
            return null;
        }

        return Codec.Decode(Convert.FromHexString(result.Data.State));
    }

    private static void UpdateTipIndex()
    {
        
    }
}
