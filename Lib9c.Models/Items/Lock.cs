using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.ILock"/>
/// </summary>
public record Lock : IBencodable, ILock
{
    public LockType Type { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public virtual IValue Bencoded => new List(Type.Serialize());

    public Lock()
    {
    }

    public Lock(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Type = l[0].ToEnum<LockType>();
    }

    public IValue Serialize() => Bencoded;
}
