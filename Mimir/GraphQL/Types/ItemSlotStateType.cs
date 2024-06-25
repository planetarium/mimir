using Lib9c.GraphQL.Types;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.State;

namespace Mimir.GraphQL.Types;

public class ItemSlotStateType : ObjectType<ItemSlotState>
{
    protected override void Configure(IObjectTypeDescriptor<ItemSlotState> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field(f => f.BattleType)
            .Description("The type of battle that the item slot is used for.")
            .Type<NonNullType<EnumType<BattleType>>>();
        descriptor
            .Field(f => f.Costumes)
            .Description("The non-fungible item IDs of the costumes equipped in the item slot.")
            .Type<NonNullType<ListType<GuidType>>>();
        descriptor
            .Field(f => f.Equipments)
            .Description("The non-fungible item IDs of the equipments equipped in the item slot.")
            .Type<NonNullType<ListType<GuidType>>>();
    }
}
