using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Stats;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Consumable"/>
/// </summary>
public record Consumable : ItemUsable, IBencodable
{
    public List<DecimalStat> Stats { get; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("stats", new List(Stats
            .OrderBy(i => i.StatType)
            .ThenByDescending(i => i.BaseValue)
            .Select(s => s.BencodedWithoutAdditionalValue)));

    public Consumable(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        Stats = d["stats"].ToList(e => new DecimalStat(e));
    }
}
