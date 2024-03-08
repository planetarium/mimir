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
            foreach (var avatarAddress in arenaParticipants.AvatarAddresses)
            {
                var arenaData = await stateGetter.GetArenaData(roundData, avatarAddress);
                var avatarData = await stateGetter.GetAvatarData(avatarAddress);

                if (arenaData != null && avatarData != null)
                {
                    await storage.AddArenaData(arenaData);
                    await storage.AddAvatarData(avatarData);
                    await storage.LinkAvatarWithArenaAsync(avatarAddress);
                }
            }
        }));
    }
}
