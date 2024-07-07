using Lib9c.GraphQL.Enums;
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
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<AgentType>()
            .Resolve(context =>
            {
                context.ScopedContextData = context.ScopedContextData
                    .Add("planetName", context.ArgumentValue<PlanetName>("planetName"));
                return new AgentObject(context.ArgumentValue<Address>("address"));
            });

        descriptor
            .Field("avatar")
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<AvatarType>()
            .Resolve(context =>
            {
                context.ScopedContextData = context.ScopedContextData
                    .Add("planetName", context.ArgumentValue<PlanetName>("planetName"));
                return new AvatarObject(context.ArgumentValue<Address>("address"));
            });

        descriptor
            .Field("inventory")
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Argument("address", a => a.Type<NonNullType<AddressType>>())
            .Type<InventoryType>()
            .Resolve(context =>
            {
                context.ScopedContextData = context.ScopedContextData
                    .Add("planetName", context.ArgumentValue<PlanetName>("planetName"));
                return new InventoryObject(context.ArgumentValue<Address>("address"));
            });

        descriptor
            .Field("arena")
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Type<ArenaType>()
            .Resolve(context =>
            {
                context.ScopedContextData = context.ScopedContextData
                    .Add("planetName", context.ArgumentValue<PlanetName>("planetName"));
                return new ArenaObject();
            });

        descriptor
            .Field("product")
            .Argument("planetName", a => a.Type<NonNullType<EnumType<PlanetName>>>())
            .Argument("skip", a => a.Type<NonNullType<LongType>>())
            .Argument("limit", a => a.Type<NonNullType<IntType>>())
            .Type<ListType<ProductType>>()
            .Resolve(context =>
            {
                var repository = context.Service<ProductRepository>();
                var planetName = context.ArgumentValue<PlanetName>("planetName");
                var skip = context.ArgumentValue<long>("skip");
                var limit = context.ArgumentValue<int>("limit");

                return repository.GetProducts(planetName, skip, limit);
            });
    }
}
