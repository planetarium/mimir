using Bencodex;

namespace Mimir.Worker.Handler;

public interface IStateHandler
{
    IBencodable ConvertToState(StateDiffContext context);
}
