using System.Text.Json.Serialization;
using Bencodex.Types;
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
public record AvatarState : State
{
    public int Version { get; init; }
    public string Name { get; init; }
    public int CharacterId { get; init; }
    public int Level { get; init; }
    public long Exp { get; init; }
    public long UpdatedAt { get; init; }
    public Address AgentAddress { get; init; }
    public MailBox MailBox { get; init; }
    public long BlockIndex { get; init; }
    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public long DailyRewardReceivedIndex { get; init; }
    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public int ActionPoint { get; init; }
    public CollectionMap StageMap { get; init; }
    public CollectionMap MonsterMap { get; init; }
    public CollectionMap ItemMap { get; init; }
    public CollectionMap EventMap { get; init; }
    public int Hair { get; init; }
    public int Lens { get; init; }
    public int Ear { get; init; }
    public int Tail { get; init; }
    public List<Address> CombinationSlotAddresses { get; init; }
    public Address RankingMapAddress { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public override IValue Bencoded => new List(
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
        CombinationSlotAddresses
            .OrderBy(e => e)
            .Select(e => e.Serialize())
            .Serialize(),
        RankingMapAddress.Serialize());

    public AvatarState(IValue bencoded) : base(((List)bencoded)[0])
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
            bencoded.Kind);
    }

    public AvatarState()
    {
        
    }
}
