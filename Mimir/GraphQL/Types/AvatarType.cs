using Lib9c.GraphQL.Types;
using Lib9c.Models.Rune;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Resolvers;
using Nekoyume.Model.EnumType;

namespace Mimir.GraphQL.Types;

public class AvatarType : ObjectType<AvatarObject>
{
    protected override void Configure(IObjectTypeDescriptor<AvatarObject> descriptor)
    {
        descriptor
            .Field(f => f.Address)
            .Type<NonNullType<AddressType>>();
        descriptor
            .Field(f => f.AgentAddress)
            .Type<AddressType>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetAgentAddress(default!, default!, default!, default!));
        descriptor
            .Field(f => f.Index)
            .Type<IntType>();
        descriptor
            .Field("name")
            .Type<StringType>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetName(default!, default!, default!, default!));
        descriptor
            .Field("level")
            .Type<IntType>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetLevel(default!, default!, default!, default!));
        descriptor
            .Field("actionPoint")
            .Type<IntType>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetActionPoint(default!, default!));
        descriptor
            .Field("dailyRewardReceivedBlockIndex")
            .Type<IntType>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetDailyRewardReceivedBlockIndex(default!, default!));
        descriptor
            .Field("inventory")
            .Type<InventoryType>()
            .ResolveWith<AvatarResolver>(_ => AvatarResolver.GetInventory(default!));
        descriptor
            .Field("runes")
            .Type<ListType<NonNullType<RuneType>>>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetRunes(default!, default!));
        descriptor
            .Field("collection")
            .Type<ListType<NonNullType<CollectionElementType>>>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetCollectionElements(default!, default!));
        descriptor
            .Field("itemSlots")
            .Argument("battleType", a => a
                .Description("The type of battle that the item slot is used for.")
                .Type<NonNullType<EnumType<BattleType>>>())
            .Type<ItemSlotStateType>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetItemSlot(default!, default!, default!));
        descriptor
            .Field("runeSlots")
            .Argument("battleType", a => a
                .Description("The type of battle that the rune slot is used for.")
                .Type<NonNullType<EnumType<BattleType>>>())
            .Type<ListType<ObjectType<RuneSlot>>>()
            .ResolveWith<AvatarResolver>(_ =>
                AvatarResolver.GetRuneSlots(default!, default!, default!));
    }
}
