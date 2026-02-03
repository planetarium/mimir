using Mimir.Shared.Constants;
using Mimir.Shared.Client;
using Mimir.Shared.Services;
using Lib9c.Models.States;
using Mimir.MongoDB.Bson;

namespace Mimir.Worker.StateDocumentConverter;

public class InfiniteTowerInfoStateDocumentConverter : IStateDocumentConverter
{
    private readonly int _infiniteTowerId;

    public InfiniteTowerInfoStateDocumentConverter(int infiniteTowerId)
    {
        _infiniteTowerId = infiniteTowerId;
    }

    public MimirBsonDocument ConvertToDocument(AddressStatePair context)
    {
        var state = new InfiniteTowerInfo(context.RawState);
        // NOTE: RawState 내부의 InfiniteTowerId 값이 타워별로 안정적이지 않을 수 있으므로,
        // 핸들러가 구독 중인 towerId를 우선 신뢰해 문서 키(_id)를 분리한다.
        var normalizedState = state with { InfiniteTowerId = _infiniteTowerId };
        return new InfiniteTowerInfoDocument(
            context.BlockIndex,
            context.Address,
            normalizedState.Address,
            _infiniteTowerId,
            normalizedState
        );
    }
}
