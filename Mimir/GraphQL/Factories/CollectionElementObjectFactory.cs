using Mimir.GraphQL.Objects;

namespace Mimir.GraphQL.Factories;

public static class CollectionElementObjectFactory
{
    public static CollectionElementObject Create(int collectionSheetId) => new()
    {
        CollectionSheetId = collectionSheetId
    };
}
