using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;

namespace Mimir.GraphQL.Types.MimirBsonDocuments;

public class AdventureCpDocumentType : ObjectType<AdventureCpDocument>
{
    protected override void Configure(IObjectTypeDescriptor<AdventureCpDocument> descriptor)
    {
        descriptor.Field("avatar")
            .Type<ObjectType<AvatarDocument>>()
            .Resolve(async context =>
            {
                var parent = context.Parent<AdventureCpDocument>();
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
