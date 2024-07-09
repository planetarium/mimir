using Bencodex;
using Bencodex.Types;
using Mimir.Models.Stat;
using Nekoyume.Model.State;

namespace Mimir.Models.Item;

public class Consumable : ItemUsable, IBencodable
{
    public List<DecimalStat> Stats { get; }

    public Consumable(Dictionary bencoded)
        : base(bencoded)
    {
        Stats = bencoded["stats"].ToList(i => new DecimalStat((Dictionary)i));
    }

    new public IValue Bencoded =>
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
