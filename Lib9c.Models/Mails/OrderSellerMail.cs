using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;

namespace Lib9c.Models.Mails;

/// <summary>
/// <see cref="Nekoyume.Model.Mail.OrderSellerMail"/>
/// </summary>
public record OrderSellerMail : Mail
{
    public Guid OrderId { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add(OrderIdKey, OrderId.Serialize());

    public OrderSellerMail(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                [ValueKind.Dictionary],
                bencoded.Kind);
        }

        OrderId = d[OrderIdKey].ToGuid();
    }
}
