using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Common;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Material"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Material : ItemBase
{
    public HashDigest<SHA256> ItemId { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("item_id", ItemId.Serialize());

    public Material()
    {
    }

    public Material(IValue bencoded) : base(bencoded)
    {
        try
        {
            var material = (Nekoyume.Model.Item.Material)Nekoyume.Model.Item.ItemFactory.Deserialize(bencoded);
            ItemId = material.ItemId;
        }
        catch (ArgumentException)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary, ValueKind.List },
                bencoded.Kind);
        }
    }

    // public Material(
    //     int id,
    //     int grade,
    //     Nekoyume.Model.Item.ItemType itemType,
    //     Nekoyume.Model.Item.ItemSubType itemSubType,
    //     Nekoyume.Model.Elemental.ElementalType elementalType,
    //     HashDigest<SHA256> itemId)
    //     : base(id, grade, itemType, itemSubType, elementalType)
    // {
    //     ItemId = itemId;
    // }
}
