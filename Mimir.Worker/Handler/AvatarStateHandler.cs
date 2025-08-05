using Bencodex.Types;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using Mimir.Worker.Client;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.Services;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AvatarStateHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IOptions<Configuration> configuration
)
    : BaseDiffHandler(
        "avatar",
        Addresses.Avatar,
        new AvatarStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        Log.ForContext<AvatarStateHandler>(),
        configuration
    )
{
    protected override async Task<MimirBsonDocument> CreateDocumentAsync(
        IStateDocumentConverter converter,
        long blockIndex,
        Address address,
        IValue rawState)
    {
        var inventoryState = await StateGetter.GetInventoryState(address, CancellationToken.None);
        var armorId = inventoryState.GetArmorId();
        var portraitId = inventoryState.GetPortraitId();

        var pair = new AddressStatePair
        {
            BlockIndex = blockIndex,
            Address = address,
            RawState = rawState,
            AdditionalData = new Dictionary<string, object>
            {
                { "armorId", armorId },
                { "portraitId", portraitId },
            },
        };
        var avatarState = converter.ConvertToDocument(pair);
        return avatarState;
    }
}
