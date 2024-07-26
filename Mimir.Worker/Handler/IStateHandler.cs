using Mimir.MongoDB.Bson;

namespace Mimir.Worker.Handler;

public interface IStateHandler
{
    IMimirBsonDocument ConvertToState(StateDiffContext context);
}
