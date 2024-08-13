using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Arena;

public record ArenaInformation : IBencodable
{
    public Address Address { get; init; }
    public int Win { get; init; }
    public int Lose { get; init; }
    public int Ticket { get; init; }
    public int TicketResetCount { get; init; }
    public int PurchasedTicketCount { get; init; }

    public IValue Bencoded => List.Empty
        .Add(Address.Serialize())
        .Add(Win)
        .Add(Lose)
        .Add(Ticket)
        .Add(TicketResetCount)
        .Add(PurchasedTicketCount);

    public ArenaInformation(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Address = l[0].ToAddress();
        Win = (Integer)l[1];
        Lose = (Integer)l[2];
        Ticket = (Integer)l[3];
        TicketResetCount = (Integer)l[4];
        PurchasedTicketCount = (Integer)l[5];
    }
}
