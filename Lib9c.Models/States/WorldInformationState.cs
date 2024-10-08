using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.WorldInformation;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

// FIXME: correct namespace. Nekoyume.Model.WorldInformation
/// <summary>
/// <see cref="Nekoyume.Model.WorldInformation"/>
/// </summary>
[BsonIgnoreExtraElements]
public record WorldInformationState : IBencodable
{
    public Dictionary<int, World> WorldDictionary { get; init; }

    public WorldInformationState(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind
            );
        }

        WorldDictionary = d.ToDictionary(kv => kv.Key.ToInteger(), kv => new World(kv.Value));
    }

    public IValue Bencoded =>
        WorldDictionary.Aggregate(
            List.Empty,
            (current, kv) => current.Add(List.Empty.Add(kv.Key.Serialize()).Add(kv.Value.Bencoded))
        );
}
