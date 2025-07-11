using System.Text.Json.Serialization;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.Costume"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Costume : ItemBase
{
    public bool Equipped { get; init; }
    public string SpineResourcePath { get; init; }
    public Guid ItemId { get; init; }
    public long RequiredBlockIndex { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded
    {
        get
        {
            var d = ((Dictionary)base.Bencoded)
                .Add("equipped", Equipped.Serialize())
                .Add("spine_resource_path", SpineResourcePath.Serialize())
                .Add(LegacyCostumeItemIdKey, ItemId.Serialize());

            return RequiredBlockIndex > 0
                ? d.Add(RequiredBlockIndexKey, RequiredBlockIndex.Serialize())
                : d;
        }
    }

    public Costume()
    {
    }

    public Costume(IValue bencoded) : base(bencoded)
    {
        try
        {
            var costume = (Nekoyume.Model.Item.Costume)Nekoyume.Model.Item.ItemFactory.Deserialize(bencoded);
            Equipped = costume.Equipped;
            SpineResourcePath = costume.SpineResourcePath;
            ItemId = costume.ItemId;
            RequiredBlockIndex = costume.RequiredBlockIndex;
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
