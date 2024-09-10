using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

public class WorldBossKillRewardRecord : IBencodable
{
    public Dictionary<int, bool> RewardRecordDictionary { get; init; }

    public WorldBossKillRewardRecord(IValue bencoded)
    {
        if (bencoded is not List list)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        RewardRecordDictionary = new Dictionary<int, bool>();

        foreach (var iValue in list)
        {
            var pair = (List)iValue;
            var key = pair[0].ToInteger();
            var value = pair[1].ToBoolean();
            RewardRecordDictionary[key] = value;
        }
    }

    public IValue Bencoded =>
        RewardRecordDictionary
            .OrderBy(kv => kv.Key)
            .Aggregate(
                List.Empty,
                (current, kv) =>
                    current.Add(List.Empty.Add(kv.Key.Serialize()).Add(kv.Value.Serialize()))
            );
}
