using Lib9c.GraphQL.Types;
using Mimir.Models.AdventureBoss;

namespace Mimir.GraphQL.Types.AdventureBoss;

public class SeasonInfoType : ObjectType<SeasonInfo>
{
    protected override void Configure(IObjectTypeDescriptor<SeasonInfo> descriptor)
    {
        descriptor
            .Field(f => f.Address)
            .Type<AddressType>();
    }
}
