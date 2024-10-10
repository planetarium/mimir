using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.AllCombinationSlotState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record AllCombinationSlotState : IBencodable
{
    public Dictionary<int, CombinationSlotState> CombinationSlots { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => CombinationSlots
        .OrderBy(kvp => kvp.Key)
        .Aggregate(
            List.Empty,
            (current, combinationSlot) => current.Add(combinationSlot.Value.Bencoded));

    public AllCombinationSlotState()
    {
    }

    public AllCombinationSlotState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        CombinationSlots = new Dictionary<int, CombinationSlotState>();
        foreach (var item in l.OfType<Dictionary>())
        {
            var slotState = new CombinationSlotState(item);
            CombinationSlots.Add(slotState.Index, slotState);
        }
    }
}
