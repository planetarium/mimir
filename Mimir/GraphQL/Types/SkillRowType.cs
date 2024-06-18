using HotChocolate.Types;
using Mimir.Models.Product;

namespace Mimir.GraphQL.Types;

public class SkillRowType : ObjectType<SkillRow>
{
    protected override void Configure(IObjectTypeDescriptor<SkillRow> descriptor)
    {
        descriptor.Field(s => s.Key).Type<IntType>();
        descriptor.Field(s => s.Id).Type<IntType>();
        descriptor.Field(s => s.ElementalType).Type<IntType>();
        descriptor.Field(s => s.SkillType).Type<IntType>();
        descriptor.Field(s => s.SkillCategory).Type<IntType>();
        descriptor.Field(s => s.SkillTargetType).Type<IntType>();
        descriptor.Field(s => s.HitCount).Type<IntType>();
        descriptor.Field(s => s.Cooldown).Type<IntType>();
        descriptor.Field(s => s.Combo).Type<BooleanType>();
    }
}
