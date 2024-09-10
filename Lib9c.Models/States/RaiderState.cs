using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Libplanet.Crypto;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

public class RaiderState : IBencodable
{
    public long TotalScore { get; init; }
    public long HighScore { get; init; }
    public int TotalChallengeCount { get; init; }
    public int RemainChallengeCount { get; init; }
    public int LatestRewardRank { get; init; }
    public long ClaimedBlockIndex { get; init; }
    public long RefillBlockIndex { get; init; }
    public int PurchaseCount { get; init; }
    public int Cp { get; init; }
    public int Level { get; init; }
    public int IconId { get; init; }
    public Address AvatarAddress { get; init; }
    public string AvatarName { get; init; }
    public int LatestBossLevel { get; init; }
    public long UpdatedBlockIndex { get; init; }

    public IValue Bencoded =>
        List
            .Empty.Add(TotalScore.Serialize())
            .Add(HighScore.Serialize())
            .Add(TotalChallengeCount.Serialize())
            .Add(RemainChallengeCount.Serialize())
            .Add(LatestRewardRank.Serialize())
            .Add(ClaimedBlockIndex.Serialize())
            .Add(RefillBlockIndex.Serialize())
            .Add(PurchaseCount.Serialize())
            .Add(Cp.Serialize())
            .Add(Level.Serialize())
            .Add(IconId.Serialize())
            .Add(AvatarAddress.Serialize())
            .Add(AvatarName.Serialize())
            .Add(LatestBossLevel.Serialize())
            .Add(UpdatedBlockIndex.Serialize());

    public RaiderState(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentValueException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind
            );
        }

        TotalScore = l[0].ToLong();
        HighScore = l[1].ToLong();
        TotalChallengeCount = l[2].ToInteger();
        RemainChallengeCount = l[3].ToInteger();
        LatestRewardRank = l[4].ToInteger();
        ClaimedBlockIndex = l[5].ToLong();
        RefillBlockIndex = l[6].ToLong();
        PurchaseCount = l[7].ToInteger();
        Cp = l[8].ToInteger();
        Level = l[9].ToInteger();
        IconId = l[10].ToInteger();
        AvatarAddress = l[11].ToAddress();
        AvatarName = l[12].ToDotnetString();
        LatestBossLevel = l[13].ToInteger();
        UpdatedBlockIndex = l[14].ToLong();
    }
}
