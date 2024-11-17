using Bencodex;
using HotChocolate;
using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Enums;
using Mimir.MongoDB.Models;
using Mimir.MongoDB.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Nekoyume.Model.Market;
using SortDirection = Mimir.MongoDB.Enums.SortDirection;

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
        productFilter.SortDirection ??= SortDirection.Ascending;

        switch (productFilter.SortBy)
        {
            case ProductSortBy.Grade:
                return SortByGrade(productFilter.SortDirection.Value, find);
            case ProductSortBy.Price:
                return SortByPrice(filter, productFilter.SortDirection.Value);
            case ProductSortBy.UnitPrice:
                return SortByUnitPrice(productFilter.ProductType, productFilter.SortDirection.Value, filter, find);
        }

        return find.AsExecutable();
    }

    private static MongoDbFindFluentExecutable<ProductDocument> SortByGrade(SortDirection sortDirection,
        IFindFluent<ProductDocument, ProductDocument> find)
    {
        var sortBuilder = Builders<ProductDocument>.Sort;
        find = sortDirection == SortDirection.Ascending
            ? find.Sort(sortBuilder.Ascending("Object.TradableItem.Grade"))
            : find.Sort(sortBuilder.Descending("Object.TradableItem.Grade"));

        return find.AsExecutable();
    }

    private MongoDbAggregateFluentExecutable<ProductDocument> SortByPrice(FilterDefinition<ProductDocument> filter,
        SortDirection sortDirection)
    {
        var convertStage = new BsonDocument("$addFields", new BsonDocument
        {
            { "convertedPrice", new BsonDocument("$toLong", "$Object.Price.MajorUnit") },
        });
        var sortStage = new BsonDocument("$sort", new BsonDocument("convertedPrice",
            sortDirection == SortDirection.Ascending ? 1 : -1));

        return _collection.Aggregate()
            .Match(filter)
            .AppendStage<ProductDocument>(convertStage)
            .AppendStage<ProductDocument>(sortStage)
            .AsExecutable();
    }

    private IExecutable<ProductDocument> SortByUnitPrice(ProductType? productType, SortDirection sortDirection,
        FilterDefinition<ProductDocument> filter, IFindFluent<ProductDocument, ProductDocument> find)
    {
        // ProductDocument.UnitPrice is null for FungibleAssetValue
        if (productType == ProductType.FungibleAssetValue)
        {
            return SortFungibleAssetValueByUnitPrice(sortDirection, filter);
        }

        find = sortDirection == SortDirection.Ascending
            ? find.SortBy(x => x.UnitPrice)
            : find.SortByDescending(x => x.UnitPrice);

        return find.AsExecutable();
    }

    private MongoDbAggregateFluentExecutable<ProductDocument> SortFungibleAssetValueByUnitPrice(SortDirection sortDirection,
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
            sortDirection == SortDirection.Ascending ? 1 : -1));

        return _collection.Aggregate()
            .Match(filter)
            .AppendStage<ProductDocument>(convertStage)
            .AppendStage<ProductDocument>(calcStage)
            .AppendStage<ProductDocument>(sortStage)
            .AsExecutable();
    }
}