using System.Text.Json.Serialization;
using Bencodex.Types;
using Lib9c.Models.AttachmentActionResults;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Factories;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.CombinationSlotState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record CombinationSlotState : State
{
    private const string UnlockBlockIndexKey = "unlockBlockIndex";
    private const string StartBlockIndexKey = "startBlockIndex";
    private const string ResultKey = "result";
    private const string PetIdKey = "petId";
    private const string IndexKey = "index";
    private const string IsUnlockedKey = "isUnlocked";

    public long UnlockBlockIndex { get; init; }
    public long StartBlockIndex { get; init; }
    public AttachmentActionResult? Result { get; init; }
    public int? PetId { get; init; }
    public int Index { get; init; }
    public bool IsUnlocked { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded
    {
        get
        {
            var values = new Dictionary<IKey, IValue>
            {
                [(Text)UnlockBlockIndexKey] = UnlockBlockIndex.Serialize(),
                [(Text)StartBlockIndexKey] = StartBlockIndex.Serialize(),
                [(Text)IndexKey] = (Integer)Index,
                [(Text)IsUnlockedKey] = IsUnlocked.Serialize(),
            };

            if (Result is not null)
            {
                values.Add((Text)ResultKey, Result.Bencoded);
            }

            if (PetId is not null)
            {
                values.Add((Text)PetIdKey, PetId.Serialize());
            }

#pragma warning disable LAA1002
            return new Dictionary(values.Union((Dictionary) base.BencodedAsDictionaryV1));
#pragma warning restore LAA1002
        }
    }

    public CombinationSlotState()
    {
        IsUnlocked = Index < Nekoyume.Model.State.AvatarState.DefaultCombinationSlotCount;
    }

    public CombinationSlotState(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind
            );
        }

        UnlockBlockIndex = d[UnlockBlockIndexKey].ToLong();

        if (d.TryGetValue((Text)IndexKey, out var index))
        {
            Index = (Integer)index;
        }

        if (d.TryGetValue((Text)ResultKey, out var result))
        {
            Result = AttachmentActionResultFactory.Create(result);
        }

        if (d.TryGetValue((Text)StartBlockIndexKey, out var value))
        {
            StartBlockIndex = value.ToLong();
        }

        if (d.TryGetValue((Text)PetIdKey, out var petId))
        {
            PetId = petId.ToNullableInteger();
        }

        if (d.TryGetValue((Text)IsUnlockedKey, out var isUnlocked))
        {
            IsUnlocked = isUnlocked.ToBoolean();
        }
        else
        {
            IsUnlocked = Index < Nekoyume.Model.State.AvatarState.DefaultCombinationSlotCount;
        }
    }
}
