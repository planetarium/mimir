using Bencodex.Types;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.Market;

public class RegisterProductHandler : BaseActionHandler
{
    public RegisterProductHandler(IStateService stateService, MongoDbService store)
        : base(stateService, store, "^register_product[0-9]*$") { }

    public override async Task HandleAction(long processBlockIndex, Dictionary actionValues)
    {
        var avatarAddress = new Address(actionValues["a"]);

        var productsState = await _stateGetter.GetProductsState(avatarAddress);

        await _store.UpsertStateDataAsync(
            new StateData(
                ProductsState.DeriveAddress(avatarAddress),
                new WrappedProductsState(ProductsState.DeriveAddress(avatarAddress), productsState)
            )
        );

        foreach (var productId in productsState.ProductIds)
        {
            var product = await _stateGetter.GetProductState(productId);

            await _store.UpsertStateDataAsync(
                new StateData(
                    Product.DeriveAddress(productId),
                    new ProductState(
                        Product.DeriveAddress(productId),
                        ProductsState.DeriveAddress(avatarAddress),
                        product
                    )
                )
            );
        }
    }
}
