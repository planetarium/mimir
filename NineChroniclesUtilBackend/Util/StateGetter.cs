using Libplanet.Crypto;
using Bencodex.Types;
using Nekoyume.TableData;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using NineChroniclesUtilBackend.Services;

namespace NineChroniclesUtilBackend.Util;

public class StateGetter
{
    private readonly IStateService _stateService;

    public StateGetter(IStateService stateService)
    {
        _stateService = stateService;
    }

    public async Task<T> GetSheet<T>()
        where T : ISheet, new()
    {
        var sheetState = await _stateService.GetState(
            Addresses.TableSheet.Derive(typeof(T).Name)
        );
        if (sheetState is not Text sheetValue)
        {
            throw new ArgumentException(nameof(T));
        }

        var sheet = new T();
        sheet.Set(sheetValue.Value);
        return sheet;
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
        var rawState = await GetAvatarStateWithLegacyAccount(
            avatarAddress, Addresses.Inventory, legacyInventoryAddress);

        if (rawState is not List list)
        {
            throw new ArgumentException(nameof(avatarAddress));
        }

        return new Inventory(list);
    }

    public async Task<ItemSlotState> GetItemSlotState(Address avatarAddress)
    {
        var state = await _stateService.GetState(
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
        var state = await _stateService.GetState(
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
            if (await _stateService.GetState(runeStateAddress) is List list)
            {
                runes.Add(new RuneState(list));
            }
        }

        return runes;
    }

    public async Task<Dictionary<Address, CollectionState>> GetCollectionStates(List<Address> addresses)
    {
        var result = new Dictionary<Address, CollectionState>();

        var values = await _stateService.GetStates(
            addresses.ToArray(),
            Addresses.Collection
        );

        for (int i = 0; i < addresses.Count; i++)
        {
            var serialized = values[i];
            var address = addresses[i];

            if (serialized is List bencoded)
            {
                result.TryAdd(address, new CollectionState(bencoded));
            }
        }
        
        return result;
    }

    public async Task<IValue?> GetStateWithLegacyAccount(Address address, Address accountAddress)
    {
        var state = await _stateService.GetState(address, accountAddress);
        
        if (state == null)
        {
            state = await _stateService.GetState(address);
        }
        return state;
    }

    public async Task<IValue?> GetAvatarStateWithLegacyAccount(Address avatarAddress, Address accountAddress, Address legacyAddress)
    {
        var state = await _stateService.GetState(avatarAddress, accountAddress);
        
        if (state == null)
        {
            state = await _stateService.GetState(legacyAddress);
        }
        return state;
    }
}
