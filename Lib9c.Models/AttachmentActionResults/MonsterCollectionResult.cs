using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using Nekoyume.Model.State;
using Nekoyume.TableData;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;

namespace Lib9c.Models.AttachmentActionResults;

/// <summary>
/// <see cref="Nekoyume.Model.State.MonsterCollectionResult"/>
/// </summary>
public record MonsterCollectionResult : AttachmentActionResult
{
    public Guid Id { get; init; }
    public Address AvatarAddress { get; init; }
    public List<MonsterCollectionRewardSheet.RewardInfo> Rewards { get; init; }

    public override IValue Bencoded => ((Dictionary)base.Bencoded)
        .Add("id", Id.Serialize())
        .Add(AvatarAddressKey, AvatarAddress.Serialize())
        .Add(MonsterCollectionResultKey, new List(Rewards.Select(r => r.Serialize())));

    public MonsterCollectionResult(IValue bencoded) : base(bencoded)
    {
        if (bencoded is not Dictionary d)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.Dictionary },
                bencoded.Kind);
        }

        Id = d["id"].ToGuid();
        AvatarAddress = d[AvatarAddressKey].ToAddress();
        Rewards = d[MonsterCollectionResultKey]
            .ToList(s => new MonsterCollectionRewardSheet.RewardInfo((Dictionary)s));
    }
}
