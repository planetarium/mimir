using Mimir.GraphQL.Objects;
using Mimir.Models.Items;

namespace Mimir.GraphQL.Factories;

public static class ItemObjectFactory
{
    public static ItemObject Create(Item item)
    {
        return new ItemObject
        {
            ItemSheetId = item.ItemSheetId,
            Grade = item.Grade,
            ItemType = item.ItemType,
            ItemSubType = item.ItemSubType,
            ElementalType = item.ElementalType,
            Count = item.Count,
            Level = item.Level,
            RequiredBlockIndex = item.RequiredBlockIndex,
            FungibleId = item.FungibleId,
            NonFungibleId = item.NonFungibleId,
            TradableId = item.TradableId,
        };
    }
}
