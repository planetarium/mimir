using Mimir.MongoDB.Models;

namespace Mimir.GraphQL.Types;

public class DailyActiveUserType : ObjectType<DailyActiveUser>
{
    protected override void Configure(IObjectTypeDescriptor<DailyActiveUser> descriptor)
    {
        descriptor.Field(d => d.Date).Description("The date in YYYY-MM-DD format");
        descriptor.Field(d => d.Count).Description("The number of unique active users on this date");
    }
}

