using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

public record ProductsState : IBencodable
{
    public List<Guid> ProductIds = new List<Guid>();

    public IValue Bencoded =>
        ProductIds.Aggregate(
            List.Empty,
            (current, productId) => current.Add(productId.Serialize())
        );

    public ProductsState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        ProductIds = l.ToList(StateExtensions.ToGuid);
    }

    public ProductsState(List bencoded)
    {
        ProductIds = bencoded.ToList(StateExtensions.ToGuid);
    }
}
