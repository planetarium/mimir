using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Stake;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

public record StakeState : IBencodable
{
    public const string StateTypeName = "stake_state";
    public const int StateTypeVersion = 2;
    public Contract Contract { get; init; }
    public long StartedBlockIndex { get; init; }
    public long ReceivedBlockIndex { get; init; }

    public StakeState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        if (
            l[0] is not Text stateTypeNameValue
            || stateTypeNameValue != StateTypeName
            || l[1] is not Integer stateTypeVersionValue
            || stateTypeVersionValue.Value != StateTypeVersion
        )
        {
            throw new ArgumentException(
                nameof(bencoded),
                $"{nameof(bencoded)} doesn't have valid header."
            );
        }

        const int reservedCount = 2;

        Contract = new Contract(l[reservedCount]);
        StartedBlockIndex = (Integer)l[reservedCount + 1];
        ReceivedBlockIndex = (Integer)l[reservedCount + 2];
    }

    public IValue Bencoded =>
        new List(
            (Text)StateTypeName,
            (Integer)StateTypeVersion,
            Contract.Bencoded,
            (Integer)StartedBlockIndex,
            (Integer)ReceivedBlockIndex
        );
}
