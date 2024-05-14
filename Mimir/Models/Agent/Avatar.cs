namespace Mimir.Models.Agent;

public class Avatar(string avatarAddress, string avatarName, int level, int actionPoint)
{
    public string AvatarAddress { get; set; } = avatarAddress;
    public string AvatarName { get; set; } = avatarName;
    public int Level { get; set; } = level;
    public int ActionPoint { get; private set; } = actionPoint;
}
