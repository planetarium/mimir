using Lib9c.GraphQL.Types;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Resolvers;
using Mimir.MongoDB.Bson;

namespace Mimir.GraphQL.Types;

public class ArenaType : ObjectType<ArenaObject>
{
    protected override void Configure(IObjectTypeDescriptor<ArenaObject> descriptor)
    {
        descriptor
            .Field("round")
            .Description("The round of the arena.")
            .Type<ArenaSheetRoundDataType>()
            .ResolveWith<ArenaResolver>(_ =>
                ArenaResolver.GetRoundAsync(default!, default!, default!, default!));
        descriptor
            .Field("ranking")
            .Description("The avatar's ranking of the arena.")
            .Argument("avatarAddress", a => a.Type<NonNullType<AddressType>>())
            .Type<LongType>()
            .ResolveWith<ArenaResolver>(_ =>
                ArenaResolver.GetRankingAsync(default!, default!, default!, default!, default!, default!));
        descriptor
            .Field("leaderboard")
            .Description("The leaderboard of the arena.")
            .Argument("ranking", a => a
                .Description("The ranking of the first avatar. default is 1. This must be greater than or equal to 1.")
                .Type<NonNullType<LongType>>()
                .DefaultValue(1))
            .Argument("length", a => a
                .Description("The number of avatars. default is 10. This must be greater than or equal to 1 and less than or equal to 100.")
                .Type<NonNullType<IntType>>()
                .DefaultValue(10))
            .Type<ListType<ObjectType<ArenaParticipantDocument>>>()
            .ResolveWith<ArenaResolver>(_ =>
                ArenaResolver.GetLeaderboardAsync(
                    default!, default!, default!, default!, default!, default!, default!));
        descriptor
            .Field("leaderboardByAvatarAddress")
            .Description("The leaderboard of the arena filtered by the avatar's address.")
            .Argument("avatarAddress", a => a.Type<NonNullType<AddressType>>())
            .Type<ListType<ObjectType<ArenaParticipantDocument>>>()
            .ResolveWith<ArenaResolver>(_ =>
                ArenaResolver.GetLeaderboardByAvatarAddressAsync(
                    default!, default!, default!, default!, default!, default!));
    }
}
