using Bencodex.Types;
using Lib9c.Models.Items;

namespace Lib9c.Models.Extensions;

public static class EquipmentExtensions
{
    public static Nekoyume.Model.Item.Equipment ToNekoyumeEquipment(this Equipment equipment)
    {
        return new Nekoyume.Model.Item.Equipment((Dictionary)equipment.Bencoded);
    }
}