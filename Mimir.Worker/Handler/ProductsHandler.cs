using System.Text.RegularExpressions;
using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.Market;

public class ProductsHandler : BaseActionHandler
{
    public ProductsHandler(IStateService stateService, MongoDbService store)
        : base(
            stateService,
            store,
            "^register_product[0-9]*$|^cancel_product_registration[0-9]*$|^buy_product[0-9]*$"
        ) { }

    public override async Task HandleAction(
        string actionType,
        long processBlockIndex,
        Dictionary actionValues
    )
    {
        List<Address> avatarAddresses = [];
        if (Regex.IsMatch(actionType, "buy_product[0-9]*$"))
        {
            var serialized = (List)actionValues["p"];
            var productInfos = serialized
                .Cast<List>()
                .Select(ProductFactory.DeserializeProductInfo)
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

        foreach (var avatarAddress in avatarAddresses)
        {
            var productsStateAddress = ProductsState.DeriveAddress(avatarAddress);
            var productsState = await _stateGetter.GetProductsState(avatarAddress);
            await SyncWrappedProductsStateAsync(productsStateAddress, productsState);
        }
    }

    public async Task SyncWrappedProductsStateAsync(Address address, ProductsState productsState)
    {
        var productIds = productsState.ProductIds.Select(id => Guid.Parse(id.ToString())).ToList();

        var existingState = await _store.GetProductsStateByAddress(address);
        List<Guid> existingProductIds;
        if (existingState == null)
        {
            await _store.UpsertStateDataAsync(
                new StateData(address, new WrappedProductsState(address, productsState))
            );
            existingProductIds = new();
        }
        else
        {
            existingProductIds = existingState["State"]
                ["Object"]["ProductIds"]
                .AsBsonArray.Select(p => Guid.Parse(p.AsString))
                .ToList();
        }

        var productsToAdd = productIds.Except(existingProductIds).ToList();
        var productsToRemove = existingProductIds.Except(productIds).ToList();

        foreach (var productId in productsToAdd)
        {
            var product = await _stateGetter.GetProductState(productId);
            var productAddress = Product.DeriveAddress(productId);
            var stateData = new StateData(
                productAddress,
                new ProductState(productAddress, address, product)
            );
            await _store.UpsertStateDataAsync(stateData);
        }

        foreach (var productId in productsToRemove)
        {
            await _store.RemoveProduct(productId);
        }
    }
}
