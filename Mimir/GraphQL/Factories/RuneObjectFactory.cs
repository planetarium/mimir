using Mimir.GraphQL.Objects;
using Mimir.Models.Assets;

namespace Mimir.GraphQL.Factories;

public static class RuneObjectFactory
{
    public static RuneObject Create(Rune rune) => new()
    {
        RuneSheetId = rune.RuneSheetId,
        Level = rune.Level,
    };
}
