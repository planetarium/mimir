using HotChocolate.Data;
using HotChocolate.Data.MongoDb;
using Libplanet.Crypto;
using Mimir.MongoDB.Bson;
using Mimir.MongoDB.Enums;
using Mimir.MongoDB.Exceptions;
using Mimir.MongoDB.Models;
using Mimir.MongoDB.Services;
using MongoDB.Driver;
using System.Text.Json;
using HotChocolate;
using SortDirection = Mimir.MongoDB.Enums.SortDirection;

namespace Mimir.MongoDB.Repositories;

public interface IInfiniteTowerInfoRepository
{
    Task<InfiniteTowerInfoDocument> GetByAvatarAddressAndTowerIdAsync(
        Address avatarAddress,
        int infiniteTowerId
    );
    IExecutable<InfiniteTowerInfoDocument> Get(InfiniteTowerInfoFilter? filter);
}

public class InfiniteTowerInfoRepository(IMongoDbService dbService) : IInfiniteTowerInfoRepository
{
    private readonly IMongoCollection<InfiniteTowerInfoDocument> _collection = dbService
        .GetCollection<InfiniteTowerInfoDocument>(
            CollectionNames.GetCollectionName<InfiniteTowerInfoDocument>()
        );

    public async Task<InfiniteTowerInfoDocument> GetByAvatarAddressAndTowerIdAsync(
        Address avatarAddress,
        int infiniteTowerId
    )
    {
        var collectionName = CollectionNames.GetCollectionName<InfiniteTowerInfoDocument>();
        var collection = dbService.GetCollection<InfiniteTowerInfoDocument>(collectionName);
        // Document ID format is {AvatarAddress.ToHex()}_{InfiniteTowerId},
        // but using field-based search to avoid case sensitivity issues
        var id = $"{avatarAddress.ToHex()}_{infiniteTowerId}";
        var filter = Builders<InfiniteTowerInfoDocument>.Filter.Eq("_id", id);
        var document = await collection.Find(filter).FirstOrDefaultAsync();
        if (document is null)
        {
            throw new DocumentNotFoundInMongoCollectionException(
                collection.CollectionNamespace.CollectionName,
                $"'AvatarAddress' equals to '{avatarAddress.ToHex()}' and 'InfiniteTowerId' equals to '{infiniteTowerId}'"
            );
        }

        return document;
    }

    public IExecutable<InfiniteTowerInfoDocument> Get(InfiniteTowerInfoFilter? filter)
    {
        var mongoFilter = BuildFilter(filter);
        var find = _collection.Find(mongoFilter);

        if (filter?.SortBy is not null)
        {
            return ApplySorting(filter, find);
        }

        // Default sort: ClearedFloor descending
        var sortBuilder = Builders<InfiniteTowerInfoDocument>.Sort;
        find = find.Sort(sortBuilder.Descending("Object.ClearedFloor"));

        return find.AsExecutable();
    }

    private static FilterDefinition<InfiniteTowerInfoDocument> BuildFilter(
        InfiniteTowerInfoFilter? filter
    )
    {
        var filterBuilder = Builders<InfiniteTowerInfoDocument>.Filter;
        var mongoFilter = filterBuilder.Empty;

        if (filter is not null)
        {
            mongoFilter &= filterBuilder.Eq(x => x.InfiniteTowerId, filter.InfiniteTowerId);
        }

        return mongoFilter;
    }

    private IExecutable<InfiniteTowerInfoDocument> ApplySorting(
        InfiniteTowerInfoFilter filter,
        IFindFluent<InfiniteTowerInfoDocument, InfiniteTowerInfoDocument> find
    )
    {
        filter.SortDirection ??= SortDirection.Descending;
        var sortBuilder = Builders<InfiniteTowerInfoDocument>.Sort;
        var isAscending = filter.SortDirection == SortDirection.Ascending;

        find = filter.SortBy switch
        {
            InfiniteTowerInfoSortBy.ClearedFloor
                => isAscending
                    ? find.Sort(sortBuilder.Ascending("Object.ClearedFloor"))
                    : find.Sort(sortBuilder.Descending("Object.ClearedFloor")),
            InfiniteTowerInfoSortBy.RemainingTickets
                => isAscending
                    ? find.Sort(sortBuilder.Ascending("Object.RemainingTickets"))
                    : find.Sort(sortBuilder.Descending("Object.RemainingTickets")),
            InfiniteTowerInfoSortBy.TotalTicketsUsed
                => isAscending
                    ? find.Sort(sortBuilder.Ascending("Object.TotalTicketsUsed"))
                    : find.Sort(sortBuilder.Descending("Object.TotalTicketsUsed")),
            InfiniteTowerInfoSortBy.NumberOfTicketPurchases
                => isAscending
                    ? find.Sort(sortBuilder.Ascending("Object.NumberOfTicketPurchases"))
                    : find.Sort(sortBuilder.Descending("Object.NumberOfTicketPurchases")),
            _ => find.Sort(sortBuilder.Descending("Object.ClearedFloor")),
        };

        return find.AsExecutable();
    }
}
