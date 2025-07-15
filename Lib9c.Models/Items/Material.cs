using System.Security.Cryptography;
using System.Text.Json.Serialization;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
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
            // 공통 속성은 base 생성자에서 처리됨
            // ItemId만 추가로 파싱
            ItemId = ParseItemId(bencoded);
        }
        catch (ArgumentException)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary, ValueKind.List },
                bencoded.Kind);
        }
    }

    private static HashDigest<SHA256> ParseItemId(IValue bencoded)
    {
        if (bencoded is Dictionary d && d.TryGetValue((Text)"item_id", out var itemIdValue))
        {
            return itemIdValue.ToItemId();
        }

        if (bencoded is List l && l.Count > 5) // List 포맷에서 ItemId 위치
        {
            return l[6].ToItemId();
        }

        // ItemId가 없는 경우 기본값 반환
        return new HashDigest<SHA256>(new byte[32]);
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
