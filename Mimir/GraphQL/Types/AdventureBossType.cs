using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Resolvers;
using Mimir.GraphQL.Types.AdventureBoss;

namespace Mimir.GraphQL.Types;

public class AdventureBossType : ObjectType<AdventureBossObject>
{
    protected override void Configure(IObjectTypeDescriptor<AdventureBossObject> descriptor)
    {
        descriptor
            .Field("seasonInfo")
            .Description("The season of the adventure boss")
            .Type<SeasonInfoType>()
            .ResolveWith<AdventureBossResolver>(_ =>
                AdventureBossResolver.GetSeasonInfoAsync(default!, default!, default!));
    }
}
