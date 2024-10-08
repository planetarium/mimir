using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.RuneState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record RuneState : IBencodable
{
    public int RuneId { get; init; }
    public int Level { get; init; }

    public IValue Bencoded => List.Empty.Add(RuneId.Serialize()).Add(Level.Serialize());

    public RuneState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        RuneId = l[0].ToInteger();
        Level = l[1].ToInteger();
    }
}
