using Nekoyume.Model.State;

namespace Mimir.Models;

public class Avatar(
    string agentAddress,
    string avatarAddress,
    string avatarName,
    int level,
    int actionPoint,
    long dailyRewardReceivedIndex,
    int characterId)
{
    public string AgentAddress { get; set; } = agentAddress;
    public string AvatarAddress { get; set; } = avatarAddress;
    public string AvatarName { get; set; } = avatarName;
    public int Level { get; set; } = level;
    public int ActionPoint { get; private set; } = actionPoint;
    public long DailyRewardReceivedIndex { get; private set; } = dailyRewardReceivedIndex;
    public int CharacterId { get; private set; } = characterId;

    public Avatar(AvatarState avatarState) : this(
        avatarState.agentAddress.ToString(),
        avatarState.address.ToString(),
        avatarState.name,
        avatarState.level,
        avatarState.actionPoint,
        avatarState.dailyRewardReceivedIndex,
        avatarState.characterId)
    {
    }

    public Avatar(Lib9c.Models.States.AvatarState avatarState) : this(
        avatarState.AgentAddress.ToString(),
        avatarState.Address.ToString(),
        avatarState.Name,
        avatarState.Level,
        avatarState.ActionPoint,
        avatarState.DailyRewardReceivedIndex,
        avatarState.CharacterId)
    {
    }
}
