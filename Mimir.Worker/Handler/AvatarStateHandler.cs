using Bencodex.Types;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.Initializer.Manager;
using Mimir.Worker.StateDocumentConverter;
using Nekoyume;
using Serilog;

namespace Mimir.Worker.Handler;

public sealed class AvatarStateHandler(
    IMongoDbService dbService,
    IStateService stateService,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetter
)
    : BaseDiffHandler(
        "avatar",
        Addresses.Avatar,
        new AvatarStateDocumentConverter(),
        dbService,
        stateService,
        headlessGqlClient,
        initializerManager,
        stateGetter,
        Log.ForContext<AvatarStateHandler>()
    )
{
    protected override async Task<MimirBsonDocument> CreateDocumentAsync(
        IStateDocumentConverter converter,
        long blockIndex,
        Address address,
        IValue rawState
    )
    {
        var inventoryState = await stateGetter.GetInventoryState(address, CancellationToken.None);
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
