using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Stat;
using Nekoyume.TableData;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Skills;

/// <summary>
/// <see cref="Nekoyume.Model.Skill.Skill"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Skill : IBencodable
{
    public SkillSheet.Row SkillRow { get; init; }
    public long Power { get; init; }
    public int Chance { get; init; }
    public int StatPowerRatio { get; init; }
    public StatType ReferencedStatType { get; init; }

    private readonly bool _skillRowHasCombo = false;

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded
    {
        get
        {
            var d = Dictionary.Empty
                .Add("skillRow", SkillRow.Serialize())
                .Add("power", Power.Serialize())
                .Add("chance", Chance.Serialize());

            if (StatPowerRatio != default && ReferencedStatType != StatType.NONE)
            {
                d = d
                    .Add("stat_power_ratio", StatPowerRatio.Serialize())
                    .Add("referenced_stat_type", ReferencedStatType.Serialize());
            }

            if (!_skillRowHasCombo)
            {
                var skillRow = (Dictionary)d["skillRow"];
                if (skillRow.ContainsKey("combo"))
                {
                    skillRow = new Dictionary(skillRow.Remove((Text)"combo"));
                }

                d = d.SetItem("skillRow", skillRow);
            }

            return d;
        }
    }

    public Skill()
    {
    }

    public Skill(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        SkillRow = SkillSheet.Row.Deserialize((Dictionary)d["skillRow"]);
        _skillRowHasCombo = ((Dictionary)d["skillRow"]).ContainsKey("combo");

        Power = d["power"].ToInteger();
        Chance = d["chance"].ToInteger();
        StatPowerRatio = d.TryGetValue((Text)"stat_power_ratio", out var ratioValue)
            ? ratioValue.ToInteger()
            : default;
        ReferencedStatType = d.TryGetValue((Text)"referenced_stat_type", out var refStatType)
            ? StatTypeExtension.Deserialize((Binary)refStatType)
            : StatType.NONE;
    }
}
