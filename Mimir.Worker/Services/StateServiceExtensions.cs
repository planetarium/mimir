using Mimir.Worker.Util;

namespace Mimir.Worker.Services;

public static class StateServiceExtensions
{
    public static StateGetter At(this IStateService service)
    {
        return new StateGetter(service);
    }
}
