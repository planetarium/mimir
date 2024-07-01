using Bencodex.Types;
using Libplanet.Crypto;
using Nekoyume.Model.Item;
using Nekoyume.Model.State;

namespace Mimir.Models;

public class AvatarState : StateModel
{
    public string Name { get; private set; }
    public int CharacterId { get; private set; }
    public int Level { get; private set; }
    public long Exp { get; private set; }
    public Inventory Inventory { get; private set; }
    public long UpdatedAt { get; private set; }
    public Address AgentAddress { get; private set; }
    public List MailBox { get; private set; }
    public long BlockIndex { get; private set; }
    public long DailyRewardReceivedIndex { get; private set; }
    public int ActionPoint { get; private set; }
    public Dictionary StageMap { get; private set; }
    public Dictionary MonsterMap { get; private set; }
    public Dictionary ItemMap { get; private set; }
    public Dictionary EventMap { get; private set; }
    public int Hair { get; private set; }
    public int Lens { get; private set; }
    public int Ear { get; private set; }
    public int Tail { get; private set; }
    public List<Address> CombinationSlotAddresses { get; private set; }
    public string NameWithHash { get; private set; }
    public int Version { get; private set; }
    public readonly Address RankingMapAddress;

    public AvatarState(List serialized)
        : base(serialized[0].ToAddress())
    {
        Version = (int)((Integer)serialized[3]).Value;
        Name = serialized[4].ToDotnetString();
        CharacterId = (int)((Integer)serialized[5]).Value;
        Level = (int)((Integer)serialized[6]).Value;
        Exp = (long)((Integer)serialized[7]).Value;
        UpdatedAt = serialized[8].ToLong();
        AgentAddress = serialized[9].ToAddress();
        MailBox = (List)serialized[10];
        BlockIndex = (long)((Integer)serialized[11]).Value;
        DailyRewardReceivedIndex = (long)((Integer)serialized[12]).Value;
        ActionPoint = (int)((Integer)serialized[13]).Value;
        StageMap = (Dictionary)serialized[14];
        MonsterMap = (Dictionary)serialized[15];
        ItemMap = (Dictionary)serialized[16];
        EventMap = (Dictionary)serialized[17];
        Hair = (int)((Integer)serialized[18]).Value;
        Lens = (int)((Integer)serialized[19]).Value;
        Ear = (int)((Integer)serialized[20]).Value;
        Tail = (int)((Integer)serialized[21]).Value;
        CombinationSlotAddresses = serialized[22].ToList(StateExtensions.ToAddress);
        RankingMapAddress = serialized[23].ToAddress();
    }
}
