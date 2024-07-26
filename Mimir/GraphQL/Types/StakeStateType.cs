// using Nekoyume.Model.Stake;
//
// namespace Mimir.GraphQL.Types;
//
// public class StakeStateType : ObjectType<StakeStateV2>
// {
//     protected override void Configure(IObjectTypeDescriptor<StakeStateV2> descriptor)
//     {
//         descriptor.BindFieldsExplicitly();
//         descriptor
//             .Field("Contract")
//             .Description("The contract of the stake.")
//             .Type<NonNullType<ContractType>>()
//             .Resolve(context => context.Parent<StakeStateV2>().Contract);
//         descriptor
//             .Field("StartedBlockIndex")
//             .Description("The block index when the stake started.")
//             .Type<NonNullType<LongType>>()
//             .Resolve(context => context.Parent<StakeStateV2>().StartedBlockIndex);
//         descriptor
//             .Field("ReceivedBlockIndex")
//             .Description("The block index when the stake received. If 0, the stake is not received yet.")
//             .Type<NonNullType<LongType>>()
//             .Resolve(context => context.Parent<StakeStateV2>().ReceivedBlockIndex);
//         descriptor
//             .Field(f => f.CancellableBlockIndex)
//             .Description("The block index when the stake can be cancelled.")
//             .Type<NonNullType<LongType>>();
//         descriptor
//             .Field(f => f.ClaimableBlockIndex)
//             .Description("The block index when the stake can be claimed.")
//             .Type<NonNullType<LongType>>();
//         descriptor
//             .Field(f => f.ClaimedBlockIndex)
//             .Description("The block index when the stake was claimed. If 0, the stake is not claimed yet.")
//             .Type<NonNullType<LongType>>();
//     }
// }
