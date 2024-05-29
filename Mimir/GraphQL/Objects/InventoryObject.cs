namespace Mimir.GraphQL.Objects;

public class InventoryObject
{
    public ItemObject[] Consumables { get; set; }
    public ItemObject[] Costumes { get; set; }
    public ItemObject[] Equipments { get; set; }
    public ItemObject[] Materials { get; set; }

    public InventoryObject(
        ItemObject[] consumables,
        ItemObject[] costumes,
        ItemObject[] equipments,
        ItemObject[] materials)
    {
        Consumables = consumables;
        Costumes = costumes;
        Equipments = equipments;
        Materials = materials;
    }
}
