using Lib9c.Models.Items;

namespace Lib9c.Models.Tests.Items;

public class ItemsCommonTests
{
    [Fact]
    public void Handle_All_Relates_Types_Of_ItemBase()
    {
        var targetType = typeof(Nekoyume.Model.Item.ItemBase);
        var targetNameTuples = targetType.Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(targetType))
            .Select(t => (t.Name, t.FullName))
            .ToArray();

        // Prevent unexpected namespace. "Nekoyume.Model.Item.{Item}"
        foreach (var (_, targetMailFullName) in targetNameTuples)
        {
            Assert.NotNull(targetMailFullName);
            Assert.StartsWith("Nekoyume.Model.Item.", targetMailFullName);
        }

        var itemType = typeof(ItemBase);
        var itemNameTuples = itemType.Assembly.GetTypes()
            .Where(t => t.IsAssignableTo(itemType))
            .Select(t => (t.Name, t.FullName))
            .ToArray();

        // Prevent unexpected namespace. "Lib9c.Models.Items.{ItemBase}"
        foreach (var (_, itemFullName) in itemNameTuples)
        {
            Assert.NotNull(itemFullName);
            Assert.StartsWith("Lib9c.Models.Items.", itemFullName);
        }

        var itemNames = itemNameTuples.Select(e => e.Name).ToArray();
        foreach (var (targetName, _) in targetNameTuples)
        {
            Assert.Contains(targetName, itemNames);
        }
    }
}
