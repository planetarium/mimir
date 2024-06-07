using Libplanet.Crypto;
using MongoDB.Bson;
using Nekoyume.Model.State;

namespace Mimir.Models;

public class Agent
{
    public int Version { get; set; }
    public Address Address { get; set; }
    public Dictionary<int, Address> AvatarAddresses { get; set; }
    public int MonsterCollectionRound { get; set; }

    public Agent(AgentState agentState)
    {
        Version = agentState.Version;
        Address = agentState.address;
        AvatarAddresses = agentState.avatarAddresses;
        MonsterCollectionRound = agentState.MonsterCollectionRound;
    }

    public Agent(BsonDocument document)
    {
        Version = document["Version"].AsInt32;
        Address = new Address(document["address"].AsString);
        AvatarAddresses = document["avatarAddresses"]
            .AsBsonDocument
            .ToDictionary(
                kvp => int.Parse(kvp.Name),
                kvp => new Address(kvp.Value.AsString));
        MonsterCollectionRound = document["MonsterCollectionRound"].AsInt32;
    }
}
