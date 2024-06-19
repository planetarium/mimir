using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Mimir.Models.Product;

public class ProductState
{
    public class ProductObjectSerializer : SerializerBase<IProductObject>
    {
        public override IProductObject Deserialize(
            BsonDeserializationContext context,
            BsonDeserializationArgs args
        )
        {
            var document = BsonDocumentSerializer.Instance.Deserialize(context, args);
            if (document.Contains("ItemCount"))
            {
                return BsonSerializer.Deserialize<ItemProductObject>(document);
            }
            else
            {
                return BsonSerializer.Deserialize<FavProductObject>(document);
            }
            throw new NotSupportedException("Unsupported product object type");
        }

        public override void Serialize(
            BsonSerializationContext context,
            BsonSerializationArgs args,
            IProductObject value
        )
        {
            BsonSerializer.Serialize(context.Writer, value.GetType(), value);
        }
    }

    [BsonElement("address")]
    public string Address { get; set; }
    public string AvatarAddress { get; set; }
    public string ProductsStateAddress { get; set; }

    [BsonSerializer(typeof(ProductObjectSerializer))]
    public IProductObject Object { get; set; }
    public int? CombatPoint { get; set; }
    public int? UnitPrice { get; set; }
    public int? Crystal { get; set; }
    public int? CrystalPerPrice { get; set; }
    public string Raw { get; set; }
}
