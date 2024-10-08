using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.CollectionState"/>
/// </summary>
[BsonIgnoreExtraElements]
public class CollectionState : IBencodable
{
    public SortedSet<int> Ids { get; init; } = new();

    public CollectionState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        var rawList = (List)l[0];
        foreach (var value in rawList)
        {
            Ids.Add((Integer)value);
        }
    }

    public IValue Bencoded => List.Empty.Add(new List(Ids));
}
