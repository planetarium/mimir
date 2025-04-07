using Mimir.GraphQL.Queries;
using Mimir.GraphQL.Types.MimirBsonDocuments;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Models;
using Mimir.MongoDB.Repositories;

namespace Mimir.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("products")
            .Description("Retrieves a list of market products.")
            .Argument("filter", a => a.Type<ProductFilterInputType>())
            .UseOffsetPaging<ProductDocumentType>()
            .Resolve(context =>
            {
                var productFilter = context.ArgumentValue<ProductFilter?>("filter");
                return context.Service<IProductRepository>().Get(productFilter);
            });

        descriptor
            .Field("worldInformationRanking")
            .Description("Retrieves a ranking of users based on their highest StageClearedId in the last world.")
            .UseOffsetPaging<WorldInformationDocumentType>()
            .Resolve(context =>
            {
                return context.Service<IWorldInformationRankingRepository>().GetRanking();
            });

        descriptor
            .Field("adventureCpRanking")
            .Description("Cp ranking of users based on their adventure score.")
            .UseOffsetPaging<AdventureCpDocumentType>()
            .Resolve(context =>
            {
                return context.Service<ICpRepository<AdventureCpDocument>>().GetRanking();
            });

        descriptor
            .Field("arenaCpRanking")
            .Description("Cp ranking of users based on their arena score.")
            .UseOffsetPaging<ArenaCpDocumentType>()
            .Resolve(context =>
            {
                return context.Service<ICpRepository<ArenaCpDocument>>().GetRanking();
            });

        descriptor
            .Field("raidCpRanking")
            .Description("Cp ranking of users based on their raid score.")
            .UseOffsetPaging<RaidCpDocumentType>()
            .Resolve(context =>
            {
                return context.Service<ICpRepository<RaidCpDocument>>().GetRanking();
            });
    }
}
