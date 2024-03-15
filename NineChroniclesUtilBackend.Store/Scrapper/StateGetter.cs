using NineChroniclesUtilBackend.Store.Services;
using Libplanet.Crypto;
using Bencodex.Types;
using BTAI;
using Nekoyume.TableData;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.Model.Arena;
using NineChroniclesUtilBackend.Store.Models;

namespace NineChroniclesUtilBackend.Store.Scrapper;

public class StateGetter
{
    private readonly ILogger<StateGetter> _logger;
    private readonly IStateService _service;
    private readonly long? _blockIndex;

    public StateGetter(IStateService service, long? blockIndex)
    {
        _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<StateGetter>();
        _service = service;
        _blockIndex = blockIndex;
    }

    public async Task<T> GetSheet<T>()
        where T : ISheet, new()
    {
        var sheetState = await _service.GetState(Addresses.TableSheet.Derive(typeof(T).Name), _blockIndex);
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
        var state = await _service.GetState(arenaParticipantsAddress, _blockIndex);
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
        var state = await _service.GetState(arenaScoreAddress, _blockIndex);
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
        var state = await _service.GetState(arenaInfoAddress, _blockIndex);
        return state switch
        {
            List list => new ArenaInformation(list),
            _ => throw new ArgumentException(nameof(arenaInfoAddress))
        };
    }

    public async Task<AvatarState> GetAvatarState(Address avatarAddress)
    {
        var state = await GetStateWithLegacyAccount(avatarAddress, Addresses.Avatar);
        var inventory = await GetInventoryState(avatarAddress);
        
        AvatarState avatarState;
        if (state is Dictionary dictionary)
        {
            avatarState = new AvatarState(dictionary)
            {
                inventory = inventory
            };
        }
        else if (state is List alist)
        {
            avatarState = new AvatarState(alist)
            {
                inventory = inventory
            };
        }
        else
        {
            throw new ArgumentException($"Unsupported state type for address: {avatarAddress}");
        }

        return avatarState;
    }

    public async Task<Inventory> GetInventoryState(Address avatarAddress)
    {

        var legacyInventoryAddress = avatarAddress.Derive("inventory");
        var rawState = await GetStateWithLegacyAccount(
            avatarAddress, Addresses.Inventory, legacyInventoryAddress);

        if (rawState is not List list)
        {
            throw new ArgumentException(nameof(avatarAddress));
        }

        return new Inventory(list);
    }

    public async Task<ItemSlotState> GetItemSlotState(Address avatarAddress)
    {
        var state = await _service.GetState(
            ItemSlotState.DeriveAddress(avatarAddress, BattleType.Arena), _blockIndex);
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
            RuneSlotState.DeriveAddress(avatarAddress, BattleType.Arena), _blockIndex);
        var runeSlotState = state switch
        {
            List list => new RuneSlotState(list),
            null => new RuneSlotState(BattleType.Arena),
            _ => throw new ArgumentException(nameof(avatarAddress))
        };

        var runes = new List<RuneState>();
        foreach (var runeStateAddress in runeSlotState.GetEquippedRuneSlotInfos().Select(info => RuneState.DeriveAddress(avatarAddress, info.RuneId)))
        {
            if (await _service.GetState(runeStateAddress, _blockIndex) is List list)
            {
                runes.Add(new RuneState(list));
            }
        }

        return runes;
    }

    private async Task<IValue?> GetStateWithLegacyAccount(Address address, Address accountAddress)
    {
        try
        {
            return await _service.GetState(address, accountAddress, _blockIndex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return await _service.GetState(address, _blockIndex);
        }
    }

    private async Task<IValue?> GetStateWithLegacyAccount(Address avatarAddress, Address accountAddress, Address legacyAddress)
    {
        IValue? rawState;
        try
        {
            rawState = await _service.GetState(avatarAddress, accountAddress, _blockIndex);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            rawState = await _service.GetState(legacyAddress, _blockIndex);
        }
        return rawState;
    }
    
    public async Task<ArenaSheet.RoundData> GetArenaRoundData(long index)
    {
        var arenaSheet = await GetSheet<ArenaSheet>();
        var roundData = arenaSheet.GetRoundByBlockIndex(index);

        return roundData;
    }

    public async Task<ArenaData?> GetArenaData(ArenaSheet.RoundData roundData, Address avatarAddress)
    {
        try
        {
            var arenaScore = await GetArenaScoreState(avatarAddress, roundData.ChampionshipId, roundData.Round);
            var arenaInfo = await GetArenaInfoState(avatarAddress, roundData.ChampionshipId, roundData.Round);

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
            var avatarState = await GetAvatarState(avatarAddress);
            var avatarItemSlotState = await GetItemSlotState(avatarAddress);
            var avatarRuneStates = await GetRuneStates(avatarAddress);

            return new AvatarData(avatarState, avatarItemSlotState, avatarRuneStates);
        }
        catch (Exception ex)
        {
            _logger.LogError($"An error occurred during GetAvatarData: {ex.Message}");
            return null;
        }
    }
}
