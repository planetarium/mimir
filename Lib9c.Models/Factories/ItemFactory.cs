using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Items;
using ItemType = Nekoyume.Model.Item.ItemType;
using ItemSubType = Nekoyume.Model.Item.ItemSubType;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Factories;

public static class ItemFactory
{
    public static ItemBase Deserialize(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        if (d.TryGetValue((Text)"item_type", out var type) &&
            d.TryGetValue((Text)"item_sub_type", out var subType))
        {
            var itemType = type.ToEnum<ItemType>();
            var itemSubType = subType.ToEnum<ItemSubType>();

            switch (itemType)
            {
                case ItemType.Consumable:
                    return new Consumable(d);
                case ItemType.Costume:
                    return new Costume(d);
                case ItemType.Equipment:
                    switch (itemSubType)
                    {
                        case ItemSubType.Weapon:
                            return new Weapon(d);
                        case ItemSubType.Armor:
                            return new Armor(d);
                        case ItemSubType.Belt:
                            return new Belt(d);
                        case ItemSubType.Necklace:
                            return new Necklace(d);
                        case ItemSubType.Ring:
                            return new Ring(d);
                        case ItemSubType.Aura:
                            return new Aura(d);
                        case ItemSubType.Grimoire:
                            return new Grimoire(d);
                    }

                    break;
                case ItemType.Material:
                    return d.ContainsKey(RequiredBlockIndexKey)
                        ? new TradableMaterial(d)
                        : new Material(d);

                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType));
            }
        }

        throw new ArgumentException($"Can't Deserialize Item {d}");
    }
}
