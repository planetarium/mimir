using Bencodex;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume.Model.Item;
using Nekoyume.Model.Market;

namespace Mimir.MongoDB.Repositories;

public class ProductRepository
{
    private static readonly Codec Codec = new();

    private readonly IMongoCollection<ProductDocument> _collection;
    private readonly GridFSBucket _gridFsBucket;

    public ProductRepository(MongoDbService dbService)
    {
        var collectionName = CollectionNames.GetCollectionName<ProductDocument>();
        _collection = dbService.GetCollection<ProductDocument>(collectionName);
        _gridFsBucket = dbService.GetGridFs();
    }

    public IExecutable<ProductDocument> Get(ProductFilter? productFilter)
    {
        var builder = Builders<ProductDocument>.Filter;

        var filter = builder.Empty;

        if (productFilter?.ProductType is not null)
        {
            filter &= builder.Eq(x => x.Object.ProductType, productFilter.ProductType);
        }

        if (productFilter?.ItemType is not null)
        {
            filter &= builder.Eq("Object.TradableItem.ItemType", productFilter.ItemType);
        }

        if (productFilter?.ItemSubType is not null)
        {
            filter &= builder.Eq("Object.TradableItem.ItemSubType", productFilter.ItemSubType);
        }

        var find = _collection.Find(filter);

        if (productFilter?.SortBy is not null)
        {
            switch (productFilter.SortBy)
            {
                case SortBy.Class:
                    break;
                case SortBy.Cp:
                    break;
                case SortBy.Crystal:
                    break;
                case SortBy.Price:
                    return SortByPrice(filter, productFilter);
                case SortBy.UnitPrice:
                    // ProductDocument.UnitPrice is null
                    if (productFilter.ProductType == ProductType.FungibleAssetValue)
                    {
                        return SortFungibleAssetValueByUnitPrice(productFilter, filter);
                    }

                    find = productFilter.SortDirection == SortDirection.Ascending
                        ? find.SortBy(x => x.UnitPrice)
                        : find.SortByDescending(x => x.UnitPrice);
                    break;
                case null:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return find.AsExecutable();
    }

    public async Task<ProductDocument> GetByProductIdAsync(Guid productId)
    {
        var filter = Builders<ProductDocument>.Filter.Eq("Object.ProductId", productId);
        var productDocument = await _collection.Find(filter).FirstOrDefaultAsync();
        if (productDocument is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                _collection.CollectionNamespace.CollectionName,
                $"'ProductId' equals to '{productId}'");
        }

        return productDocument;
    }

    public class ProductFilter
    {
        public ProductType? ProductType { get; set; }
        public ItemType? ItemType { get; set; }
        public ItemSubType? ItemSubType { get; set; }
        public SortBy? SortBy { get; set; }
        public SortDirection? SortDirection { get; set; } = new SortDirection?(0);
    }

    public enum SortBy
    {
        Class,
        Cp,
        Crystal,
        Price,
        UnitPrice
    }

    
    private MongoDbAggregateFluentExecutable<ProductDocument> SortByPrice(FilterDefinition<ProductDocument> filter, ProductFilter productFilter)
    {
        var convertStage = new BsonDocument("$addFields", new BsonDocument
        {
            { "convertedPrice", new BsonDocument("$toLong", "$Object.Price.RawValue") },
        });
        var sortStage = new BsonDocument("$sort", new BsonDocument("convertedPrice",
            productFilter.SortDirection == SortDirection.Ascending ? 1 : -1));
        
        return _collection.Aggregate()
            .Match(filter)
            .AppendStage<ProductDocument>(convertStage)
            .AppendStage<ProductDocument>(sortStage)
            .AsExecutable();
    }

    private MongoDbAggregateFluentExecutable<ProductDocument> SortFungibleAssetValueByUnitPrice(ProductFilter productFilter, FilterDefinition<ProductDocument> filter)
    {
        var convertStage = new BsonDocument("$addFields", new BsonDocument
        {
            { "convertedPrice", new BsonDocument("$toLong", "$Object.Price.RawValue") },
            { "convertedQty", new BsonDocument("$toLong", "$Object.Asset.RawValue") },
        });
        var calcStage = new BsonDocument("$addFields", new BsonDocument
        {
            { "calcUnitPrice", new BsonDocument("$divide", new BsonArray { "$convertedPrice", "$convertedQty" }) },
        });
        var sortStage = new BsonDocument("$sort", new BsonDocument("calcUnitPrice",
            productFilter.SortDirection == SortDirection.Ascending ? 1 : -1));

        return _collection.Aggregate()
            .Match(filter)
            .AppendStage<ProductDocument>(convertStage)
            .AppendStage<ProductDocument>(calcStage)
            .AppendStage<ProductDocument>(sortStage)
            .AsExecutable();
    }
}