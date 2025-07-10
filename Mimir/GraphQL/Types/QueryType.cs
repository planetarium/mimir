using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Mimir.GraphQL.Models;
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
            .Field("blocks")
            .Description("Retrieves a paginated list of blocks.")
            .UseOffsetPaging<NonNullType<BlockDocumentType>>()
            .Resolve(context =>
            {
                return context.Service<IBlockRepository>().Get();
            });

        descriptor
            .Field("transactions")
            .Description("Retrieves a paginated list of transactions.")
            .UseOffsetPaging<NonNullType<TransactionDocumentType>>()
            .Resolve(context =>
            {
                return context.Service<ITransactionRepository>().Get();
            });

        descriptor
            .Field("myWorldInformationRanking")
            .Description("Get my world information ranking based on StageClearedId.")
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<UserWorldInformationRankingType>()
            .Resolve(async context =>
            {
                var address = context.ArgumentValue<Address>("address");
                var result = await context.Service<IWorldInformationRankingRepository>().GetUserWithRanking(address);
                
                if (result == null)
                    return null;
                    
                var (userDocument, rank) = result.Value;
                return new UserWorldInformationRanking 
                { 
                    UserDocument = userDocument, 
                    Rank = rank 
                };
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
            .Field("myAdventureCpRanking")
            .Description("Get my ranking based on adventure CP.")
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<UserAdventureRankingType>()
            .Resolve(async context =>
            {
                var address = context.ArgumentValue<Address>("address");
                var result = await context.Service<ICpRepository<AdventureCpDocument>>().GetUserWithRanking(address);
                
                if (result == null)
                    return null;
                    
                var (userDocument, rank) = result.Value;
                return new UserAdventureRanking
                {
                    UserDocument = userDocument,
                    Rank = rank
                };
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
            .Field("myArenaCpRanking")
            .Description("Get my ranking based on arena CP.")
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<UserArenaRankingType>()
            .Resolve(async context =>
            {
                var address = context.ArgumentValue<Address>("address");
                var result = await context.Service<ICpRepository<ArenaCpDocument>>().GetUserWithRanking(address);
                
                if (result == null)
                    return null;
                    
                var (userDocument, rank) = result.Value;
                return new UserArenaRanking
                {
                    UserDocument = userDocument,
                    Rank = rank
                };
            });

        descriptor
            .Field("raidCpRanking")
            .Description("Cp ranking of users based on their raid score.")
            .UseOffsetPaging<RaidCpDocumentType>()
            .Resolve(context =>
            {
                return context.Service<ICpRepository<RaidCpDocument>>().GetRanking();
            });

        descriptor
            .Field("myRaidCpRanking")
            .Description("Get my ranking based on raid CP.")
            .Argument("address", a => a.Type<NonNullType<StringType>>())
            .Type<UserRaidRankingType>()
            .Resolve(async context =>
            {
                var address = context.ArgumentValue<Address>("address");
                var result = await context.Service<ICpRepository<RaidCpDocument>>().GetUserWithRanking(address);
                
                if (result == null)
                    return null;
                    
                var (userDocument, rank) = result.Value;
                return new UserRaidRanking
                {
                    UserDocument = userDocument,
                    Rank = rank
                };
            });
    }
}
