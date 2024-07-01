using Nekoyume.Model.State;
using NCAvatarState = Nekoyume.Model.State.AvatarState;

namespace Mimir.Models;

public class Avatar(
    string agentAddress,
    string avatarAddress,
    string avatarName,
    int level,
    int actionPoint,
    long dailyRewardReceivedIndex)
{
    public string AgentAddress { get; set; } = agentAddress;
    public string AvatarAddress { get; set; } = avatarAddress;
    public string AvatarName { get; set; } = avatarName;
    public int Level { get; set; } = level;
    public int ActionPoint { get; private set; } = actionPoint;
    public long DailyRewardReceivedIndex { get; private set; } = dailyRewardReceivedIndex;

    public Avatar(NCAvatarState avatarState) : this(
        avatarState.agentAddress.ToString(),
        avatarState.address.ToString(),
        avatarState.name,
        avatarState.level,
        avatarState.actionPoint,
        avatarState.dailyRewardReceivedIndex)
    {
    }
}
