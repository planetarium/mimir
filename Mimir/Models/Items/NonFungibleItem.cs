using MongoDB.Bson;

namespace Mimir.Models.Items;

public class NonFungibleItem(BsonValue nonFungibleItem) : Item(nonFungibleItem)
{
    public Guid NonFungibleId { get; set; } =
        Guid.TryParse(nonFungibleItem["NonFungibleId"].AsString, out var nonFungibleId)
            ? nonFungibleId
            : Guid.Empty;
}
