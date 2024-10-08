using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Item;
using ItemFactory = Lib9c.Models.Factories.ItemFactory;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Inventory.Item"/>
/// </summary>
[BsonIgnoreExtraElements]
public record InventoryItem : IBencodable
{
    public ItemBase Item { get; init; }
    public int Count { get; init; }
    public ILock? Lock { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded
    {
        get
        {
            var d = Dictionary.Empty
                .Add("item", Item.Bencoded)
                .Add("count", Count);
            return Lock is null
                ? d
                : d.Add("l", Lock.Serialize());
        }
    }

    public InventoryItem(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Item = ItemFactory.Deserialize(d["item"]);
        Count = (Integer)d["count"];
        if (d.ContainsKey("l"))
        {
            Lock = d["l"].ToLock();
        }
    }
}
