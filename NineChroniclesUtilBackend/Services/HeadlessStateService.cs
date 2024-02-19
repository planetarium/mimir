using Bencodex;
using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Crypto;
using Libplanet.Action.State;
using Libplanet.Types.Blocks;
using StrawberryShake;

namespace NineChroniclesUtilBackend.Services;

public class HeadlessStateService(IHeadlessGQLClient client) : IStateService
{
    private static readonly Codec Codec = new();

    private (long, BlockHash) TipInfo { get; set; }

    public long TipIndex => TipInfo.Item1;

    public Task<IValue?[]> GetStates(Address[] addresses)
    {
        return Task.WhenAll(addresses.Select(GetState));
    }

    public async Task<IValue?> GetState(Address address)
    {
        var result = await client.GetState.ExecuteAsync(ReservedAddresses.LegacyAccount.ToString(), address.ToString());
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
