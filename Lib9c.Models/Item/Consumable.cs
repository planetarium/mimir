using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Stat;
using Nekoyume.Model.State;

namespace Lib9c.Models.Item;

public class Consumable : ItemUsable, IBencodable
{
    public List<DecimalStat> Stats { get; }

    public Consumable(Dictionary bencoded)
        : base(bencoded)
    {
        Stats = bencoded["stats"].ToList(i => new DecimalStat((Dictionary)i));
    }

    public new IValue Bencoded =>
        ((Dictionary)base.Bencoded).Add(
            "stats",
            new List(
                Stats
                    .OrderBy(i => i.StatType)
                    .ThenByDescending(i => i.BaseValue)
                    .Select(s => s.Bencoded)
            )
        );
}
