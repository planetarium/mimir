using MongoDB.Bson;
using Nekoyume.Model.State;

namespace Mimir.Models.Assets;

public class Collection
{
    public int[] CollectionSheetIds { get; set; }

    public Collection(CollectionState collectionState)
    {
        CollectionSheetIds = collectionState.Ids.ToArray();
    }

    public Collection(BsonDocument document)
    {
        CollectionSheetIds = document["Ids"].AsBsonArray
            .Select(x => x.AsInt32)
            .ToArray();
    }
}
