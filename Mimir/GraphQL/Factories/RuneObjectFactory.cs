using Mimir.GraphQL.Objects;
using Mimir.Models.Assets;

namespace Mimir.GraphQL.Factories;

public static class RuneObjectFactory
{
    public static RuneObject Create(Rune rune)
    {
        return new RuneObject
        {
            RuneSheetId = rune.RuneSheetId,
            Level = rune.Level,
        };
    }
}
