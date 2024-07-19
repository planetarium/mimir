namespace Mimir.Models.Abstractions;

public interface IItem
{
    Nekoyume.Model.Item.ItemType ItemType { get; }

    Nekoyume.Model.Item.ItemSubType ItemSubType { get; }
}
