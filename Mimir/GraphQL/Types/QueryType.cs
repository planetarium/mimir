using Mimir.GraphQL.Queries;
using Mimir.Repositories;

namespace Mimir.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        // descriptor
        //     .Field("inventory")
        //     .Argument("address", a => a.Type<NonNullType<AddressType>>())
        //     .Type<InventoryType>()
        //     .Resolve(context => new InventoryObject(context.ArgumentValue<Address>("address")));
        //
        // descriptor
        //     .Field("product")
        //     .Argument("skip", a => a.Type<NonNullType<LongType>>())
        //     .Argument("limit", a => a.Type<NonNullType<IntType>>())
        //     .Type<ListType<ProductType>>()
        //     .Resolve(context =>
        //     {
        //         var repository = context.Service<ProductRepository>();
        //         var skip = context.ArgumentValue<long>("skip");
        //         var limit = context.ArgumentValue<int>("limit");
        //
        //         return repository.GetProducts(skip, limit);
        //     });
        //
        // NOTE: Use it when Mimir.Worker handle adventure boss data into MongoDB.
        // descriptor
        //     .Field("adventureBoss")
        //     .Type<AdventureBossType>()
        //     .Resolve(_ => new AdventureBossObject());
    }
}
