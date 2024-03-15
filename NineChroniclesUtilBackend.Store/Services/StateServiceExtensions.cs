using NineChroniclesUtilBackend.Store.Scrapper;

namespace NineChroniclesUtilBackend.Store.Services;

public static class StateServiceExtensions
{
    public static StateGetter At(this IStateService service, long blockIndex)
    {
        return new StateGetter(service, blockIndex);
    }
}
