using Bencodex;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Enums;
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
        var filter = BuildFilter(productFilter);

        var find = _collection.Find(filter);

        if (productFilter?.SortBy is not null)
        {
            return ApplySorting(productFilter, filter, find);
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
        public ProductSortBy? SortBy { get; set; }
        public Enums.SortDirection? SortDirection { get; set; } = Enums.SortDirection.Ascending;
    }

    private static FilterDefinition<ProductDocument> BuildFilter(ProductFilter? productFilter)
    {
        var filterBuilder = Builders<ProductDocument>.Filter;
        var filter = filterBuilder.Empty;

        if (productFilter?.ProductType is not null)
        {
            filter &= filterBuilder.Eq(x => x.Object.ProductType, productFilter.ProductType);
        }

        if (productFilter?.ItemType is not null)
        {
            filter &= filterBuilder.Eq("Object.TradableItem.ItemType", productFilter.ItemType.ToString());
        }

        if (productFilter?.ItemSubType is not null)
        {
            filter &= filterBuilder.Eq("Object.TradableItem.ItemSubType", productFilter.ItemSubType.ToString());
        }

        return filter;
    }

    private IExecutable<ProductDocument> ApplySorting(ProductFilter productFilter, FilterDefinition<ProductDocument> filter,
        IFindFluent<ProductDocument, ProductDocument> find)
    {
        productFilter.SortDirection ??= Enums.SortDirection.Ascending;

        switch (productFilter.SortBy)
        {
            case ProductSortBy.Cp:
                break;
            case ProductSortBy.Crystal:
                break;
            case ProductSortBy.Grade:
                return SortByGrade(productFilter, find);
            case ProductSortBy.Price:
                return SortByPrice(filter, productFilter);
            case ProductSortBy.UnitPrice:
                return SortByUnitPrice(productFilter, filter, find);
        }

        return find.AsExecutable();
    }

    private static MongoDbFindFluentExecutable<ProductDocument> SortByGrade(ProductFilter productFilter,
        IFindFluent<ProductDocument, ProductDocument> find)
    {
        var sortBuilder = Builders<ProductDocument>.Sort;
        find = productFilter.SortDirection == Enums.SortDirection.Ascending
            ? find.Sort(sortBuilder.Ascending("Object.TradableItem.Grade"))
            : find.Sort(sortBuilder.Descending("Object.TradableItem.Grade"));

        return find.AsExecutable();
    }

    private MongoDbAggregateFluentExecutable<ProductDocument> SortByPrice(FilterDefinition<ProductDocument> filter,
        ProductFilter productFilter)
    {
        var convertStage = new BsonDocument("$addFields", new BsonDocument
        {
            { "convertedPrice", new BsonDocument("$toLong", "$Object.Price.MajorUnit") },
        });
        var sortStage = new BsonDocument("$sort", new BsonDocument("convertedPrice",
            productFilter.SortDirection == Enums.SortDirection.Ascending ? 1 : -1));

        return _collection.Aggregate()
            .Match(filter)
            .AppendStage<ProductDocument>(convertStage)
            .AppendStage<ProductDocument>(sortStage)
            .AsExecutable();
    }

    private IExecutable<ProductDocument> SortByUnitPrice(ProductFilter productFilter, FilterDefinition<ProductDocument> filter, IFindFluent<ProductDocument, ProductDocument> find)
    {
        // ProductDocument.UnitPrice is null for FungibleAssetValue
        if (productFilter.ProductType == ProductType.FungibleAssetValue)
        {
            return SortFungibleAssetValueByUnitPrice(productFilter, filter);
        }

        find = productFilter.SortDirection == Enums.SortDirection.Ascending
            ? find.SortBy(x => x.UnitPrice)
            : find.SortByDescending(x => x.UnitPrice);

        return find.AsExecutable();
    }

    private MongoDbAggregateFluentExecutable<ProductDocument> SortFungibleAssetValueByUnitPrice(ProductFilter productFilter,
        FilterDefinition<ProductDocument> filter)
    {
        var convertStage = new BsonDocument("$addFields", new BsonDocument
        {
            { "convertedPrice", new BsonDocument("$toLong", "$Object.Price.MajorUnit") },
            { "convertedQty", new BsonDocument("$toLong", "$Object.Asset.MajorUnit") },
        });
        var calcStage = new BsonDocument("$addFields", new BsonDocument
        {
            { "calcUnitPrice", new BsonDocument("$divide", new BsonArray { "$convertedPrice", "$convertedQty" }) },
        });
        var sortStage = new BsonDocument("$sort", new BsonDocument("calcUnitPrice",
            productFilter.SortDirection == Enums.SortDirection.Ascending ? 1 : -1));

        return _collection.Aggregate()
            .Match(filter)
            .AppendStage<ProductDocument>(convertStage)
            .AppendStage<ProductDocument>(calcStage)
            .AppendStage<ProductDocument>(sortStage)
            .AsExecutable();
    }
}