using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.WorldInformation;

/// <summary>
/// <see cref="Nekoyume.Model.WorldInformation.World"/>
/// </summary>
[BsonIgnoreExtraElements]
public record World : IBencodable
{
    public int Id { get; init; }
    public string Name { get; init; }
    public int StageBegin { get; init; }
    public int StageEnd { get; init; }
    public long UnlockedBlockIndex { get; init; }
    public long StageClearedBlockIndex { get; init; }
    public int StageClearedId { get; init; }

    public bool IsUnlocked => UnlockedBlockIndex != -1;
    public bool IsStageCleared => StageClearedBlockIndex != -1;

    public World(IValue bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind
            );
        }

        Id = d.GetInteger("Id");
        Name = d.GetString("Name");
        StageBegin = d.GetInteger("StageBegin");
        StageEnd = d.GetInteger("StageEnd");
        UnlockedBlockIndex = d.GetLong("UnlockedBlockIndex");
        StageClearedBlockIndex = d.GetLong("StageClearedBlockIndex");
        StageClearedId = d.GetInteger("StageClearedId");
    }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded =>
        new Dictionary(
            new Dictionary<IKey, IValue>
            {
                [(Text)"Id"] = Id.Serialize(),
                [(Text)"Name"] = Name.Serialize(),
                [(Text)"StageBegin"] = StageBegin.Serialize(),
                [(Text)"StageEnd"] = StageEnd.Serialize(),
                [(Text)"UnlockedBlockIndex"] = UnlockedBlockIndex.Serialize(),
                [(Text)"StageClearedBlockIndex"] = StageClearedBlockIndex.Serialize(),
                [(Text)"StageClearedId"] = StageClearedId.Serialize(),
            }
        );
}
