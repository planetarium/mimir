using HeadlessGQL;
using Libplanet.Crypto;
using Mimir.Worker.Handler;
using Mimir.Worker.Models;
using Mimir.Worker.Services;
using Nekoyume.Model.State;

namespace Mimir.Worker.Scrapper;

public class DiffScrapper
{
    private readonly HeadlessGQLClient _headlessGqlClient;
    private readonly DiffMongoDbService _store;

    public DiffScrapper(HeadlessGQLClient headlessGqlClient, DiffMongoDbService store)
    {
        _headlessGqlClient = headlessGqlClient;
        _store = store;
    }

    public async Task ExecuteAsync(long baseIndex, long targetIndex)
    {
        long indexDifference = Math.Abs(targetIndex - baseIndex);

        long currentBaseIndex = baseIndex;

        while (indexDifference > 0)
        {
            long currentTargetIndex =
                currentBaseIndex + (indexDifference > 9 ? 9 : indexDifference);

            var diffResult = await _headlessGqlClient.GetDiffs.ExecuteAsync(
                currentBaseIndex,
                currentTargetIndex
            );

            if (diffResult.Data?.Diffs != null)
            {
                foreach (var diff in diffResult.Data.Diffs)
                {
                    switch (diff)
                    {
                        case IGetDiffs_Diffs_RootStateDiff rootDiff:
                            ProcessRootStateDiff(rootDiff);
                            break;

                        case IGetDiffs_Diffs_StateDiff stateDiff:
                            ProcessStateDiff(stateDiff);
                            break;
                    }
                }
            }

            currentBaseIndex = currentTargetIndex;
            indexDifference -= 9;
        }
    }

    private async void ProcessRootStateDiff(IGetDiffs_Diffs_RootStateDiff rootDiff)
    {
        var handler = AddressHandlerMappings.HandlerMappings[new Address(rootDiff.Path)];
        foreach (var subDiff in rootDiff.Diffs)
        {
            if (handler is not null)
            {
                if (subDiff.ChangedState is not null)
                {
                    var state = handler.ConvertToState(subDiff.ChangedState);
                    await _store.UpsertAvatarDataAsync(
                        new OnlyAvatarData(new Address(subDiff.Path), new AvatarState(state))
                    );
                }
            }
        }
    }

    private void ProcessStateDiff(IGetDiffs_Diffs_StateDiff stateDiff) { }
}
