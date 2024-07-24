using Bencodex;
using Bencodex.Types;
using Mimir.Models.Exceptions;
using Mimir.Models.Factories;
using ValueKind = Bencodex.Types.ValueKind;

namespace Mimir.Models.Mails;

public record MailBox : IBencodable
{
    public List<Mail> Mails { get; init; }

    public IValue Bencoded => new List(Mails
        .OrderBy(i => i.Id)
        .Select(m => m.Bencoded));

    public MailBox(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.List],
                bencoded.Kind);
        }

        Mails = l
            .Select(MailFactory.Create)
            .ToList();
    }
}
