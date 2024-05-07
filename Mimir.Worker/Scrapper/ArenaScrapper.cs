using Mimir.Worker.Events;
using Mimir.Worker.Services;
using Mimir.Worker.Models;
using Nekoyume.TableData;
using Libplanet.Crypto;

namespace Mimir.Worker.Scrapper;

public class ArenaScrapper(ILogger<ArenaScrapper> logger, IStateService service, MongoDbWorker worker)
{
    private readonly ILogger<ArenaScrapper> _logger = logger;

    private readonly IStateService _stateService = service;
    private readonly MongoDbWorker _worker = worker;

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var latestBlockIndex = await service.GetLatestIndex();
        var stateGetter = _stateService.At();
        var roundData = await stateGetter.GetArenaRoundData(latestBlockIndex);
        var arenaParticipants = await stateGetter.GetArenaParticipantsState(roundData.ChampionshipId, roundData.Round);

        var buffer = new List<(Address AvatarAddress, ArenaData Arena, AvatarData Avatar)>();
        const int maxBufferSize = 10;
        async Task FlushBufferAsync()
        {
            await _worker.BulkUpsertArenaDataAsync(buffer.Select(x => x.Arena).ToList());
            await _worker.BulkUpsertAvatarDataAsync(buffer.Select(x => x.Avatar).ToList());
            foreach (var pair in buffer)
            {
                await _worker.LinkAvatarWithArenaAsync(pair.AvatarAddress);
            }
            
            buffer.Clear();
        }
        
        await _worker.UpdateLatestBlockIndex(latestBlockIndex);

        foreach (var avatarAddress in arenaParticipants.AvatarAddresses)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var arenaData = await stateGetter.GetArenaData(roundData, avatarAddress);
            var avatarData = await stateGetter.GetAvatarData(avatarAddress);

            if (arenaData != null && avatarData != null)
            {
                buffer.Add((avatarAddress, arenaData, avatarData));
            }

            if (buffer.Count >= maxBufferSize)
            {
                await FlushBufferAsync();
            }
        }

        if (buffer.Count > 0)
        {
            await FlushBufferAsync();
        }
    }
}
