using Bencodex;
using Bencodex.Types;
using HeadlessGQL;
using Libplanet.Action.State;
using Libplanet.Crypto;
using Libplanet.Types.Blocks;
using Mimir.Worker.Util;
using StrawberryShake;

namespace Mimir.Worker.Services;

public class HeadlessStateService(IHeadlessGQLClient client) : IStateService
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
        return await RetryUtil.RequestWithRetryAsync(
            async () =>
            {
                var result = await client.GetState.ExecuteAsync(
                    accountAddress.ToString(),
                    address.ToString()
                );

                result.EnsureNoErrors();

                if (result.Data?.State is null)
                {
                    return null;
                }

                return Codec.Decode(Convert.FromHexString(result.Data.State));
            },
            retryCount: 3,
            delayMilliseconds: 5000,
            cancellationToken: new CancellationToken(),
            null
        );
    }

    public async Task<int> GetLatestIndex()
    {
        var result = await client.GetTip.ExecuteAsync();
        result.EnsureNoErrors();

        return result.Data.NodeStatus.Tip.Index;
    }
}
