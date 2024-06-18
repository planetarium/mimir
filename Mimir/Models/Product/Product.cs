using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.Models.Product;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    public string Address { get; set; }
    public ProductState State { get; set; }
    public int CombatPoint { get; set; }
    public int UnitPrice { get; set; }
    public int Crystal { get; set; }
    public int CrystalPerPrice { get; set; }
    public string Raw { get; set; }
}
