using Mimir.MongoDB.Bson;

namespace Mimir.GraphQL.Models;

public class UserAdventureRanking
{
    public AdventureCpDocument? UserDocument { get; set; }
    public int Rank { get; set; }
}

public class UserArenaRanking
{
    public ArenaCpDocument? UserDocument { get; set; }
    public int Rank { get; set; }
}

public class UserRaidRanking
{
    public RaidCpDocument? UserDocument { get; set; }
    public int Rank { get; set; }
}

public class UserWorldInformationRanking
{
    public WorldInformationDocument? UserDocument { get; set; }
    public int Rank { get; set; }
} 