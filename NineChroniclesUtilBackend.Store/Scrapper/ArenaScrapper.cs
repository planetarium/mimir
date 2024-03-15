using NineChroniclesUtilBackend.Store.Events;
using NineChroniclesUtilBackend.Store.Services;
using NineChroniclesUtilBackend.Store.Models;
using NineChroniclesUtilBackend.Store.Client;
using Nekoyume.TableData;
using Libplanet.Crypto;

namespace NineChroniclesUtilBackend.Store.Scrapper;

public class ArenaScrapper(ILogger<ArenaScrapper> logger, IStateService service, EmptyChronicleClient client, MongoDbStore store)
{
    private readonly ILogger<ArenaScrapper> _logger = logger;

    private readonly IStateService _stateService = service;
    private readonly EmptyChronicleClient _client = client;
    private readonly MongoDbStore _store = store;

    public async Task ExecuteAsync()
    {
        var latestBlock = await _client.GetLatestBlock();
        var stateGetter = _stateService.At(latestBlock.Index);
        var roundData = await stateGetter.GetArenaRoundData(latestBlock.Index);
        var arenaParticipants = await stateGetter.GetArenaParticipantsState(roundData.ChampionshipId, roundData.Round);

        await _store.WithTransaction((async (storage, ct) =>
        {
            var buffer = new List<(Address AvatarAddress, ArenaData Arena, AvatarData Avatar)>();
            const int maxBufferSize = 50;
            async Task FlushBufferAsync()
            {
                await storage.AddArenaData(buffer.Select(x => x.Arena).ToList());
                await storage.AddAvatarData(buffer.Select(x => x.Avatar).ToList());
                foreach (var pair in buffer)
                {
                    await storage.LinkAvatarWithArenaAsync(pair.AvatarAddress);
                }
                
                buffer.Clear();
            }
            
            await storage.UpdateLatestBlockIndex(latestBlock.Index);

            foreach (var avatarAddress in arenaParticipants.AvatarAddresses)
            {
                var arenaData = await stateGetter.GetArenaData(roundData, avatarAddress);
                var avatarData = await stateGetter.GetAvatarData(avatarAddress);

                if (arenaData != null && avatarData != null)
                {
                    buffer.Add((arenaData, avatarData));
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
        }));
    }
}
