using Mimir.MongoDB.Enums;

namespace Mimir.MongoDB.Models;

public class InfiniteTowerInfoFilter
{
    public int InfiniteTowerId { get; set; }
    public InfiniteTowerInfoSortBy? SortBy { get; set; }
    public SortDirection? SortDirection { get; set; } = Enums.SortDirection.Descending;
}
