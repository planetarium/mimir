using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Repositories;

namespace Mimir.GraphQL.Types.MimirBsonDocuments;

public class WorldInformationDocumentType : ObjectType<WorldInformationDocument>
{
    protected override void Configure(IObjectTypeDescriptor<WorldInformationDocument> descriptor)
    {
        descriptor
            .Field("avatar")
            .Type<ObjectType<AvatarDocument>>()
            .Resolve(async context =>
            {
                var parent = context.Parent<WorldInformationDocument>();
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
