using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
using Lib9c.Models.Skills;
using Lib9c.Models.Stats;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.ItemUsable"/>
/// </summary>
public record ItemUsable : ItemBase, IBencodable
{
    public Guid ItemId { get; init; }

    public StatMap StatsMap { get; init; }

    public List<Skill> Skills { get; init; }

    public List<Skill> BuffSkills { get; init; }
    public long RequiredBlockIndex { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("itemId", ItemId.Serialize())
        .Add("statsMap", StatsMap.Bencoded)
        .Add("skills", new List(Skills
            .OrderByDescending(i => i.Chance)
            .ThenByDescending(i => i.Power)
            .Select(s => s.Bencoded)))
        .Add("buffSkills", new List(BuffSkills
            .OrderByDescending(i => i.Chance)
            .ThenByDescending(i => i.Power)
            .Select(s => s.Bencoded)))
        .Add("requiredBlockIndex", RequiredBlockIndex.Serialize());

    public ItemUsable(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        ItemId = d["itemId"].ToGuid();
        StatsMap = new StatMap(d["statsMap"]);
        Skills = ((List)d["skills"])
            .Select(SkillFactory.Create)
            .ToList();
        BuffSkills = ((List)d["buffSkills"])
            .Select(SkillFactory.Create)
            .ToList();
        RequiredBlockIndex = d["requiredBlockIndex"].ToLong();
    }
}
