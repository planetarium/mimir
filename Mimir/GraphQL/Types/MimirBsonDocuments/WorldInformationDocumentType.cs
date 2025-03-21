using HotChocolate.Types;
using Mimir.GraphQL.Types;
using Mimir.MongoDB.Bson;

namespace Mimir.GraphQL.Types.MimirBsonDocuments;

public class WorldInformationDocumentType : ObjectType<WorldInformationDocument>
{
    protected override void Configure(IObjectTypeDescriptor<WorldInformationDocument> descriptor)
    {
        descriptor.Name("WorldInformationRanking");
        
        descriptor.Field(f => f.Id).Type<StringType>();
        descriptor.Field(f => f.Object).Type<WorldInformationStateType>();
        
        // Add calculated field for max world ID
        descriptor
            .Field("maxWorldId")
            .Type<IntType>()
            .Resolve(context =>
            {
                var document = context.Parent<WorldInformationDocument>();
                return document.Object.WorldDictionary.Keys.Max();
            });
        
        // Add calculated field for the StageClearedId of the max world ID
        descriptor
            .Field("maxStageClearedId")
            .Type<IntType>()
            .Resolve(context =>
            {
                var document = context.Parent<WorldInformationDocument>();
                var maxWorldId = document.Object.WorldDictionary.Keys.Max();
                return document.Object.WorldDictionary[maxWorldId].StageClearedId;
            });
    }
} 