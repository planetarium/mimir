using Libplanet.Crypto;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;

namespace Mimir.Initializer.Initializer;

public static class ConverterMappings
{
    private static Dictionary<Address, IStateDocumentConverter> pairs = new();

    static ConverterMappings()
    {
        pairs.Add(Addresses.Agent, new AgentStateDocumentConverter());
        pairs.Add(Addresses.Avatar, new AgentStateDocumentConverter());
    }

    public static IStateDocumentConverter GetConverter(Address accountAddress)
    {
        return pairs[accountAddress];
    }
}
