using Bencodex;
using Bencodex.Types;
using Libplanet.Common;

namespace Lib9c.Models.Tests.Fixtures.States;

public static class StateWriter
{
    private static readonly string StatesFullPath = Path.GetFullPath(
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "../../../",
            "Fixtures",
            "States"));

    private static readonly Codec Codec = new();

    public static void WriteState(string fileName, IValue bencoded)
    {
        var hex = ByteUtil.Hex(Codec.Encode(bencoded));
        WriteFile(fileName, hex);
    }

    private static void WriteFile(string fileName, string hex)
    {
        var path = Path.Combine(StatesFullPath, fileName);
        File.WriteAllText(path, hex);
    }
}
