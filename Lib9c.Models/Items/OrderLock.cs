using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Items;

/// <summary>
/// <see cref="Nekoyume.Model.Item.OrderLock"/>
/// </summary>
public record OrderLock : Lock
{
    public Guid OrderId { get; init; }

    public override IValue Bencoded => new List(
        base.Bencoded,
        OrderId.Serialize());

    public OrderLock()
    {
    }

    public OrderLock(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }
        
        OrderId = l[1].ToGuid();
    }
}
