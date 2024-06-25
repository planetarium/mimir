using Nekoyume.Model.Stake;

namespace Mimir.GraphQL.Types;

public class StakeStateType : ObjectType<StakeStateV2>
{
    protected override void Configure(IObjectTypeDescriptor<StakeStateV2> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field(f => f.Contract)
            .Description("The contract of the stake.")
            .Type<NonNullType<ContractType>>();
        descriptor
            .Field(f => f.StartedBlockIndex)
            .Description("The block index when the stake started.")
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.ReceivedBlockIndex)
            .Description("The block index when the stake received. If 0, the stake is not received yet.")
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.CancellableBlockIndex)
            .Description("The block index when the stake can be cancelled.")
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.ClaimableBlockIndex)
            .Description("The block index when the stake can be claimed.")
            .Type<NonNullType<LongType>>();
        descriptor
            .Field(f => f.ClaimedBlockIndex)
            .Description("The block index when the stake was claimed. If 0, the stake is not claimed yet.")
            .Type<NonNullType<LongType>>();
    }
}
