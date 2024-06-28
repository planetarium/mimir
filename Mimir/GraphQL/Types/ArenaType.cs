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
    }
}
