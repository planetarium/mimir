using HotChocolate.Types;
using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class TradableItemType : ObjectType<TradableItem>
{
    protected override void Configure(IObjectTypeDescriptor<TradableItem> descriptor)
    {
        descriptor.Field(t => t.Equipped).Type<BooleanType>();
        descriptor.Field(t => t.Level).Type<IntType>();
        descriptor.Field(t => t.Exp).Type<IntType>();
        descriptor.Field(t => t.OptionCountFromCombination).Type<IntType>();
        descriptor.Field(t => t.TradableId).Type<StringType>();
        descriptor.Field(t => t.Stat).Type<StatType>();
        descriptor.Field(t => t.SetId).Type<IntType>();
        descriptor.Field(t => t.SpineResourcePath).Type<StringType>();
        descriptor.Field(t => t.MadeWithMimisbrunnrRecipe).Type<BooleanType>();
        descriptor.Field(t => t.UniqueStatType).Type<IntType>();
        descriptor.Field(t => t.ItemId).Type<StringType>();
        descriptor.Field(t => t.NonFungibleId).Type<StringType>();
        descriptor.Field(t => t.Skills).Type<ListType<SkillType>>();
        descriptor.Field(t => t.BuffSkills).Type<ListType<SkillType>>();
        descriptor.Field(t => t.RequiredBlockIndex).Type<LongType>();
        descriptor.Field(t => t.Id).Type<IntType>();
        descriptor.Field(t => t.Grade).Type<IntType>();
        descriptor.Field(t => t.ItemType).Type<IntType>();
        descriptor.Field(t => t.ItemSubType).Type<IntType>();
        descriptor.Field(t => t.ElementalType).Type<IntType>();
    }
}
