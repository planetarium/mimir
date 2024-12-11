using Lib9c.Models.Market;

namespace Mimir.Worker.Services;

public interface IItemProductCalculationService
{
    Task<int?> CalculateCombatPointAsync(ItemProduct itemProduct);

    Task<(int? crystal, int? crystalPerPrice)> CalculateCrystalMetricsAsync(ItemProduct itemProduct);
}