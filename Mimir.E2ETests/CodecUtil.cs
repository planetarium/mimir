using Bencodex;
using Bencodex.Types;

namespace Mimir.E2ETests;

public static class CodecUtil
{
    private static readonly Codec Codec = new();

    public static IValue DecodeState(string stateHex)
    {
        var decoded = Codec.Decode(Convert.FromHexString(stateHex));
        return decoded;
    }
}
