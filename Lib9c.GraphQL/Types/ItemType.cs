using HotChocolate.Types;
using Lib9c.GraphQL.Objects;

namespace Lib9c.GraphQL.Types;

public class ItemType : ObjectType<ItemObject>
{
    protected override void Configure(IObjectTypeDescriptor<ItemObject> descriptor)
    {
        descriptor
            .Field(f => f.ItemSheetId)
            .Name("itemSheetId")
            .Description("The ItemSheet ID of the item.")
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.Grade)
            .Name("grade")
            .Description("The grade of the item.")
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.ItemType)
            .Name("itemType")
            .Description("The ItemType of the item.")
            .Type<NonNullType<ItemTypeEnumType>>();
        descriptor
            .Field(f => f.ItemSubType)
            .Name("itemSubType")
            .Description("The ItemSubType of the item.")
            .Type<NonNullType<ItemSubTypeEnumType>>();
        descriptor
            .Field(f => f.ElementalType)
            .Name("elementalType")
            .Description("The ElementalType of the item.")
            .Type<NonNullType<ElementalTypeEnumType>>();
        descriptor
            .Field(f => f.Count)
            .Name("count")
            .Description("The count of the item.")
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.Level)
            .Name("level")
            .Description("The level of the item.")
            .Type<IntType>();
        descriptor
            .Field(f => f.RequiredBlockIndex)
            .Name("requiredBlockIndex")
            .Description("The required block index of the item.")
            .Type<IntType>();
        descriptor
            .Field(f => f.FungibleId)
            .Name("fungibleId")
            .Description("The Fungible ID of the item.")
            .Type<HashDigestSHA256Type>();
        descriptor
            .Field(f => f.NonFungibleId)
            .Name("nonFungibleId")
            .Description("The Non-Fungible ID of the item.")
            .Type<GuidType>();
        descriptor
            .Field(f => f.TradableId)
            .Name("tradableId")
            .Description("The Tradable ID of the item.")
            .Type<GuidType>();
    }
}
