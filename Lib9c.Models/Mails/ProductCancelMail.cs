using Bencodex.Types;
using Lib9c.Models.Exceptions;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.ProductCancelMail"/>
/// </summary>
[BsonIgnoreExtraElements]
public record ProductCancelMail : Mail
{
    public Guid ProductId { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add(ProductIdKey, ProductId.Serialize());

    public ProductCancelMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        ProductId = d[ProductIdKey].ToGuid();
    }
}
