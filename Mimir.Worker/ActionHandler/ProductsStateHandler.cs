using System.Text.RegularExpressions;
using Bencodex.Types;
using Lib9c.Models.Market;
using Libplanet.Crypto;
using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Exceptions;
using Mimir.Worker.Services;
using MongoDB.Driver;
using Serilog;

namespace Mimir.Worker.ActionHandler;

public class ProductsStateHandler(IStateService stateService, MongoDbService store) :
    BaseActionHandler(
        stateService,
        store,
        "^register_product[0-9]*$|^cancel_product_registration[0-9]*$|^buy_product[0-9]*$|^re_register_product[0-9]*$",
        Log.ForContext<ProductsStateHandler>())
{
    protected override async Task HandleAction(
        long blockIndex,
        Address signer,
        IValue actionPlainValue,
        string actionType,
        IValue? actionPlainValueInternal,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        if (actionPlainValueInternal is not Dictionary actionValues)
        {
            throw new InvalidTypeOfActionPlainValueInternalException(
                [ValueKind.Dictionary],
                actionPlainValueInternal?.Kind);
        }

        var avatarAddresses = GetAvatarAddresses(actionType, actionValues);
        foreach (var avatarAddress in avatarAddresses)
        {
            var productsStateAddress = Nekoyume.Model.Market.ProductsState.DeriveAddress(avatarAddress);
            var productsState = await StateGetter.GetProductsState(avatarAddress, stoppingToken);
            await SyncWrappedProductsStateAsync(
                avatarAddress,
                productsStateAddress,
                productsState,
                session,
                stoppingToken);
        }
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

    public async Task SyncWrappedProductsStateAsync(
        Address avatarAddress,
        Address productsStateAddress,
        ProductsState productsState,
        IClientSessionHandle? session = null,
        CancellationToken stoppingToken = default)
    {
        await Store.UpsertStateDataManyAsync(
            CollectionNames.GetCollectionName<ProductsStateDocument>(),
            [new ProductsStateDocument(productsStateAddress, productsState, avatarAddress)],
            session,
            stoppingToken);
    }
}
