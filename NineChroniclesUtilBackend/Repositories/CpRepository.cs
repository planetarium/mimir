using System.Text.Json.Nodes;
using Bencodex;
using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume;
using Nekoyume.Battle;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using Nekoyume.TableData;

namespace NineChroniclesUtilBackend.Repositories;

public static class CpRepository
{
    public static async Task<int> CalculateCp(
        string avatarAddress,
        int avatarLevel,
        int characterId,
        IEnumerable<string> equipmentIds,
        IEnumerable<string> costumeIds,
        IEnumerable<(int id, int level)> runeOption
    )
    {
        var inventory = await GetInventory(avatarAddress);
        var equipments = equipmentIds
            .Select(x => inventory.GetValueOrDefault(x))
            .Where(x => x != null)
            .Select(x => new Equipment(x))
            .Where(x => x.Equipped);
        var costumes = costumeIds
            .Select(x => inventory.GetValueOrDefault(x))
            .Where(x => x != null)
            .Select(x => new Costume(x))
            .Where(x => x.Equipped);
        var runes = runeOption.Select(x => GetRuneOptionInfo(x.id, x.level).Result);
        var characterRow = await GetCharacterRow(characterId);
        var costumeStatSheet = await GetSheet(new CostumeStatSheet());

        return CPHelper.TotalCP(
            equipments,
            costumes,
            runes,
            avatarLevel,
            characterRow,
            costumeStatSheet
        );
    }

    private static async Task<Dictionary<string, Dictionary>> GetInventory(string address)
    {
        var inventoryState = await GetInventoryState(address);

        return inventoryState
            .Select(x => (Dictionary)((Dictionary)x)["item"])
            .Where(x =>
                ((Text)x["item_type"]).Value == "Costume"
                || ((Text)x["item_type"]).Value == "Equipment"
            )
            .ToDictionary(x =>
            {
                if (((Text)x["item_type"]).Value == "Costume")
                    return ((Binary)x["item_id"]).ToGuid().ToString();

                return ((Binary)x["itemId"]).ToGuid().ToString();
            });
    }

    private static async Task<RuneOptionSheet.Row.RuneOptionInfo> GetRuneOptionInfo(
        int id,
        int level
    )
    {
        var sheets = await GetSheet(new RuneOptionSheet());

        if (!sheets.TryGetValue(id, out var optionRow))
        {
            throw new SheetRowNotFoundException("RuneOptionSheet", id);
        }

        if (!optionRow.LevelOptionMap.TryGetValue(level, out var option))
        {
            throw new SheetRowNotFoundException("RuneOptionSheet", level);
        }

        return option;
    }

    private static async Task<CharacterSheet.Row> GetCharacterRow(int characterId)
    {
        var sheets = await GetSheet(new CharacterSheet());

        if (!sheets.TryGetValue(characterId, out var row))
        {
            throw new SheetRowNotFoundException("CharacterSheet", characterId);
        }

        return row;
    }

    private static async Task<T> GetSheet<T>(T sheets)
        where T : ISheet
    {
        var sheetsAddress = Addresses.GetSheetAddress<T>();
        var rawSheets = await GetState<Text>(sheetsAddress);
        sheets.Set(rawSheets);

        return sheets;
    }

    private static async Task<T> GetState<T>(Address address, string? account = null)
        where T : IValue => await GetState<T>(address.ToString(), account);

    // TODO: Cache
    private static async Task<T?> GetState<T>(string address, string? account = null)
        where T : IValue
    {
        var codec = new Codec();
        var url =
            account == null
                ? $"http://localhost:5009/api/states/{address}/raw"
                : $"http://localhost:5009/api/states/{address}/raw?account={account}";
        var client = new HttpClient();
        var response = await client.GetAsync(url);
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync());
        var rawValue = json["value"]?.GetValue<string>();
        if (rawValue == null)
        {
            return default;
        }
        var value = codec.Decode(Convert.FromBase64String(rawValue));

        return (T)value;
    }

    private static async Task<List> GetInventoryState(string address) =>
        await GetState<List?>(address, Addresses.Inventory.ToString()) ?? [];
}
