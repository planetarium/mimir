using NineChroniclesUtilBackend.Store.Scrapper;

namespace NineChroniclesUtilBackend.Store.Services;

public static class StateServiceExtensions
{
    public static StateGetter At(this IStateService service)
    {
        return new StateGetter(service);
    }
}
