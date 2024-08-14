using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Arena;

public record ArenaScore : IBencodable
{
    public Address Address { get; init; }
    public int Score { get; init; }
    
    [BsonIgnore, GraphQLIgnore]
    public IValue Bencoded => List.Empty
        .Add(Address.Serialize())
        .Add(Score);

    public ArenaScore(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Address = l[0].ToAddress();
        Score = (Integer)l[1];
    }
}
