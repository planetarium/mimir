using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Block;

/// <summary>
/// <see cref="Action"/>
/// </summary>
[BsonIgnoreExtraElements]
public record class Action
{
    public string Raw { get; set; }
    public string TypeId { get; set; }
    public BsonDocument Values { get; set; }
}
