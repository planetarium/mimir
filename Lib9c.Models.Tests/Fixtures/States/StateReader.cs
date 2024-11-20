using Bencodex;
using Bencodex.Types;
using Libplanet.Common;

namespace Lib9c.Models.Tests.Fixtures.States;

public static class StateReader
{
    private static readonly string StatesFullPath = Path.GetFullPath(
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "../../../",
            "Fixtures",
            "States"));

    private static readonly Codec Codec = new();

    public static IValue ReadState(string fileName)
    {
        var path = Path.Combine(StatesFullPath, fileName);
        var text = File.ReadAllText(path).Trim();
        return Codec.Decode(ByteUtil.ParseHex(text));
    }
}
