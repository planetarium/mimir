using Mimir.MongoDB.Bson;
using Lib9c.Models.States;
using Mimir.MongoDB;
using Mimir.Worker.CollectionUpdaters;
using Mimir.Worker.Services;
using MongoDB.Bson;
using MongoDB.Driver;
using Nekoyume.TableData;
using Serilog;

namespace Mimir.Worker.Initializer;

public class ArenaInitializer(IStateService stateService, MongoDbService dbService)
    : BaseInitializer(stateService, dbService, Log.ForContext<ArenaInitializer>())
{
    public override async Task RunAsync(CancellationToken stoppingToken)
    {
        var stateGetter = _stateService.At();
        var blockIndex = await _stateService.GetLatestIndex();
        var arenaSheet = await _store.GetSheetAsync<ArenaSheet>();
        if (arenaSheet is null)
        {
            // If arena sheet is null, we can't initialize
            return;
        }

        var roundData = arenaSheet.GetRoundByBlockIndex(blockIndex);

        var arenaParticipants = await stateGetter.GetArenaParticipantsState(
            roundData.ChampionshipId,
            roundData.Round
        );

        foreach (var avatarAddress in arenaParticipants.AvatarAddresses)
        {
            var arenaScore = await stateGetter.GetArenaScoreAsync(
                avatarAddress,
                roundData.ChampionshipId,
                roundData.Round,
                stoppingToken
            );
            var arenaInfo = await stateGetter.GetArenaInformationAsync(
                avatarAddress,
                roundData.ChampionshipId,
                roundData.Round,
                stoppingToken
            );

            var avatarState = await stateGetter.GetAvatarState(avatarAddress, stoppingToken);
            var simpleAvatarState = SimplifiedAvatarState.FromAvatarState(avatarState);

            _logger.Information("Init arena, address: {AvatarAddress}", avatarAddress);
            await _store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AgentDocument>(),
                [
                    ArenaCollectionUpdater.UpsertAsync(
                        simpleAvatarState,
                        arenaScore,
                        arenaInfo,
                        avatarAddress,
                        roundData.ChampionshipId,
                        roundData.Round
                    )
                ],
                null,
                stoppingToken
            );
            await _store.UpsertStateDataManyAsync(
                CollectionNames.GetCollectionName<AvatarDocument>(),
                [AvatarCollectionUpdater.UpsertAsync(avatarState)],
                null,
                stoppingToken
            );
        }
    }

    public override async Task<bool> IsInitialized()
    {
        var blockIndex = await _stateService.GetLatestIndex();

        var arenaSheet = await _store.GetSheetAsync<ArenaSheet>();
        if (arenaSheet is null)
        {
            // If arena sheet is null, we can't initialize
            return true;
        }

        var roundData = arenaSheet.GetRoundByBlockIndex(blockIndex);
        var count = await GetArenaDocumentCount(roundData.ChampionshipId, roundData.Round);
        return count != 0;
    }

    private async Task<long> GetArenaDocumentCount(int championshipId, int round)
    {
        var builder = Builders<BsonDocument>.Filter;
        var filter = builder.Eq("ChampionshipId", championshipId);
        filter &= builder.Eq("Round", round);
        return await _store.GetCollection<ArenaDocument>().Find(filter).CountDocumentsAsync();
    }
}
