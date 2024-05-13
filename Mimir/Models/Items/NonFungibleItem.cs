using MongoDB.Bson;
using Nekoyume.Model.Item;

namespace Mimir.Models.Items;

public class NonFungibleItem : Item
{
    public Guid NonFungibleId { get; set; }

    public NonFungibleItem(INonFungibleItem nonFungibleItem) : base(nonFungibleItem)
    {
        NonFungibleId = nonFungibleItem.NonFungibleId;
    }

    public NonFungibleItem(BsonValue nonFungibleItem) : base(nonFungibleItem)
    {
        NonFungibleId = Guid.TryParse(nonFungibleItem["NonFungibleId"].AsString, out var nonFungibleId)
            ? nonFungibleId
            : Guid.Empty;
    }
}
