namespace Mimir.Models.Agent;

public class Avatar(string avatarAddress, string avatarName, int level, int actionPoint, long dailyRewardReceivedIndex)
{
    public string AvatarAddress { get; set; } = avatarAddress;
    public string AvatarName { get; set; } = avatarName;
    public int Level { get; set; } = level;
    public int ActionPoint { get; private set; } = actionPoint;
    public long DailyRewardReceivedIndex { get; private set; } = dailyRewardReceivedIndex;
}
