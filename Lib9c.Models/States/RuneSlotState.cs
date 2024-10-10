using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Runes;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.EnumType;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.RuneSlotState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record RuneSlotState : IBencodable
{
    public BattleType BattleType { get; init; }

    public List<RuneSlot> Slots { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded =>
        List.Empty.Add(BattleType.Serialize()).Add(new List(Slots.Select(x => x.Bencoded)));

    public RuneSlotState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        BattleType = l[0].ToEnum<BattleType>();
        Slots = ((List)l[1]).Select(x => new RuneSlot(x)).ToList();
        if (Slots.Count == 6)
        {
            Slots.Add(new RuneSlot(6, RuneSlotType.Crystal, RuneType.Stat, true));
            Slots.Add(new RuneSlot(7, RuneSlotType.Crystal, RuneType.Skill, true));
        }
    }

    public RuneSlotState(BattleType battleType, List<RuneSlot> slots)
    {
        BattleType = battleType;
        Slots = slots;
    }
}
