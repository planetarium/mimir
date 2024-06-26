using Mimir.GraphQL.Objects;
using Mimir.Models.Assets;

namespace Mimir.GraphQL.Factories;

public static class ItemObjectFactory
{
    public static ItemObject Create(Item item) => new()
    {
        ItemSheetId = item.ItemSheetId,
        Grade = item.Grade,
        ItemType = item.ItemType,
        ItemSubType = item.ItemSubType,
        ElementalType = item.ElementalType,
        Count = item.Count,
        Locked = item.Locked,
        Level = item.Level,
        Exp = item.Exp,
        RequiredBlockIndex = item.RequiredBlockIndex,
        FungibleId = item.FungibleId,
        NonFungibleId = item.NonFungibleId,
        TradableId = item.TradableId,
        Equipped = item.Equipped,
        MainStatType = item.MainStatType,
        StatsMap = item.StatsMap,
        Skills = item.Skills,
        BuffSkills = item.BuffSkills,
    };
}
