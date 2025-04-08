using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;

namespace Mimir.GraphQL.Types.MimirBsonDocuments;

public class RaidCpDocumentType : ObjectType<RaidCpDocument>
{
    protected override void Configure(IObjectTypeDescriptor<RaidCpDocument> descriptor)
    {
        descriptor
            .Field("avatar")
            .Type<ObjectType<AvatarDocument>>()
            .Resolve(async context =>
            {
                var parent = context.Parent<RaidCpDocument>();
                var avatarAddress = new Address(parent.Id);
                var avatarRepository = context.Service<IAvatarRepository>();

                try
                {
                    return await avatarRepository.GetByAddressAsync(avatarAddress);
                }
                catch
                {
                    return null;
                }
            });
    }
}
