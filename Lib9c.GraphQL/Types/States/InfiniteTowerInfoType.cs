using HotChocolate.Types;
using Lib9c.Models.States;

namespace Lib9c.GraphQL.Types.States;

public class InfiniteTowerInfoType : ObjectType<InfiniteTowerInfo>
{
    protected override void Configure(IObjectTypeDescriptor<InfiniteTowerInfo> descriptor)
    {
        // 기본 필드들은 자동으로 매핑됩니다
    }
}
