using Mimir.Worker.Models;
using Nekoyume.Model.State;

namespace Mimir.Worker.Constants
{
    public static class CollectionNames
    {
        public static Dictionary<Type, string> CollectionMappings =
            new Dictionary<Type, string>();

        static CollectionNames()
        {
            CollectionMappings.Add(typeof(AvatarState), "avatar");
            CollectionMappings.Add(typeof(InventoryState), "inventory");
            CollectionMappings.Add(typeof(QuestListState), "quest_list");
            CollectionMappings.Add(typeof(WorldInformationState), "world_information");
            CollectionMappings.Add(typeof(SheetState), "table_sheet");
        }
    }
}
