using Bencodex.Types;
using Lib9c.Models.Items;
using Lib9c.Models.Tests.Fixtures.States;

namespace Lib9c.Models.Tests.Items;

public class InventoryTest
{
    [Fact(Skip = "Failed")]
    public void Test()
    {
        // Prepare target state
        var value = StateReader.ReadState("Inventory");
        var target = new Nekoyume.Model.Item.Inventory((List)value);

        // serialize target state and deserialize as paired state
        var serialized = target.Serialize();
        var paired = new Inventory(serialized);
        Assert.Equal(target.Items.Count, paired.Items.Count);
        // ...

        // serialize paired state and verify
        var bencoded = paired.Bencoded;
        Assert.Equal(serialized, bencoded);
        // var serializedList = (List)serialized;
        // var bencodedList = (List)bencoded;
        // for (var i = 0; i < serializedList.Count; i++)
        // {
        //     var serializedItem = serializedList[i];
        //     var bencodedItem = bencodedList[i];
        //     Assert.Equal(serializedItem, bencodedItem);
        // }

        // deserialize bencoded state as target2 and verify
        var target2 = new Nekoyume.Model.Item.Inventory((List)bencoded);
        var serialized2 = target2.Serialize();
        var target3 = new Nekoyume.Model.Item.Inventory((List)serialized);
        var serialized3 = target3.Serialize();
        Assert.Equal(serialized2, serialized3);
        // var serialized2List = (List)serialized2;
        // var serialized3List = (List)serialized3;
        // for (var i = 0; i < serialized2List.Count; i++)
        // {
        //     var serialized2Item = serialized2List[i];
        //     var serialized3Item = serialized3List[i];
        //     Assert.Equal(serialized2Item, serialized3Item);
        // }
    }
}
