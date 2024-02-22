namespace NineChroniclesUtilBackend.Models.Arena;

public class ArenaRanking(string agentAddress, string avatarAddress)
{
    public string AgentAddress { get; private set; } = agentAddress;
    public string AvatarAddress { get; private set; } = avatarAddress;
}