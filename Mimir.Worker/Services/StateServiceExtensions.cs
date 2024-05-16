using Mimir.Worker.Scrapper;

namespace Mimir.Worker.Services;

public static class StateServiceExtensions
{
    public static StateGetter At(this IStateService service, long index)
    {
        return new StateGetter(service, index);
    }
}
