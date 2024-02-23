using NineChroniclesUtilBackend.Store.Services;
using Libplanet.Crypto;
using Bencodex.Types;
using Nekoyume.TableData;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.Model.Arena;

namespace NineChroniclesUtilBackend.Store.Scrapper;

public class StateGetter
{
    private readonly IStateService _service;
    
    public StateGetter(IStateService service)
    {
        _service = service;
    }

    public async Task<T> GetSheet<T>()
        where T : ISheet, new()
    {
        var sheetState = await _service.GetState(Addresses.TableSheet.Derive(typeof(T).Name));
        if (sheetState is not Text sheetValue)
        {
            throw new ArgumentException(nameof(T));
        }

        var sheet = new T();
        sheet.Set(sheetValue.Value);
        return sheet;
    }

    public async Task<ArenaParticipants> GetArenaParticipantsState(int championshipId, int roundId)
    {
        var arenaParticipantsAddress =
            ArenaParticipants.DeriveAddress(championshipId, roundId);
        var state = await _service.GetState(arenaParticipantsAddress);
        return state switch
        {
            List list => new ArenaParticipants(list),
            _ => throw new ArgumentException(nameof(arenaParticipantsAddress))
        };
    }

    public async Task<ArenaScore> GetArenaScoreState(Address avatarAddress, int championshipId, int roundId)
    {
        var arenaScoreAddress =
            ArenaScore.DeriveAddress(avatarAddress, championshipId, roundId);
        var state = await _service.GetState(arenaScoreAddress);
        return state switch
        {
            List list => new ArenaScore(list),
            _ => throw new ArgumentException(nameof(arenaScoreAddress))
        };
    }

    public async Task<ArenaInformation> GetArenaInfoState(Address avatarAddress, int championshipId, int roundId)
    {
        var arenaInfoAddress =
            ArenaInformation.DeriveAddress(avatarAddress, championshipId, roundId);
        var state = await _service.GetState(arenaInfoAddress);
        return state switch
        {
            List list => new ArenaInformation(list),
            _ => throw new ArgumentException(nameof(arenaInfoAddress))
        };
    }

    public async Task<AvatarState> GetAvatarState(Address avatarAddress)
    {
        var state = await _service.GetState(avatarAddress);
        if (state is not Dictionary dictionary)
        {
            throw new ArgumentException(nameof(avatarAddress));
        }

        var inventoryAddress = avatarAddress.Derive("inventory");
        var inventoryState = await _service.GetState(inventoryAddress);
        if (inventoryState is not List list)
        {
            throw new ArgumentException(nameof(avatarAddress));
        }

        var inventory = new Inventory(list);

        var avatarState = new AvatarState(dictionary)
        {
            inventory = inventory
        };

        return avatarState;
    }

    public async Task<ItemSlotState> GetItemSlotState(Address avatarAddress)
    {
        var state = await _service.GetState(
            ItemSlotState.DeriveAddress(avatarAddress, BattleType.Arena));
        return state switch
        {
            List list => new ItemSlotState(list),
            null => new ItemSlotState(BattleType.Arena),
            _ => throw new ArgumentException(nameof(avatarAddress))
        };
    }

    public async Task<List<RuneState>> GetRuneStates(Address avatarAddress)
    {
        var state = await _service.GetState(
            RuneSlotState.DeriveAddress(avatarAddress, BattleType.Arena));
        var runeSlotState = state switch
        {
            List list => new RuneSlotState(list),
            null => new RuneSlotState(BattleType.Arena),
            _ => throw new ArgumentException(nameof(avatarAddress))
        };

        var runes = new List<RuneState>();
        foreach (var runeStateAddress in runeSlotState.GetEquippedRuneSlotInfos().Select(info => RuneState.DeriveAddress(avatarAddress, info.RuneId)))
        {
            if (await _service.GetState(runeStateAddress) is List list)
            {
                runes.Add(new RuneState(list));
            }
        }

        return runes;
    }
}
