using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Newtonsoft.Json;

namespace Mimir.Worker.Scrapper;

public class DiffScrapper
{
    private readonly ILogger<DiffScrapper> _logger;
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly DiffMongoDbService _store;

    public DiffScrapper(
        ILogger<DiffScrapper> logger,
        HeadlessGQLClient headlessGqlClient,
        DiffMongoDbService store
    )
    {
        _logger = logger;
        _headlessGqlClient = headlessGqlClient;
        _store = store;
    }

    public async Task ExecuteAsync(long baseIndex, long targetIndex)
    {
        var diffResult = await _headlessGqlClient.GetDiffs.ExecuteAsync(baseIndex, targetIndex);

        foreach (var rootDiffs in diffResult.Data.Diffs)
        {
            var handler = AddressHandlerMappings.HandlerMappings[new Address(rootDiffs.Path)];
            if (handler is not null)
            {
                foreach (var diff in rootDiffs.Diffs)
                {
                    var state = handler.ConvertToState(diff.ChangedState);
                    await _store.BulkUpsertAvatarDataAsync(
                        new List<AvatarData>() { new AvatarData(state) }
                    );
                }
            }
        }
    }
}
