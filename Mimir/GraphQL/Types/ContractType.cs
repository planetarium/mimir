// using Nekoyume.Model.Stake;
//
// namespace Mimir.GraphQL.Types;
//
// public class ContractType : ObjectType<Contract>
// {
//     protected override void Configure(IObjectTypeDescriptor<Contract> descriptor)
//     {
//         descriptor.BindFieldsExplicitly();
//         descriptor
//             .Field(f => f.StakeRegularFixedRewardSheetTableName)
//             .Description("The name of the table that contains the regular fixed rewards for staking.")
//             .Type<NonNullType<StringType>>();
//         descriptor
//             .Field(f => f.StakeRegularRewardSheetTableName)
//             .Description("The name of the table that contains the regular rewards for staking.")
//             .Type<NonNullType<StringType>>();
//         descriptor
//             .Field(f => f.RewardInterval)
//             .Description("The interval at which rewards are given.")
//             .Type<NonNullType<LongType>>();
//         descriptor
//             .Field(f => f.LockupInterval)
//             .Description("The interval at which the stake is locked up.")
//             .Type<NonNullType<LongType>>();
//     }
// }
