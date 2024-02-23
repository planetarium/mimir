using NineChroniclesUtilBackend.Store.Services;
using NineChroniclesUtilBackend.Store.Models;
using Nekoyume.TableData;
using Nekoyume.Model.Arena;
using Libplanet.Crypto;

namespace NineChroniclesUtilBackend.Store.Scrapper;

public class ArenaScrapper
{
    private StateGetter _stateGetter;
    
    public ArenaScrapper(IStateService service)
    {
        _stateGetter = new StateGetter(service);
    }

    public async Task ExecuteAsync()
    {
        var roundData = await GetArenaRoundData(900000);

        var arenaParticipants = await _stateGetter.GetArenaParticipantsState(roundData.ChampionshipId, roundData.Round);
        
        foreach(var avatarAddress in arenaParticipants.AvatarAddresses)
        {
            var arenaData = await GetArenaData(roundData, avatarAddress);
            var avataData = await GetAvatarData(avatarAddress);
            Console.WriteLine();
        }
    }

    public async Task<ArenaSheet.RoundData> GetArenaRoundData(int index)
    {
        var arenaSheet = await _stateGetter.GetSheet<ArenaSheet>();
        var roundData = arenaSheet.GetRoundByBlockIndex(index);

        return roundData;
    }

    public async Task<ArenaData> GetArenaData(ArenaSheet.RoundData roundData, Address avatarAddress)
    {
        var arenaScore = await _stateGetter.GetArenaScoreState(avatarAddress, roundData.ChampionshipId, roundData.Round);
        var arenaInfo = await _stateGetter.GetArenaInfoState(avatarAddress, roundData.ChampionshipId, roundData.Round);

        return new ArenaData(arenaScore, arenaInfo);
    }

    public async Task<AvatarData> GetAvatarData(Address avatarAddress)
    {
        var avatarState = await _stateGetter.GetAvatarState(avatarAddress);
        var avatarItemSlotState = await _stateGetter.GetItemSlotState(avatarAddress);
        var avatarRuneStates = await _stateGetter.GetRuneStates(avatarAddress);

        return new AvatarData(avatarState, avatarItemSlotState, avatarRuneStates);
    }
}
