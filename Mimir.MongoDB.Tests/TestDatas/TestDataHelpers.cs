using Bencodex;
using Bencodex.Types;

namespace Mimir.MongoDB.Tests.TestDatas;

public static class TestDataHelpers
{
    private static readonly Codec Codec = new();

    /// <summary>
    /// Load state from file.
    /// </summary>
    /// <param name="path">Typically, it is filename under 'TestDatas/States' directory.</param>
    /// <returns>Loaded state from file.</returns>
    public static IValue LoadState(string path)
    {
        var bytes = File.ReadAllBytes(Path.Combine("TestDatas/States/", path));
        return Codec.Decode(bytes);
    }
}
