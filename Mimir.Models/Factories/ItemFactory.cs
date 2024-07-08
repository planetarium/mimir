using Bencodex.Types;
using Mimir.Models.Item;
using Nekoyume.Model.State;
using ItemType = Nekoyume.Model.Item.ItemType;
using ItemSubType = Nekoyume.Model.Item.ItemSubType;

namespace Mimir.Models.Factories;

public static class ItemFactory
{
    public static ItemBase Deserialize(Dictionary serialized)
    {
        if (
            serialized.TryGetValue((Text)"item_type", out var type)
            && serialized.TryGetValue((Text)"item_sub_type", out var subType)
        )
        {
            var itemType = type.ToEnum<ItemType>();
            var itemSubType = subType.ToEnum<ItemSubType>();

            switch (itemType)
            {
                case ItemType.Consumable:
                    return new Consumable(serialized);
                case ItemType.Costume:
                    return new Costume(serialized);
                case ItemType.Equipment:
                    switch (itemSubType)
                    {
                        case ItemSubType.Weapon:
                            return new Weapon(serialized);
                        case ItemSubType.Armor:
                            return new Armor(serialized);
                        case ItemSubType.Belt:
                            return new Belt(serialized);
                        case ItemSubType.Necklace:
                            return new Necklace(serialized);
                        case ItemSubType.Ring:
                            return new Ring(serialized);
                        case ItemSubType.Aura:
                            return new Aura(serialized);
                    }
                    break;
                case ItemType.Material:
                    if (serialized.ContainsKey("rbi"))
                    {
                        return new TradableMaterial(serialized);
                    }
                    else
                    {
                        return new Material(serialized);
                    }
                default:
                    throw new ArgumentOutOfRangeException(nameof(itemType));
            }
        }

        throw new ArgumentException($"Can't Deserialize Item {serialized}");
    }
}
