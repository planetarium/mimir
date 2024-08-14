using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Factories;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Mails;

public record MailBox : IBencodable
{
    public List<Mail> Mails { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public IValue Bencoded => new List(Mails
        .OrderBy(i => i.Id)
        .Select(m => m.Bencoded));

    public MailBox(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Mails = l
            .Select(MailFactory.Create)
            .ToList();
    }
}
