using NineChroniclesUtilBackend.Store.Events;
using NineChroniclesUtilBackend.Store.Services;
using NineChroniclesUtilBackend.Store.Models;
using Nekoyume.TableData;
using Libplanet.Crypto;

namespace NineChroniclesUtilBackend.Store.Scrapper;

public class ArenaScrapper
{
    private readonly ILogger<ArenaScrapper> _logger;
    public readonly ScrapperResult Result = new ScrapperResult();

    private StateGetter _stateGetter;
    public event EventHandler<ArenaDataCollectedEventArgs> OnDataCollected;
    
    public ArenaScrapper(ILogger<ArenaScrapper> logger, IStateService service)
    {
        _stateGetter = new StateGetter(service);
        _logger = logger;
    }

    protected virtual void RaiseDataCollected(ArenaData arenaData, AvatarData avatarData)
    {
        OnDataCollected?.Invoke(this, new ArenaDataCollectedEventArgs(arenaData, avatarData));
    }

    public async Task ExecuteAsync()
    {
        Result.StartTime = DateTime.UtcNow;

        var roundData = await GetArenaRoundData(900000);

        var arenaParticipants = await _stateGetter.GetArenaParticipantsState(roundData.ChampionshipId, roundData.Round);
        
        foreach(var avatarAddress in arenaParticipants.AvatarAddresses)
        {
            var arenaData = await GetArenaData(roundData, avatarAddress);
            var avatarData = await GetAvatarData(avatarAddress);

            if (arenaData != null && avatarData != null)
            {
                RaiseDataCollected(arenaData, avatarData);
            }
        }
        
        Result.TotalElapsedMinutes = DateTime.UtcNow.Subtract(Result.StartTime).Minutes;
    }

    public async Task<ArenaSheet.RoundData> GetArenaRoundData(int index)
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

            Result.ArenaScrappedCount += 1;
            return new ArenaData(arenaScore, arenaInfo, roundData, avatarAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during GetArenaData: {ex.Message}");
            Result.FailedArenaAddresses.Add(avatarAddress);
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

            Result.AvatarScrappedCount += 1;
            return new AvatarData(avatarState, avatarItemSlotState, avatarRuneStates);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during GetAvatarData: {ex.Message}");
            Result.FailedAvatarAddresses.Add(avatarAddress);
            return null;
        }
    }
}
