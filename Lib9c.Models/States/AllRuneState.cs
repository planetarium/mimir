using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.AllRuneState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record AllRuneState : IBencodable
{
    public Dictionary<int, RuneState> Runes { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => new List(Runes
        .OrderBy(e => e.Key)
        .Select(e => e.Value.Bencoded));

    public AllRuneState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Runes = new Dictionary<int, RuneState>();
        foreach (var e in l)
        {
            var runeState = new RuneState(e);
            Runes.Add(runeState.RuneId, runeState);
        }
    }
}
