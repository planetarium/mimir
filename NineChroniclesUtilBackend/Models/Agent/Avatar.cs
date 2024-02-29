using MongoDB.Bson;

namespace NineChroniclesUtilBackend.Models.Agent;

public class Avatar(string avatarAddress, string avatarName, int level)
{
    public string AvatarAddress { get; set; } = avatarAddress;
    public string AvatarName { get; set; } = avatarName;
    public int Level { get; set; } = level;
}
