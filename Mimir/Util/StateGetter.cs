using Libplanet.Crypto;
using Bencodex.Types;
using Nekoyume.TableData;
using Nekoyume;
using Nekoyume.Action;
using Nekoyume.Model.EnumType;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Mimir.Services;

namespace Mimir.Util;

public class StateGetter(IStateService stateService)
{
    public async Task<IValue?> GetStateAsync(Address address, Address accountAddress) =>
        await stateService.GetState(address, accountAddress) ??
        await stateService.GetState(address);

    public async Task<IValue?> GetStateWithLegacyAccountAsync(
        Address address,
        Address accountAddress,
        Address legacyAddress) =>
        await stateService.GetState(address, accountAddress) ??
        await stateService.GetState(legacyAddress);

    public async Task<T?> GetSheetAsync<T>()
        where T : ISheet, new()
    {
        var sheetState = await stateService.GetState(Addresses.GetSheetAddress<T>());
        if (sheetState is not Text sheetValue)
        {
            throw new ArgumentException(nameof(T));
        }

        var sheet = new T();
        sheet.Set(sheetValue.Value);
        return sheet;
    }
    
    public async Task<List<AvatarState>?> GetAvatarStatesAsync(
        Address agentAddress,
        bool withInventory = true)
    {
        var rawState = await stateService.GetState(agentAddress, Addresses.Agent) ??
                       await stateService.GetState(agentAddress);
        var agentState = rawState switch
        {
            List agentStateList => new AgentState(agentStateList),
            Dictionary agentStateDictionary => new AgentState(agentStateDictionary),
            _ => null,
        };
        if (agentState is null)
        {
            return null;
        }

        var avatars = new List<AvatarState>();
        foreach(var avatarAddress in agentState.avatarAddresses.Values)
        {
            var avatarState = await GetAvatarStateAsync(avatarAddress, withInventory);
            if (avatarState is null)
            {
                continue;
            }

            avatars.Add(avatarState);
        }

        return avatars;
    }

    public async Task<AvatarState?> GetAvatarStateAsync(
        Address avatarAddress,
        bool withInventory = true)
    {
        var avatarValue = await GetStateAsync(avatarAddress, Addresses.Avatar);
        var avatarState = avatarValue switch
        {
            List list => new AvatarState(list),
            Dictionary dictionary => new AvatarState(dictionary),
            _ => null,
        };
        if (avatarState is null)
        {
            return null;
        }

        if (withInventory)
        {
            var inventory = await GetInventoryStateAsync(avatarAddress);
            if (inventory is not null)
            {
                avatarState.inventory = inventory;
            }
        }
        
        return avatarState;
    }

    public async Task<Inventory?> GetInventoryStateAsync(Address avatarAddress)
    {
        var rawState = await GetStateWithLegacyAccountAsync(
            avatarAddress,
            Addresses.Inventory,
            avatarAddress.Derive("inventory"));
        return rawState is List list
            ? new Inventory(list)
            : null;
    }

    public async Task<ItemSlotState?> GetItemSlotStateAsync(Address avatarAddress)
    {
        var state = await stateService.GetState(
            ItemSlotState.DeriveAddress(avatarAddress, BattleType.Arena));
        return state switch
        {
            List list => new ItemSlotState(list),
            null => new ItemSlotState(BattleType.Arena),
            _ => null,
        };
    }

    public async Task<List<RuneState>> GetRuneStatesAsync(Address avatarAddress)
    {
        var state = await stateService.GetState(
            RuneSlotState.DeriveAddress(avatarAddress, BattleType.Arena));
        var runeSlotState = state switch
        {
            List list => new RuneSlotState(list),
            null => new RuneSlotState(BattleType.Arena),
            _ => null,
        };
        if (runeSlotState is null)
        {
            return [];
        }

        var runeAddresses = runeSlotState.GetEquippedRuneSlotInfos()
            .Select(info => RuneState.DeriveAddress(avatarAddress, info.RuneId))
            .ToArray();
        var runeValues = await stateService.GetStates(runeAddresses);
        return runeValues.OfType<List>()
            .Select(e => new RuneState(e))
            .ToList();
    }

    public async Task<Dictionary<Address, CollectionState>> GetCollectionStatesAsync(List<Address> addresses)
    {
        var result = new Dictionary<Address, CollectionState>();
        var values = await stateService.GetStates(
            addresses.ToArray(),
            Addresses.Collection
        );
        for (var i = 0; i < addresses.Count; i++)
        {
            var address = addresses[i];
            var serialized = values[i];
            if (serialized is List list)
            {
                result.TryAdd(address, new CollectionState(list));
            }
        }

        return result;
    }
}
