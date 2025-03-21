using HotChocolate.Types;
using Lib9c.Models.States;
using Lib9c.Models.WorldInformation;

namespace Mimir.GraphQL.Types;

public class WorldInformationStateType : ObjectType<WorldInformationState>
{
    protected override void Configure(IObjectTypeDescriptor<WorldInformationState> descriptor)
    {
        descriptor.Name("WorldInformationState");
        
        descriptor.Field(f => f.WorldDictionary)
            .Type<ListType<WorldType>>();
    }
}

public class WorldType : ObjectType<World>
{
    protected override void Configure(IObjectTypeDescriptor<World> descriptor)
    {
        descriptor.Name("World");

        descriptor.Field(f => f.Id).Type<IntType>();
        descriptor.Field(f => f.Name).Type<StringType>();
        descriptor.Field(f => f.StageBegin).Type<IntType>();
        descriptor.Field(f => f.StageEnd).Type<IntType>();
        descriptor.Field(f => f.StageClearedId).Type<IntType>();
        descriptor.Field(f => f.UnlockedBlockIndex).Type<LongType>();
        descriptor.Field(f => f.StageClearedBlockIndex).Type<LongType>();
        descriptor.Field(f => f.IsUnlocked).Type<BooleanType>();
        descriptor.Field(f => f.IsStageCleared).Type<BooleanType>();
    }
} 