using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.EnumType;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Runes;

/// <summary>
/// <see cref="Nekoyume.Model.Rune.RuneSlot"/>
/// </summary>
[BsonIgnoreExtraElements]
public record RuneSlot : IBencodable
{
    public int Index { get; init; }
    public RuneSlotType RuneSlotType { get; init; }
    public RuneType RuneType { get; init; }
    public bool IsLock { get; init; }
    public int? RuneId { get; init; }

    public RuneSlot() { }

    public IValue Bencoded
    {
        get
        {
            var l = List
                .Empty.Add(Index.Serialize())
                .Add(RuneSlotType.Serialize())
                .Add(RuneType.Serialize())
                .Add(IsLock.Serialize());

            if (RuneId.HasValue)
            {
                l = l.Add(RuneId.Serialize());
            }

            return l;
        }
    }

    public RuneSlot(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        Index = l[0].ToInteger();
        RuneSlotType = l[1].ToEnum<RuneSlotType>();
        RuneType = l[2].ToEnum<RuneType>();
        IsLock = l[3].ToBoolean();
        if (l.Count > 4)
        {
            RuneId = l[4].ToNullableInteger();
        }
    }

    public RuneSlot(
        int index,
        RuneSlotType runeSlotType,
        RuneType runeType,
        bool isLock,
        int? runeId = null
    )
    {
        Index = index;
        RuneSlotType = runeSlotType;
        RuneType = runeType;
        IsLock = isLock;
        RuneId = runeId;
    }
}
