using Lib9c.GraphQL.Types;
using Libplanet.Crypto;
using Mimir.GraphQL.Objects;
using Nekoyume;

namespace Mimir.GraphQL.Types;

public class AgentType : ObjectType<AgentObject>
{
    protected override void Configure(IObjectTypeDescriptor<AgentObject> descriptor)
    {
        descriptor
            .Field(f => f.Address)
            .Type<NonNullType<AddressType>>();
        descriptor
            .Field("avatar")
            .Argument("index", a => a.Type<NonNullType<IntType>>())
            .Type<AvatarType>()
            .Resolve(context =>
            {
                var agentAddress = context.Parent<AgentObject>().Address;
                var index = context.ArgumentValue<int>("index");
                Address avatarAddress;
                try
                {
                    avatarAddress = Addresses.GetAvatarAddress(agentAddress, index);
                }
                catch (ArgumentOutOfRangeException)
                {
                    return null;
                }

                return new AvatarObject(avatarAddress);
            });
        descriptor
            .Field("avatars")
            .Type<NonNullType<ListType<NonNullType<AvatarType>>>>()
            .Resolve(context =>
            {
                var agentAddress = context.Parent<AgentObject>().Address;
                return Enumerable.Range(0, GameConfig.SlotCount)
                    .Select(i => Addresses.GetAvatarAddress(agentAddress, i))
                    .Select(avatarAddress => new AvatarObject(avatarAddress))
                    .ToList();
            });
    }
}
