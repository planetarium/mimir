using Mimir.Exceptions;
using Mimir.GraphQL.Extensions;
using MongoDB.Bson;
using Nekoyume.Model.Stat;

namespace Mimir.Factories;

/// <summary>
/// <see cref="Nekoyume.Model.Stat.StatMap"/>, <see cref="Nekoyume.Model.Stat.StatsMap"/>
/// and <see cref="Nekoyume.Model.Stat.Stats"/> basically have the same structure,
/// and they are implements the same interfaces.
/// i.e. <see cref="Nekoyume.Model.Stat.IStats"/>, <see cref="Nekoyume.Model.Stat.IBaseAndAdditionalStats"/>
/// BsonDocument of these classes are also similar because of above reason.
/// So, we can create a factory class to convert between these classes.
/// </summary>
public static class StatMapFactory
{
    private static readonly Dictionary<StatType, (string baseKey, string additionalKey)> BsonKeys =
        Enum.GetValues<StatType>()
            .ToDictionary(
                e => e,
                e => ($"Base{e}", $"Additional{e}"));

    public static StatMap Create(BsonDocument bsonDocument)
    {
        var statsMap = new StatMap();
        var statTypes = Enum.GetValues<StatType>();
        foreach (var statType in statTypes)
        {
            var (baseKey, additionalKey) = BsonKeys[statType];
            if (bsonDocument.Contains(baseKey))
            {
                var baseValue = bsonDocument[baseKey].ToLong();
                statsMap[statType].SetBaseValue(baseValue);
            }

            if (bsonDocument.Contains(additionalKey))
            {
                var additionalValue = bsonDocument[additionalKey].ToLong();
                statsMap[statType].SetAdditionalValue(additionalValue);
            }
        }

        return statsMap;
    }

    public static StatMap Create(StatsMap statsMap)
    {
        var statMap = new StatMap();
        foreach (var (statType, baseValue, additionalValue) in statsMap.GetBaseAndAdditionalStats(ignoreZero: true))
        {
            statMap[statType].SetBaseValue(baseValue);
            statMap[statType].SetAdditionalValue(additionalValue);
        }

        return statMap;
    }
}
