using Mimir.MongoDB.Bson;

namespace Mimir.GraphQL.Types;

public class ArenaParticipantType : ObjectType<ArenaParticipantDocument>
{
    protected override void Configure(IObjectTypeDescriptor<ArenaParticipantDocument> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field("avatarAddress")
            .Resolve(r => r.Parent<ArenaParticipantDocument>().Object.AvatarAddr);
        descriptor.Field(f => f.Object.Name);
        descriptor.Field(f => f.Object.PortraitId);
        descriptor.Field(f => f.Object.Level);
        descriptor.Field(f => f.Object.Cp);
        descriptor.Field(f => f.Object.Score);
        descriptor.Field(f => f.Object.Ticket);
        descriptor.Field(f => f.Object.TicketResetCount);
        descriptor.Field(f => f.Object.PurchasedTicketCount);
        descriptor.Field(f => f.Object.Win);
        descriptor.Field(f => f.Object.Lose);
        descriptor.Field(f => f.Object.LastBattleBlockIndex);
        descriptor.Field(f => f.SimpleAvatar.AgentAddress);
        descriptor.Field(f => f.SimpleAvatar.Level);
        descriptor.Field(f => f.SimpleAvatar.Exp);
        descriptor.Field(f => f.SimpleAvatar.Hair);
        descriptor.Field(f => f.SimpleAvatar.Lens);
        descriptor.Field(f => f.SimpleAvatar.Ear);
        descriptor.Field(f => f.SimpleAvatar.Tail);
        descriptor.Field(f => f.Rank);
    }
}
