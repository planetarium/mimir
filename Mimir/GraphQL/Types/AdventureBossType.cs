using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Resolvers;
using Mimir.GraphQL.Types.AdventureBoss;

namespace Mimir.GraphQL.Types;

public class AdventureBossType : ObjectType<AdventureBossObject>
{
    protected override void Configure(IObjectTypeDescriptor<AdventureBossObject> descriptor)
    {
        descriptor
            .Field("season")
            .Description("The season of the adventure boss")
            .Argument("number", a => a
                .Description("The number of season. 0 is latest season.")
                .Type<LongType>()
                .DefaultValue(0))
            .Type<SeasonInfoType>()
            .ResolveWith<AdventureBossResolver>(_ =>
                AdventureBossResolver.GetSeasonInfoAsync(default!, default!));
    }
}
