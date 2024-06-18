using HotChocolate.Types;
using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class SkillType : ObjectType<Skill>
{
    protected override void Configure(IObjectTypeDescriptor<Skill> descriptor)
    {
        descriptor.Field(s => s.SkillRow).Type<SkillRowType>();
        descriptor.Field(s => s.Power).Type<IntType>();
        descriptor.Field(s => s.Chance).Type<IntType>();
        descriptor.Field(s => s.StatPowerRatio).Type<IntType>();
        descriptor.Field(s => s.ReferencedStatType).Type<IntType>();
    }
}
