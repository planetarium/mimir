using Microsoft.Extensions.Options;
using Mimir.Worker.Util;

namespace Mimir.Worker.Services;

public static class StateServiceExtensions
{
    public static StateGetter At(this IStateService service, IOptions<Configuration> configuration)
    {
        return new StateGetter(service, configuration);
    }
}
