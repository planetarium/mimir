using HotChocolate.Types;
using Lib9c.Models.Block;

namespace Lib9c.GraphQL.Types;

public class ActionType : ObjectType<Lib9c.Models.Block.Action>
{
    protected override void Configure(IObjectTypeDescriptor<Lib9c.Models.Block.Action> descriptor)
    {
        descriptor
            .Field(f => f.Raw)
            .Type<StringType>();

        descriptor
            .Field(f => f.TypeId)
            .Type<StringType>();

        descriptor
            .Field(f => f.Values)
            .Type<BsonDocumentType>()
            .Description("Dynamic action values as BSON document");
    }
} 