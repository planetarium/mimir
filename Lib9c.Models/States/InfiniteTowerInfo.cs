using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.InfiniteTower.InfiniteTowerInfo"/>
/// </summary>
[BsonIgnoreExtraElements]
public record InfiniteTowerInfo : IBencodable
{
    public Address Address { get; init; }
    public int InfiniteTowerId { get; init; }
    public int ClearedFloor { get; init; }
    public int RemainingTickets { get; init; }
    public int TotalTicketsUsed { get; init; }
    public int NumberOfTicketPurchases { get; init; }
    public long LastResetBlockIndex { get; init; }
    public long LastTicketRefillBlockIndex { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => List.Empty
        .Add(Address.Serialize())
        .Add(InfiniteTowerId.Serialize())
        .Add(ClearedFloor.Serialize())
        .Add(RemainingTickets.Serialize())
        .Add(TotalTicketsUsed.Serialize())
        .Add(NumberOfTicketPurchases.Serialize())
        .Add(LastResetBlockIndex.Serialize())
        .Add(LastTicketRefillBlockIndex.Serialize());

    public InfiniteTowerInfo()
    {
    }

    public InfiniteTowerInfo(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        Address = l[0].ToAddress();
        InfiniteTowerId = l[1].ToInteger();
        ClearedFloor = l[2].ToInteger();
        RemainingTickets = l[3].ToInteger();
        TotalTicketsUsed = l[4].ToInteger();
        NumberOfTicketPurchases = l[5].ToInteger();
        LastResetBlockIndex = l[6].ToLong();
        LastTicketRefillBlockIndex = l[7].ToLong();
    }
}





