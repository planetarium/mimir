using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Inventory"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Inventory : IBencodable
{
    public List<InventoryItem> Items { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => new List(Items
        .OrderBy(i => i.Item.Id)
        .ThenByDescending(i => i.Count)
        .Select(i => i.Bencoded));

    public Inventory(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Items = new List<InventoryItem>(l.Count);
        foreach (var value in l)
        {
            Items.Add(new InventoryItem(value));
        }
    }
}
