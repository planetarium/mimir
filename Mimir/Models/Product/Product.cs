using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Mimir.Models.Product;

[BsonIgnoreExtraElements]
public class Product
{
    public ObjectId Id { get; set; }
    public string Address { get; set; }
    public ProductState State { get; set; }
}
