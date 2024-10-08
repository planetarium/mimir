using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Arena;

[BsonIgnoreExtraElements]
public record ArenaScore : IBencodable
{
    public Address Address { get; init; }
    public int Score { get; init; }
    
    [BsonIgnore, GraphQLIgnore, JsonIgnore]
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
