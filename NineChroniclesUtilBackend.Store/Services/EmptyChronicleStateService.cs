using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Action.State;
using Libplanet.Types.Blocks;
using NineChroniclesUtilBackend.Store.Client;

namespace NineChroniclesUtilBackend.Store.Services;

public class EmptyChronicleStateService : IStateService
{
    private readonly EmptyChronicleClient client;
    private static readonly Codec Codec = new();

    public EmptyChronicleStateService(EmptyChronicleClient client)
    {
        this.client = client;
    }

    public Task<IValue?> GetState(Address address, long? blockIndex=null)
    {
        return GetState(address, ReservedAddresses.LegacyAccount, blockIndex);
    }

    public Task<IValue?[]> GetStates(Address[] addresses)
    {
        return Task.WhenAll(addresses.Select(addr => GetState(addr)));
    }

    public async Task<IValue?> GetState(Address address, Address accountAddress, long? blockIndex=null)
    {
        var result = await client.GetStateByAddressAsync(address.ToString(), accountAddress.ToString(), blockIndex);

        if (result.Value is null)
        {
            return null;
        }

        return Codec.Decode(Convert.FromHexString(result.Value));
    }
}
