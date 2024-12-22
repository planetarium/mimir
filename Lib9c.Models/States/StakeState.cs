using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Stake;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.StakeState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record StakeState : IBencodable
{
    public Contract Contract { get; init; }
    public string StateTypeName;
    public int StateTypeVersion;
    public long StartedBlockIndex { get; init; }
    public long ReceivedBlockIndex { get; init; }

    public StakeState() { }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded =>
        new List(
            (Text)StateTypeName,
            (Integer)StateTypeVersion,
            Contract.Bencoded,
            (Integer)StartedBlockIndex,
            (Integer)ReceivedBlockIndex
        );

    public StakeState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        const int reservedCount = 2;

        StateTypeName = (Text)l[0];
        StateTypeVersion = (Integer)l[1];
        Contract = new Contract(l[reservedCount]);
        StartedBlockIndex = (Integer)l[reservedCount + 1];
        ReceivedBlockIndex = (Integer)l[reservedCount + 2];
    }
}
