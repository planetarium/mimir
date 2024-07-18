using System.Net.Mime;

namespace Mimir.Enums;

public class CollectionNames
{
    private CollectionNames(string value)
    {
        Value = value;
    }

    public string Value { get; private set; }

    public static CollectionNames ArenaScore
    {
        get { return new CollectionNames("arena_score"); }
    }
    public static CollectionNames ArenaInformation
    {
        get { return new CollectionNames("arena_information"); }
    }
    public static CollectionNames Avatar
    {
        get { return new CollectionNames("avatar"); }
    }
    public static CollectionNames Product
    {
        get { return new CollectionNames("product"); }
    }
    public static CollectionNames ActionPoint
    {
        get { return new CollectionNames("action_point"); }
    }
    public static CollectionNames Agent
    {
        get { return new CollectionNames("agent"); }
    }
    public static CollectionNames AllRune
    {
        get { return new CollectionNames("all_rune"); }
    }
    public static CollectionNames Collection
    {
        get { return new CollectionNames("collection"); }
    }
    public static CollectionNames DailyReward
    {
        get { return new CollectionNames("daily_reward"); }
    }
    public static CollectionNames Inventory
    {
        get { return new CollectionNames("inventory"); }
    }
    public static CollectionNames ItemSlot
    {
        get { return new CollectionNames("item_slot"); }
    }
    public static CollectionNames Metadata
    {
        get { return new CollectionNames("metadata"); }
    }
    public static CollectionNames RuneSlot
    {
        get { return new CollectionNames("rune_slot"); }
    }
    public static CollectionNames Stake
    {
        get { return new CollectionNames("stake"); }
    }
    public static CollectionNames TableSheet
    {
        get { return new CollectionNames("table_sheet"); }
    }

    public override string ToString()
    {
        return Value;
    }
}
