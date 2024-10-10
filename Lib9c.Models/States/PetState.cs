using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.PetState"/>
/// </summary>
[BsonIgnoreExtraElements]
public class PetState : IBencodable
{
    public int PetId { get; init; }
    public int Level { get; init; }
    public long UnlockedBlockIndex { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => List.Empty
        .Add(PetId.Serialize())
        .Add(Level.Serialize())
        .Add(UnlockedBlockIndex.Serialize());

    public PetState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        PetId = l[0].ToInteger();
        Level = l[1].ToInteger();
        UnlockedBlockIndex = l[2].ToLong();
    }
}
