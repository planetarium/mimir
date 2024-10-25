using System.Text.Json.Serialization;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.OrderLock"/>
/// </summary>
public record OrderLock : Lock
{
    public Guid OrderId { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded => new List(
        Type.Serialize(),
        OrderId.Serialize());

    public OrderLock()
    {
    }

    public OrderLock(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Type = l[0].ToEnum<LockType>();
        OrderId = l[1].ToGuid();
    }
}
