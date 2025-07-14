using Mimir.GraphQL.Types;

namespace Mimir.Services;

public interface IWncgPriceService
{
    Task<WncgPriceType?> GetWncgPriceAsync();
} 