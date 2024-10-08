using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using MongoDB.Bson.Serialization.Attributes;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Stake;

/// <summary>
/// <see cref="Nekoyume.Model.Stake.Contract"/>
/// </summary>
[BsonIgnoreExtraElements]
public record Contract : IBencodable
{
    public const string StateTypeName = "stake_contract";
    public const long StateTypeVersion = 1;

    public const string StakeRegularFixedRewardSheetPrefix = "StakeRegularFixedRewardSheet_";

    public const string StakeRegularRewardSheetPrefix = "StakeRegularRewardSheet_";

    public string StakeRegularFixedRewardSheetTableName { get; init; }
    public string StakeRegularRewardSheetTableName { get; init; }
    public long RewardInterval { get; init; }
    public long LockupInterval { get; init; }

    public Contract(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        if (l[0] is not Text typeName || (string)typeName != StateTypeName)
        {
            throw new ArgumentException(
                nameof(bencoded),
                $"State type name is not {StateTypeName}"
            );
        }

        if (l[1] is not Integer typeVersion || (long)typeVersion != StateTypeVersion)
        {
            throw new ArgumentException(
                nameof(bencoded),
                $"State type version is not {StateTypeVersion}"
            );
        }

        const int reservedCount = 2;
        StakeRegularFixedRewardSheetTableName = (Text)l[reservedCount];
        StakeRegularRewardSheetTableName = (Text)l[reservedCount + 1];
        RewardInterval = (Integer)l[reservedCount + 2];
        LockupInterval = (Integer)l[reservedCount + 3];
    }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded =>
        new List(
            (Text)StateTypeName,
            (Integer)StateTypeVersion,
            (Text)StakeRegularFixedRewardSheetTableName,
            (Text)StakeRegularRewardSheetTableName,
            (Integer)RewardInterval,
            (Integer)LockupInterval
        );
}
