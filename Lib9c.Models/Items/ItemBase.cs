using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.ItemBase"/>
/// </summary>
[BsonIgnoreExtraElements]
public record ItemBase : IBencodable
{
    public int Id { get; init; }
    public int Grade { get; init; }
    public Nekoyume.Model.Item.ItemType ItemType { get; init; }
    public Nekoyume.Model.Item.ItemSubType ItemSubType { get; init; }
    public Nekoyume.Model.Elemental.ElementalType ElementalType { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public virtual IValue Bencoded => Dictionary.Empty
        .Add("id", Id.Serialize())
        .Add("item_type", ItemType.Serialize())
        .Add("item_sub_type", ItemSubType.Serialize())
        .Add("grade", Grade.Serialize())
        .Add("elemental_type", ElementalType.Serialize());

    public ItemBase()
    {
    }

    public ItemBase(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Id = d["id"].ToInteger();
        Grade = d["grade"].ToInteger();
        ItemType = d["item_type"].ToEnum<Nekoyume.Model.Item.ItemType>();
        ItemSubType = d["item_sub_type"].ToEnum<Nekoyume.Model.Item.ItemSubType>();
        ElementalType = d["elemental_type"].ToEnum<Nekoyume.Model.Elemental.ElementalType>();
    }
}
