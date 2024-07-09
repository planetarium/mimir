using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Queries;
using Mimir.Repositories;

namespace Mimir.GraphQL.Types;

public class QueryType : ObjectType<Query>
{
    protected override void Configure(IObjectTypeDescriptor<Query> descriptor)
    {
        descriptor
            .Field("agent")
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<AgentType>()
            .Resolve(context =>
            {
                return new AgentObject(context.ArgumentValue<Address>("address"));
            });

        descriptor
            .Field("avatar")
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<AvatarType>()
            .Resolve(context =>
            {
                return new AvatarObject(context.ArgumentValue<Address>("address"));
            });

        descriptor
            .Field("inventory")
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<InventoryType>()
            .Resolve(context =>
            {
                return new InventoryObject(context.ArgumentValue<Address>("address"));
            });

        descriptor
            .Field("arena")
            .Type<ArenaType>()
            .Resolve(context =>
            {
                return new ArenaObject();
            });

        descriptor
            .Field("product")
            .Argument("skip", a => a.Type<NonNullType<LongType>>())
            .Argument("limit", a => a.Type<NonNullType<IntType>>())
            .Type<ListType<ProductType>>()
            .Resolve(context =>
            {
                var repository = context.Service<ProductRepository>();
                var skip = context.ArgumentValue<long>("skip");
                var limit = context.ArgumentValue<int>("limit");

                return repository.GetProducts(skip, limit);
            });
    }
}
