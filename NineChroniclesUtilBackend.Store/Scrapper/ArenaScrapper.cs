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

    private readonly StateGetter _stateGetter = new StateGetter(service);
    private readonly EmptyChronicleClient _client = client;
    private readonly MongoDbStore _store = store;

    public async Task ExecuteAsync()
    {
        var latestBlock = await _client.GetLatestBlock();
        var roundData = await GetArenaRoundData(latestBlock.Index);
        var arenaParticipants = await _stateGetter.GetArenaParticipantsState(roundData.ChampionshipId, roundData.Round);

        foreach (var avatarAddress in arenaParticipants.AvatarAddresses)
        {
            var arenaData = await GetArenaData(roundData, avatarAddress);
            var avatarData = await GetAvatarData(avatarAddress);

            if (arenaData != null && avatarData != null)
            {
                await _store.AddArenaData(arenaData);
                await _store.AddAvatarData(avatarData);
                await _store.LinkAvatarWithArenaAsync(avatarAddress);
            }
        }
    }

    public async Task<ArenaSheet.RoundData> GetArenaRoundData(long index)
    {
        var arenaSheet = await _stateGetter.GetSheet<ArenaSheet>();
        var roundData = arenaSheet.GetRoundByBlockIndex(index);

        return roundData;
    }

    public async Task<ArenaData?> GetArenaData(ArenaSheet.RoundData roundData, Address avatarAddress)
    {
        try
        {
            var arenaScore = await _stateGetter.GetArenaScoreState(avatarAddress, roundData.ChampionshipId, roundData.Round);
            var arenaInfo = await _stateGetter.GetArenaInfoState(avatarAddress, roundData.ChampionshipId, roundData.Round);

            return new ArenaData(arenaScore, arenaInfo, roundData, avatarAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during GetArenaData: {ex.Message}");
            return null;
        }
    }

    public async Task<AvatarData?> GetAvatarData(Address avatarAddress)
    {
        try
        {
            var avatarState = await _stateGetter.GetAvatarState(avatarAddress);
            var avatarItemSlotState = await _stateGetter.GetItemSlotState(avatarAddress);
            var avatarRuneStates = await _stateGetter.GetRuneStates(avatarAddress);

            return new AvatarData(avatarState, avatarItemSlotState, avatarRuneStates);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during GetAvatarData: {ex.Message}");
            return null;
        }
    }
}
