using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Market;
using Libplanet.Crypto;
using Microsoft.Extensions.Options;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using Mimir.Shared.Client;
using Mimir.Shared.Constants;
using Mimir.Shared.Services;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Initializer.Manager;
using MongoDB.Bson;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ProductsStateHandler(
    IStateService stateService,
    IMongoDbService store,
    IHeadlessGQLClient headlessGqlClient,
    IInitializerManager initializerManager,
    IStateGetterService stateGetterService
)
    : BaseActionHandler<ProductsStateDocument>(
        stateService,
        store,
        headlessGqlClient,
        initializerManager,
        "^register_product[0-9]*$|^cancel_product_registration[0-9]*$|^buy_product[0-9]*$|^re_register_product[0-9]*$",
        Log.ForContext<ProductsStateHandler>(),
        stateGetterService
    )
{
    protected override async Task<IEnumerable<WriteModel<BsonDocument>>> HandleActionAsync(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default
    )
    {
        if (actionPlainValueInternal is not Dictionary actionValues)
        {
            throw new InvalidTypeOfActionPlainValueInternalException(
                [ValueKind.Dictionary],
                actionPlainValueInternal?.Kind
            );
        }

        var avatarAddresses = GetAvatarAddresses(actionType, actionValues);
        var ops = new List<WriteModel<BsonDocument>>();
        foreach (var avatarAddress in avatarAddresses)
        {
            var productsStateAddress = Nekoyume.Model.Market.ProductsState.DeriveAddress(
                avatarAddress
            );
            var productsState = await StateGetter.GetProductsState(avatarAddress, stoppingToken);
            ops.AddRange(
                SyncWrappedProductsStateAsync(
                    blockIndex,
                    avatarAddress,
                    productsStateAddress,
                    productsState
                )
            );
        }

        return ops;
    }

    private static List<Address> GetAvatarAddresses(string actionType, Dictionary actionValues)
    {
        var avatarAddresses = new List<Address>();

        if (Regex.IsMatch(actionType, "^buy_product[0-9]*$"))
        {
            var serialized = (List)actionValues["p"];
            var productInfos = serialized
                .Cast<List>()
                .Select(Nekoyume.Model.Market.ProductFactory.DeserializeProductInfo)
                .ToList();
            foreach (var productInfo in productInfos)
            {
                avatarAddresses.Add(productInfo.AvatarAddress);
            }
        }
        else
        {
            avatarAddresses.Add(new Address(actionValues["a"]));
        }

        return avatarAddresses;
    }

    public IEnumerable<WriteModel<BsonDocument>> SyncWrappedProductsStateAsync(
        long blockIndex,
        Address avatarAddress,
        Address productsStateAddress,
        ProductsState productsState
    )
    {
        return
        [
            new ProductsStateDocument(
                blockIndex,
                productsStateAddress,
                productsState,
                avatarAddress
            ).ToUpdateOneModel(),
        ];
    }
}
