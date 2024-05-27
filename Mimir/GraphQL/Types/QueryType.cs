using Mimir.GraphQL.Queries;

namespace Mimir.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
    }
}
