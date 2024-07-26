using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Lib9c.Models.Mails;
using Libplanet.Crypto;
using Nekoyume.Model;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;
using static Lib9c.SerializeKeys;

namespace Lib9c.Models.States;

/// <summary>
/// <see cref="Nekoyume.Model.State.AvatarState"/>
/// </summary>
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
    public long DailyRewardReceivedIndex { get; init; }
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

        if (bencoded is Dictionary d)
        {
            Version = 1;
            var nameKey = NameKey;
            var characterIdKey = CharacterIdKey;
            var levelKey = LevelKey;
            var expKey = ExpKey;
            var inventoryKey = LegacyInventoryKey;
            var worldInformationKey = LegacyWorldInformationKey;
            var updatedAtKey = UpdatedAtKey;
            var agentAddressKey = AgentAddressKey;
            var questListKey = LegacyQuestListKey;
            var mailBoxKey = MailBoxKey;
            var blockIndexKey = BlockIndexKey;
            var dailyRewardReceivedIndexKey = DailyRewardReceivedIndexKey;
            var actionPointKey = ActionPointKey;
            var stageMapKey = StageMapKey;
            var monsterMapKey = MonsterMapKey;
            var itemMapKey = ItemMapKey;
            var eventMapKey = EventMapKey;
            var hairKey = HairKey;
            var lensKey = LensKey;
            var earKey = EarKey;
            var tailKey = TailKey;
            var combinationSlotAddressesKey = CombinationSlotAddressesKey;
            var rankingMapAddressKey = RankingMapAddressKey;
            if (d.ContainsKey(LegacyNameKey))
            {
                nameKey = LegacyNameKey;
                characterIdKey = LegacyCharacterIdKey;
                levelKey = LegacyLevelKey;
                updatedAtKey = LegacyUpdatedAtKey;
                agentAddressKey = LegacyAgentAddressKey;
                mailBoxKey = LegacyMailBoxKey;
                blockIndexKey = LegacyBlockIndexKey;
                dailyRewardReceivedIndexKey = LegacyDailyRewardReceivedIndexKey;
                actionPointKey = LegacyActionPointKey;
                stageMapKey = LegacyStageMapKey;
                monsterMapKey = LegacyMonsterMapKey;
                itemMapKey = LegacyItemMapKey;
                eventMapKey = LegacyEventMapKey;
                hairKey = LegacyHairKey;
                earKey = LegacyEarKey;
                tailKey = LegacyTailKey;
                combinationSlotAddressesKey = LegacyCombinationSlotAddressesKey;
                rankingMapAddressKey = LegacyRankingMapAddressKey;
            }

            Name = d[nameKey].ToDotnetString();
            CharacterId = (int)((Integer)d[characterIdKey]).Value;
            Level = (int)((Integer)d[levelKey]).Value;
            Exp = (long)((Integer)d[expKey]).Value;
            UpdatedAt = d[updatedAtKey].ToLong();
            AgentAddress = d[agentAddressKey].ToAddress();
            MailBox = new MailBox((List)d[mailBoxKey]);
            BlockIndex = (long)((Integer)d[blockIndexKey]).Value;
            DailyRewardReceivedIndex = (long)((Integer)d[dailyRewardReceivedIndexKey]).Value;
            ActionPoint = (int)((Integer)d[actionPointKey]).Value;
            StageMap = new CollectionMap((Dictionary)d[stageMapKey]);
            d.TryGetValue((Text)monsterMapKey, out var value2);
            MonsterMap = value2 is null ? new CollectionMap() : new CollectionMap((Dictionary)value2);
            ItemMap = new CollectionMap((Dictionary)d[itemMapKey]);
            EventMap = new CollectionMap((Dictionary)d[eventMapKey]);
            Hair = (int)((Integer)d[hairKey]).Value;
            Lens = (int)((Integer)d[lensKey]).Value;
            Ear = (int)((Integer)d[earKey]).Value;
            Tail = (int)((Integer)d[tailKey]).Value;
            CombinationSlotAddresses = d[combinationSlotAddressesKey].ToList(StateExtensions.ToAddress);
            RankingMapAddress = d[rankingMapAddressKey].ToAddress();

            if (d.ContainsKey(inventoryKey) ||
                d.ContainsKey(worldInformationKey) ||
                d.ContainsKey(questListKey))
            {
                Version = 0;
            }
        }

        throw new UnsupportedArgumentTypeException<ValueKind>(
            nameof(bencoded),
            [ValueKind.List, ValueKind.Dictionary],
            bencoded.Kind);
    }
}
