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
        var file = GetFile(fileName);
        return Codec.Decode(ByteUtil.ParseHex(file));
    }

    private static string GetFile(string fileName)
    {
        var path = Path.Combine(StatesFullPath, fileName);
        return File.ReadAllText(path).Trim();
    }
}
