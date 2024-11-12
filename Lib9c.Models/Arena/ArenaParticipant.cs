using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Lib9c.Models.Exceptions;
using Libplanet.Crypto;
using MongoDB.Bson.Serialization.Attributes;
using Nekoyume.Model.State;
using ValueKind = Bencodex.Types.ValueKind;

namespace Lib9c.Models.Arena;

[BsonIgnoreExtraElements]
public record ArenaParticipant : IBencodable
{
    public const int StateVersion = 1;

    public Address AvatarAddr { get; init; }

    /// <summary>
    /// If you need to know <see cref="Nekoyume.Model.State.AvatarState.NameWithHash"/>, check
    /// <see cref="Nekoyume.Model.State.AvatarState.PostConstructor"/> method of the
    /// <see cref="Nekoyume.Model.State.AvatarState"/> class and you can find the relevant information there. It
    /// provides a formatted string that includes the avatar's <see cref="Nekoyume.Model.State.AvatarState.name"/>
    /// and a shortened version of their address.
    /// </summary>
    /// <example>
    /// <code>
    /// $"{name} &lt;size=80%&gt;&lt;color=#A68F7E&gt;#{address.ToHex().Substring(0, 4)}&lt;/color&gt;&lt;/size&gt;";
    /// </code>
    /// </example>
    public string Name { get; init; }

    public int PortraitId { get; init; }
    public int Level { get; init; }
    public int Cp { get; init; }

    public int Score { get; init; }

    public int Ticket { get; init; }
    public int TicketResetCount { get; init; }
    public int PurchasedTicketCount { get; init; }

    public int Win { get; init; }
    public int Lose { get; init; }

    public long LastBattleBlockIndex { get; init; }

    [BsonIgnore, GraphQLIgnore, JsonIgnore]
    public IValue Bencoded => List.Empty
        .Add(StateVersion)
        .Add(AvatarAddr.Serialize())
        .Add(Name)
        .Add(PortraitId)
        .Add(Level)
        .Add(Cp)
        .Add(Score)
        .Add(Ticket)
        .Add(TicketResetCount)
        .Add(PurchasedTicketCount)
        .Add(Win)
        .Add(Lose)
        .Add(LastBattleBlockIndex);

    public ArenaParticipant(IValue bencoded)
    {
        if (bencoded is not List l)
        {
            throw new UnsupportedArgumentTypeException<ValueKind>(
                nameof(bencoded),
                new[] { ValueKind.List },
                bencoded.Kind);
        }

        var stateVersion = (Integer)l[0];
        if (stateVersion != StateVersion)
        {
            throw new UnsupportedModelVersionException(StateVersion, stateVersion);
        }

        AvatarAddr = l[1].ToAddress();
        Name = (Text)l[2];
        PortraitId = (Integer)l[3];
        Level = (Integer)l[4];
        Cp = (Integer)l[5];
        Score = (Integer)l[6];
        Ticket = (Integer)l[7];
        TicketResetCount = (Integer)l[8];
        PurchasedTicketCount = (Integer)l[9];
        Win = (Integer)l[10];
        Lose = (Integer)l[11];
        LastBattleBlockIndex = (Integer)l[12];
    }
}
