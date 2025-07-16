using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Item;
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
        try
        {
            // ItemFactory의 헬퍼 메서드를 사용하여 공통 속성 파싱
            var properties = Lib9c.Models.Factories.ItemFactory.ParseCommonProperties(bencoded);

            Id = properties.id;
            Grade = properties.grade;
            ItemType = properties.itemType;
            ItemSubType = properties.itemSubType;
            ElementalType = properties.elementalType;
        }
        catch (ArgumentException)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary, ValueKind.List },
                bencoded.Kind);
        }
    }
}
