using Bencodex.Types;
using Lib9c.Models.Exceptions;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;
using Lib9c.Models.Extensions;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.OrderExpirationMail"/>
/// </summary>
public record OrderExpirationMail : Mail
{
    public Guid OrderId { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add(OrderIdKey, OrderId.Serialize());

    public OrderExpirationMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        OrderId = d[OrderIdKey].ToGuid();
    }
}
