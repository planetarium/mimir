using Bencodex.Types;
using Mimir.GraphQL.Extensions;
using Mimir.GraphQL.Objects;
using MongoDB.Bson;
using Nekoyume.Model.Skill;
using Nekoyume.Model.Stat;
using Nekoyume.TableData;

namespace Mimir.GraphQL.Factories;

/// <summary>
/// This class will be removed after the Mimir.Bson project is completed.
/// </summary>
public static class SkillObjectFactory
{
    public static SkillObject Create(BsonDocument bsonDocument)
    {
        var skillRowDocument = bsonDocument["SkillRow"].AsBsonDocument;
        var skillRow = SkillSheet.Row.Deserialize(Dictionary.Empty
            .Add("id", skillRowDocument["Id"].AsInt32)
            .Add("elemental_type", skillRowDocument["ElementalType"].AsInt32.ToString())
            .Add("skill_type", skillRowDocument["SkillType"].AsInt32.ToString())
            .Add("skill_category", skillRowDocument["SkillCategory"].AsInt32.ToString())
            .Add("skill_target_type", skillRowDocument["SkillTargetType"].AsInt32.ToString())
            .Add("hit_count", skillRowDocument["HitCount"].AsInt32)
            .Add("cooldown", skillRowDocument["Cooldown"].AsInt32)
            .Add("combo", skillRowDocument["Combo"].AsBoolean));
        var power = bsonDocument["Power"].ToLong();
        var chance = bsonDocument["Chance"].AsInt32;
        var statPowerRatio = bsonDocument["StatPowerRatio"].AsInt32;
        var referencedStatType = (StatType)bsonDocument["ReferencedStatType"].AsInt32;

        return new SkillObject(
            skillRow,
            power,
            chance,
            statPowerRatio,
            referencedStatType);
    }

    public static SkillObject Create(ISkill skill)
    {
        return new SkillObject(
            skill.SkillRow,
            skill.Power,
            skill.Chance,
            skill.StatPowerRatio,
            skill.ReferencedStatType);
    }
}
