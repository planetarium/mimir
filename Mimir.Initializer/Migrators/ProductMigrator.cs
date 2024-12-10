using Mimir.MongoDB;
using Mimir.MongoDB.Bson;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Nekoyume.Model.Market;
using ItemProduct = Lib9c.Models.Market.ItemProduct;

namespace Mimir.Initializer.Migrators;

public class ProductMigrator
{
    private readonly MongoDbService _dbService;
    private readonly IItemProductCalculationService _itemProductCalculationService;

    public ProductMigrator(MongoDbService dbService, IItemProductCalculationService itemProductCalculationService)
    {
        _dbService = dbService;
        _itemProductCalculationService = itemProductCalculationService;
    }

    public async Task AddCpAndCrystalsToProduct(CancellationToken cancellationToken)
    {
        var collectionName = CollectionNames.GetCollectionName<ProductDocument>();
        var collection = _dbService.GetCollection(collectionName);

        var builder = new FilterDefinitionBuilder<BsonDocument>();
        var filter = builder.Not(builder.Exists(nameof(ProductDocument.Crystal)));
        filter |= builder.Not(builder.Exists(nameof(ProductDocument.CombatPoint)));
        filter = builder.And(filter, builder.Eq("Object.ProductType", Enum.GetName(ProductType.NonFungible)));

        var asyncCursor = await collection.FindAsync(filter, cancellationToken: cancellationToken);
        var bsonDocs = await asyncCursor.ToListAsync(cancellationToken: cancellationToken);

        var updateDocuments = new List<WriteModel<BsonDocument>>();
        Console.WriteLine($"Updating {bsonDocs.Count} ProductDocuments with CombatPoints and Crystals");
        for (var index = 0; index < bsonDocs.Count; index++)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            var productDocument = BsonSerializer.Deserialize<ProductDocument>(bsonDocs[index]);
            if (productDocument?.Object is not ItemProduct itemProduct)
            {
                continue;
            }

            var (crystal, crystalPerPrice) = await _itemProductCalculationService.CalculateCrystalMetricsAsync(itemProduct);
            var cp = await _itemProductCalculationService.CalculateCombatPointAsync(itemProduct);

            var newProductDocument = productDocument with { Crystal = crystal, CrystalPerPrice = crystalPerPrice, CombatPoint = cp };
            updateDocuments.Add(newProductDocument.ToUpdateOneModel());

            Console.Write($"\rUpdated of {index + 1} of {bsonDocs.Count}.");
        }

        Console.WriteLine($"\nSaving changed models to {collectionName}.");

        await _dbService.UpsertStateDataManyAsync(
            collectionName,
            updateDocuments,
            cancellationToken: cancellationToken);
    }
}