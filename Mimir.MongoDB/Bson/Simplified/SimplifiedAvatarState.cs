using Bencodex.Types;
using HotChocolate;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Extensions;
using Lib9c.Models.Mails;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.AvatarState"/>
/// </summary>
[BsonIgnoreExtraElements]
public record SimplifiedAvatarState : State
{
    public int Version { get; init; }
    public string Name { get; init; }
    public int CharacterId { get; init; }
    public int Level { get; init; }
    public long Exp { get; init; }
    public long UpdatedAt { get; init; }
    public Address AgentAddress { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public MailBox MailBox { get; init; }
    public long BlockIndex { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public long DailyRewardReceivedIndex { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public int ActionPoint { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public CollectionMap StageMap { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public CollectionMap MonsterMap { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public CollectionMap ItemMap { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public CollectionMap EventMap { get; init; }
    public int Hair { get; init; }
    public int Lens { get; init; }
    public int Ear { get; init; }
    public int Tail { get; init; }
    public List<Address> CombinationSlotAddresses { get; init; }
    public Address RankingMapAddress { get; init; }

    [BsonIgnore, GraphQLIgnore]
    public override IValue Bencoded =>
        new List(
            base.Bencoded,
            (Integer)Version,
            (Text)Name,
            (Integer)CharacterId,
            (Integer)Level,
            (Integer)Exp,
            UpdatedAt.Serialize(),
            AgentAddress.Serialize(),
            MailBox.Bencoded,
            (Integer)BlockIndex,
            (Integer)DailyRewardReceivedIndex,
            (Integer)ActionPoint,
            StageMap.Serialize(),
            MonsterMap.Serialize(),
            ItemMap.Serialize(),
            EventMap.Serialize(),
            (Integer)Hair,
            (Integer)Lens,
            (Integer)Ear,
            (Integer)Tail,
            CombinationSlotAddresses.OrderBy(e => e).Select(e => e.Serialize()).Serialize(),
            RankingMapAddress.Serialize()
        );

    public SimplifiedAvatarState(IValue bencoded)
        : base(((List)bencoded)[0])
    {
        if (bencoded is List l)
        {
            Version = (int)((Integer)l[1]).Value;
            Name = l[2].ToDotnetString();
            CharacterId = (int)((Integer)l[3]).Value;
            Level = (int)((Integer)l[4]).Value;
            Exp = (long)((Integer)l[5]).Value;
            UpdatedAt = l[6].ToLong();
            AgentAddress = l[7].ToAddress();
            MailBox = new MailBox((List)l[8]);
            BlockIndex = (long)((Integer)l[9]).Value;
            DailyRewardReceivedIndex = (long)((Integer)l[10]).Value;
            ActionPoint = (int)((Integer)l[11]).Value;
            StageMap = new CollectionMap((Dictionary)l[12]);
            MonsterMap = new CollectionMap((Dictionary)l[13]);
            ItemMap = new CollectionMap((Dictionary)l[14]);
            EventMap = new CollectionMap((Dictionary)l[15]);
            Hair = (int)((Integer)l[16]).Value;
            Lens = (int)((Integer)l[17]).Value;
            Ear = (int)((Integer)l[18]).Value;
            Tail = (int)((Integer)l[19]).Value;
            CombinationSlotAddresses = l[20].ToList(StateExtensions.ToAddress);
            RankingMapAddress = l[21].ToAddress();
            return;
        }

        throw new UnsupportedArgumentTypeException<ValueKind>(
            nameof(bencoded),
            new[] { ValueKind.List },
            bencoded.Kind
        );
    }

    public SimplifiedAvatarState(
        Address address,
        int version,
        string name,
        int characterId,
        int level,
        long exp,
        long updatedAt,
        Address agentAddress,
        MailBox mailBox,
        long blockIndex,
        long dailyRewardReceivedIndex,
        int actionPoint,
        CollectionMap stageMap,
        CollectionMap monsterMap,
        CollectionMap itemMap,
        CollectionMap eventMap,
        int hair,
        int lens,
        int ear,
        int tail,
        List<Address> combinationSlotAddresses,
        Address rankingMapAddress)
        : base(new List(address.Bencoded))
    {
        Version = version;
        Name = name;
        CharacterId = characterId;
        Level = level;
        Exp = exp;
        UpdatedAt = updatedAt;
        AgentAddress = agentAddress;
        MailBox = mailBox;
        BlockIndex = blockIndex;
        DailyRewardReceivedIndex = dailyRewardReceivedIndex;
        ActionPoint = actionPoint;
        StageMap = stageMap;
        MonsterMap = monsterMap;
        ItemMap = itemMap;
        EventMap = eventMap;
        Hair = hair;
        Lens = lens;
        Ear = ear;
        Tail = tail;
        CombinationSlotAddresses = combinationSlotAddresses;
        RankingMapAddress = rankingMapAddress;        
    }

    public static SimplifiedAvatarState FromAvatarState(AvatarState avatarState)
    {
        return new SimplifiedAvatarState(
            avatarState.Address,
            avatarState.Version,
            avatarState.Name,
            avatarState.CharacterId,
            avatarState.Level,
            avatarState.Exp,
            avatarState.UpdatedAt,
            avatarState.AgentAddress,
            avatarState.MailBox,
            avatarState.BlockIndex,
            avatarState.DailyRewardReceivedIndex,
            avatarState.ActionPoint,
            avatarState.StageMap,
            avatarState.MonsterMap,
            avatarState.ItemMap,
            avatarState.EventMap,
            avatarState.Hair,
            avatarState.Lens,
            avatarState.Ear,
            avatarState.Tail,
            avatarState.CombinationSlotAddresses,
            avatarState.RankingMapAddress
        );
    }
}
