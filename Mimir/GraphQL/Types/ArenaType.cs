using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Resolvers;

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
                ArenaResolver.GetArenaRound(default!, default!, default!, default!, default!));
        descriptor
            .Field("ranking")
            .Description("The avatar's ranking of the arena.")
            .Argument("avatarAddress", a => a.Type<NonNullType<AddressType>>())
            .Type<LongType>()
            .ResolveWith<ArenaResolver>(_ =>
                ArenaResolver.GetRanking(default!, default!, default!, default!, default!, default!, default!));
    }
}
