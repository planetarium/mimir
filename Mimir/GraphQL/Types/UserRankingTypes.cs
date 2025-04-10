using HotChocolate.Types;
using Mimir.GraphQL.Models;
using Mimir.GraphQL.Types.MimirBsonDocuments;
using Mimir.MongoDB.Bson;

namespace Mimir.GraphQL.Types;

public class UserAdventureRankingType : ObjectType<UserAdventureRanking>
{
    protected override void Configure(IObjectTypeDescriptor<UserAdventureRanking> descriptor)
    {
        descriptor.Name("UserAdventureRanking");
        descriptor.Description("User ranking information for adventure CP");
        
        descriptor.Field(f => f.UserDocument)
            .Type<AdventureCpDocumentType>()
            .Description("User document with CP information");
            
        descriptor.Field(f => f.Rank)
            .Type<IntType>()
            .Description("User's rank (1-based)");
    }
}

public class UserArenaRankingType : ObjectType<UserArenaRanking>
{
    protected override void Configure(IObjectTypeDescriptor<UserArenaRanking> descriptor)
    {
        descriptor.Name("UserArenaRanking");
        descriptor.Description("User ranking information for arena CP");
        
        descriptor.Field(f => f.UserDocument)
            .Type<ArenaCpDocumentType>()
            .Description("User document with CP information");
            
        descriptor.Field(f => f.Rank)
            .Type<IntType>()
            .Description("User's rank (1-based)");
    }
}

public class UserRaidRankingType : ObjectType<UserRaidRanking>
{
    protected override void Configure(IObjectTypeDescriptor<UserRaidRanking> descriptor)
    {
        descriptor.Name("UserRaidRanking");
        descriptor.Description("User ranking information for raid CP");
        
        descriptor.Field(f => f.UserDocument)
            .Type<RaidCpDocumentType>()
            .Description("User document with CP information");
            
        descriptor.Field(f => f.Rank)
            .Type<IntType>()
            .Description("User's rank (1-based)");
    }
}

public class UserWorldInformationRankingType : ObjectType<UserWorldInformationRanking>
{
    protected override void Configure(IObjectTypeDescriptor<UserWorldInformationRanking> descriptor)
    {
        descriptor.Name("UserWorldInformationRanking");
        descriptor.Description("User ranking information for world information");
        
        descriptor.Field(f => f.UserDocument)
            .Type<WorldInformationDocumentType>()
            .Description("User world information document");
            
        descriptor.Field(f => f.Rank)
            .Type<IntType>()
            .Description("User's rank (1-based)");
    }
} 