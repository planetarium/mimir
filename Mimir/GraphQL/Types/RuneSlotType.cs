using Mimir.Models.Abstractions;

namespace Mimir.GraphQL.Types;

public class RuneSlotType : ObjectType<IRuneSlot>
{
    protected override void Configure(IObjectTypeDescriptor<IRuneSlot> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field(f => f.SlotIndex)
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
            .Field(f => f.RuneSheetId)
            .Name("runeSheetId")
            .Description("The RuneSheet ID of the rune equipped in the rune slot.")
            .Type<IntType>();
    }
}
