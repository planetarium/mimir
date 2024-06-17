using Lib9c.GraphQL.Types;
using Mimir.GraphQL.Objects;
using Mimir.GraphQL.Resolvers;

namespace Mimir.GraphQL.Types;

public class AgentType : ObjectType<AgentObject>
{
    protected override void Configure(IObjectTypeDescriptor<AgentObject> descriptor)
    {
        descriptor
            .Field(f => f.Address)
            .Type<NonNullType<AddressType>>();
        descriptor
            .Field("version")
            .Type<IntType>()
            .ResolveWith<AgentResolver>(_ =>
                AgentResolver.GetVersion(default!, default!, default!, default!, default!));
        descriptor
            .Field("avatarAddresses")
            .Type<ListType<NonNullType<AddressType>>>()
            .ResolveWith<AgentResolver>(_ =>
                AgentResolver.GetAvatarAddresses(default!, default!, default!, default!, default!));
        descriptor
            .Field("monsterCollectionRound")
            .Type<IntType>()
            .ResolveWith<AgentResolver>(_ =>
                AgentResolver.GetMonsterCollectionRound(default!, default!, default!, default!, default!));
        descriptor
            .Field("avatar")
            .Argument("index", a => a.Type<NonNullType<IntType>>())
            .Type<AvatarType>()
            .ResolveWith<AgentResolver>(_ =>
                AgentResolver.GetAvatar(default!, default!));
        descriptor
            .Field("avatars")
            .Type<NonNullType<ListType<NonNullType<AvatarType>>>>()
            .ResolveWith<AgentResolver>(_ =>
                AgentResolver.GetAvatars(default!));
    }
}
