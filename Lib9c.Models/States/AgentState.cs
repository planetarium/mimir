using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

public record AgentState : State
{
    public Dictionary<int, Address> AvatarAddresses { get; init; }

    public int MonsterCollectionRound { get; init; }

    public int Version { get; init; }

    public override IValue Bencoded => new List(
        base.Bencoded,
        (Integer)1,
        new Dictionary(AvatarAddresses.Select(
            kv => new KeyValuePair<IKey, IValue>(
                new Binary(BitConverter.GetBytes(kv.Key)),
                kv.Value.Serialize()))),
        new List(Array.Empty<IValue>()),
        MonsterCollectionRound.Serialize());

    public AgentState(IValue bencoded) : base(((List)bencoded)[0])
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        Version = (int)((Integer)l[1]).Value;
        AvatarAddresses = ((IEnumerable<KeyValuePair<IKey, IValue>>)l[2])
            .Where(kv => kv.Key is Binary)
            .ToDictionary(
                kv => BitConverter.ToInt32(((Binary)kv.Key).ToByteArray(), 0),
                kv => kv.Value.ToAddress());
        MonsterCollectionRound = l[4].ToInteger();
    }
}
