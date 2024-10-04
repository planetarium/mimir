using System.Numerics;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

[BsonIgnoreExtraElements]
public record WorldBossState : IBencodable
{
    public int Id { get; init; }
    public int Level { get; init; }
    public BigInteger CurrentHp { get; init; }
    public long StartedBlockIndex { get; init; }
    public long EndedBlockIndex { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public IValue Bencoded =>
        List
            .Empty.Add(Id.Serialize())
            .Add(Level.Serialize())
            .Add(CurrentHp.Serialize())
            .Add(StartedBlockIndex.Serialize())
            .Add(EndedBlockIndex.Serialize());

    public WorldBossState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        Id = l[0].ToInteger();
        Level = l[1].ToInteger();
        CurrentHp = l[2].ToBigInteger();
        StartedBlockIndex = l[3].ToLong();
        EndedBlockIndex = l[4].ToLong();
    }
}
