using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Items;
using ItemType = Nekoyume.Model.Item.ItemType;
using ItemSubType = Nekoyume.Model.Item.ItemSubType;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;
using ValueKind = Bencodex.Types.ValueKind;
using Nekoyume.Model.Elemental;

namespace Lib9c.Models.Factories;

public static class ItemFactory
{
    public static ItemBase Deserialize(IValue bencoded)
    {
        // Dictionary 포맷 처리
        if (bencoded is Dictionary d)
        {
            return DeserializeFromDictionary(d);
        }

        // List 포맷 처리 (PR #3201 지원)
        if (bencoded is List l)
        {
            return DeserializeFromList(l);
        }

        throw new UnsupportedArgumentTypeException<ValueKind>(
            nameof(bencoded),
            new[] { ValueKind.Dictionary, ValueKind.List },
            bencoded.Kind);
    }

    private static ItemBase DeserializeFromDictionary(Dictionary d)
    {
        if (!d.TryGetValue((Text)"item_type", out var type) ||
            !d.TryGetValue((Text)"item_sub_type", out var subType))
        {
            throw new ArgumentException($"Missing required fields in dictionary: {d}");
        }

        var itemType = type.ToEnum<ItemType>();
        var itemSubType = subType.ToEnum<ItemSubType>();

        return CreateItemFromType(itemType, itemSubType, d);
    }

    private static ItemBase DeserializeFromList(List l)
    {
        // List 포맷에서 아이템 타입 정보 추출
        // lib9c의 List 포맷 구조에 따라 인덱스로 접근
        if (l.Count < 3) // 최소 필요한 필드 수
        {
            throw new ArgumentException($"List too short for item deserialization: {l}");
        }

        var itemType = (ItemType)l[2].ToInteger();
        var itemSubType = (ItemSubType)l[3].ToInteger();

        return CreateItemFromType(itemType, itemSubType, l);
    }

    private static ItemBase CreateItemFromType(ItemType itemType, ItemSubType itemSubType, IValue bencoded)
    {
        return itemType switch
        {
            ItemType.Consumable => new Consumable(bencoded),
            ItemType.Costume => new Costume(bencoded),
            ItemType.Equipment => CreateEquipment(itemSubType, bencoded),
            ItemType.Material => CreateMaterial(bencoded),
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), $"Unsupported item type: {itemType}")
        };
    }

    private static ItemBase CreateEquipment(ItemSubType itemSubType, IValue bencoded)
    {
        return itemSubType switch
        {
            ItemSubType.Weapon => new Weapon(bencoded),
            ItemSubType.Armor => new Armor(bencoded),
            ItemSubType.Belt => new Belt(bencoded),
            ItemSubType.Necklace => new Necklace(bencoded),
            ItemSubType.Ring => new Ring(bencoded),
            ItemSubType.Aura => new Aura(bencoded),
            ItemSubType.Grimoire => new Grimoire(bencoded),
            _ => throw new ArgumentOutOfRangeException(nameof(itemSubType), $"Unsupported equipment sub-type: {itemSubType}")
        };
    }

    private static ItemBase CreateMaterial(IValue bencoded)
    {
        switch (bencoded)
        {
            case Dictionary d:
                if (d.ContainsKey(RequiredBlockIndexKey))
                {
                    return new TradableMaterial(bencoded);
                }
                return new Material(bencoded);
            case List l:
                if (l.Count == 8)
                {
                    return new TradableMaterial(bencoded);
                }
                return new Material(bencoded);
            default:
                throw new UnsupportedArgumentTypeException<ValueKind>(
                    nameof(bencoded),
                    new[] { ValueKind.Dictionary, ValueKind.List },
                    bencoded.Kind);
        }
    }

    // 공통 속성 파싱을 위한 헬퍼 메서드들
    public static (int id, int grade, ItemType itemType, ItemSubType itemSubType, Nekoyume.Model.Elemental.ElementalType elementalType)
        ParseCommonProperties(IValue bencoded)
    {
        if (bencoded is Dictionary d)
        {
            return ParseCommonPropertiesFromDictionary(d);
        }

        if (bencoded is List l)
        {
            return ParseCommonPropertiesFromList(l);
        }

        throw new ArgumentException($"Unsupported bencoded format: {bencoded.GetType()}");
    }

    private static (int id, int grade, ItemType itemType, ItemSubType itemSubType, Nekoyume.Model.Elemental.ElementalType elementalType)
        ParseCommonPropertiesFromDictionary(Dictionary d)
    {
        var id = d["id"].ToInteger();
        var grade = d["grade"].ToInteger();
        var itemType = d["item_type"].ToEnum<ItemType>();
        var itemSubType = d["item_sub_type"].ToEnum<ItemSubType>();
        var elementalType = d["elemental_type"].ToEnum<Nekoyume.Model.Elemental.ElementalType>();

        return (id, grade, itemType, itemSubType, elementalType);
    }

    private static (int id, int grade, ItemType itemType, ItemSubType itemSubType, Nekoyume.Model.Elemental.ElementalType elementalType)
        ParseCommonPropertiesFromList(List l)
    {
        // lib9c List 포맷 구조에 따라 인덱스로 접근
        var id = l[1].ToInteger();
        var itemType = (ItemType)l[2].ToInteger();
        var itemSubType = (ItemSubType)l[3].ToInteger();
        var grade = l[4].ToInteger();
        var elementalType = (ElementalType)l[5].ToInteger();

        return (id, grade, itemType, itemSubType, elementalType);
    }
}
