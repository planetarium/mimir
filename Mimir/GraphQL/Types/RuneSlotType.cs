using Nekoyume.Model.Rune;

namespace Mimir.GraphQL.Types;

public class RuneSlotType : ObjectType<RuneSlot>
{
    protected override void Configure(IObjectTypeDescriptor<RuneSlot> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field(f => f.Index)
            .Description("The index of the rune slot.")
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.RuneSlotType)
            .Description("The type of the rune slot.")
            .Type<NonNullType<EnumType<Nekoyume.Model.EnumType.RuneSlotType>>>();
        descriptor
            .Field(f => f.RuneType)
            .Description("The type of the rune.")
            .Type<NonNullType<EnumType<Nekoyume.Model.EnumType.RuneType>>>();
        descriptor
            .Field(f => f.IsLock)
            .Description("Whether the rune slot is locked.")
            .Type<NonNullType<BooleanType>>();
        descriptor
            .Field(f => f.RuneId)
            .Name("runeSheetId")
            .Description("The RuneSheet ID of the rune equipped in the rune slot.")
            .Type<IntType>();
    }
}
