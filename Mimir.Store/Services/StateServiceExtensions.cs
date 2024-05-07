using Mimir.Store.Scrapper;

namespace Mimir.Store.Services;

public static class StateServiceExtensions
{
    public static StateGetter At(this IStateService service)
    {
        return new StateGetter(service);
    }
}
