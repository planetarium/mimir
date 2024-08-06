using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public interface IStateHandler
{
    MimirBsonDocument ConvertToState(StateDiffContext context);
}
