using Mimir.Worker.Models;
using Nekoyume.Model.State;
using AllRuneState = Mimir.Worker.Models.AllRuneState;

namespace Mimir.Worker.Constants
{
    public static class CollectionNames
    {
        public static readonly Dictionary<Type, string> CollectionMappings = new();

        static CollectionNames()
        {
            CollectionMappings.Add(typeof(AvatarState), "avatar");
            CollectionMappings.Add(typeof(InventoryState), "inventory");
            CollectionMappings.Add(typeof(QuestListState), "quest_list");
            CollectionMappings.Add(typeof(WorldInformationState), "world_information");
            CollectionMappings.Add(typeof(ActionPointState), "action_point");
            CollectionMappings.Add(typeof(SheetState), "table_sheet");
            CollectionMappings.Add(typeof(ArenaScoreState), "arena_score");
            CollectionMappings.Add(typeof(ArenaInformationState), "arena_information");
            CollectionMappings.Add(typeof(AllRuneState), "rune_state");
        }
    }
}
