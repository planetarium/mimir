using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.ProductBuyerMail"/>
/// </summary>
public record ProductBuyerMail : Mail
{
    public Guid ProductId { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add(ProductIdKey, ProductId.Serialize());

    public ProductBuyerMail(IValue bencoded) : base(bencoded)
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
