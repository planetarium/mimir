using HotChocolate.Types;
using Nekoyume.Model.Elemental;
using Nekoyume.Model.Skill;
using Nekoyume.TableData;

namespace Lib9c.GraphQL.Types.Sheets;

public class SkillSheetRowType : ObjectType<SkillSheet.Row>
{
    protected override void Configure(IObjectTypeDescriptor<SkillSheet.Row> descriptor)
    {
        descriptor.BindFieldsExplicitly();
        descriptor
            .Field(f => f.Id)
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.ElementalType)
            .Type<NonNullType<EnumType<ElementalType>>>();
        descriptor
            .Field(f => f.SkillType)
            .Type<NonNullType<EnumType<SkillType>>>();
        descriptor
            .Field(f => f.SkillCategory)
            .Type<NonNullType<EnumType<SkillCategory>>>();
        descriptor
            .Field(f => f.SkillTargetType)
            .Type<NonNullType<EnumType<SkillTargetType>>>();
        descriptor
            .Field(f => f.HitCount)
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.Cooldown)
            .Type<NonNullType<IntType>>();
        descriptor
            .Field(f => f.Combo)
            .Type<NonNullType<BooleanType>>();
    }
}
