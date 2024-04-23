using MongoDB.Bson;

namespace NineChroniclesUtilBackend.Models.Items;

public class Equipment(BsonValue equipment) : NonFungibleItem(equipment)
{
    public int Level { get; set; } = equipment["level"].AsInt32;
}
