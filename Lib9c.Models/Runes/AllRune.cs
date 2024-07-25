using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Runes;

public record AllRune : IBencodable
{
    public Dictionary<int, Rune> Runes { get; }

    public IValue Bencoded => new List(Runes
        .OrderBy(e => e.Key)
        .Select(e => e.Value.Bencoded));

    public AllRune(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                [ValueKind.List],
                bencoded.Kind);
        }

        Runes = new Dictionary<int, Rune>();
        foreach (var e in l)
        {
            var runeState = new Rune(e);
            Runes.Add(runeState.RuneId, runeState);
        }
    }
}
