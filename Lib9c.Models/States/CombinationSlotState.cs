using Bencodex;
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
public record CombinationSlotState : IBencodable
{
    public long UnlockBlockIndex { get; init; }
    public int UnlockStage { get; init; }
    public long StartBlockIndex { get; init; }
    public AttachmentActionResult? Result { get; init; }
    public int? PetId { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public IValue Bencoded
    {
        get
        {
            var values = new Dictionary<IKey, IValue>
            {
                [(Text)"unlockBlockIndex"] = UnlockBlockIndex.Serialize(),
                [(Text)"unlockStage"] = UnlockStage.Serialize(),
                [(Text)"startBlockIndex"] = StartBlockIndex.Serialize(),
            };

            if (Result != null)
            {
                values.Add((Text)"result", Result.Bencoded);
            }

            if (PetId.HasValue)
            {
                values.Add((Text)"petId", PetId.Value.Serialize());
            }

            return new Dictionary(values);
        }
    }

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
        //     Result = AttachmentActionResultFactory.Create(result);
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
}
