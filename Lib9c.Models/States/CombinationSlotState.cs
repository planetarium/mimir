using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

public class CombinationSlotState : IBencodable
{
    public long UnlockBlockIndex { get; init; }
    public int UnlockStage { get; init; }

    public long StartBlockIndex { get; init; }

    // public AttachmentActionResult? Result { get; init; }
    public int? PetId { get; init; }

    public CombinationSlotState(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind
            );
        }

        UnlockBlockIndex = d["unlockBlockIndex"].ToLong();
        UnlockStage = d["unlockStage"].ToInteger();

        // if (d.TryGetValue((Text)"result", out var result))
        // {
        //     Result = AttachmentActionResult.Deserialize((Dictionary)result);
        // }

        if (d.TryGetValue((Text)"startBlockIndex", out var startIndex))
        {
            StartBlockIndex = startIndex.ToLong();
        }

        if (d.TryGetValue((Text)"petId", out var petId))
        {
            PetId = petId.ToNullableInteger();
        }
    }

    public IValue Bencoded => Serialize();

    public IValue Serialize()
    {
        var values = new Dictionary<IKey, IValue>
        {
            [(Text)"unlockBlockIndex"] = UnlockBlockIndex.Serialize(),
            [(Text)"unlockStage"] = UnlockStage.Serialize(),
            [(Text)"startBlockIndex"] = StartBlockIndex.Serialize(),
        };

        // if (Result != null)
        // {
        //     values.Add((Text)"result", Result.Serialize());
        // }

        if (PetId.HasValue)
        {
            values.Add((Text)"petId", PetId.Value.Serialize());
        }

        return new Dictionary(values);
    }
}
