using System.Collections.Generic;

namespace Zongzu.Contracts;

public enum SimulationCadenceBand
{
    Xun = 0,
    Month = 1,
    Seasonal = 2,
}

public enum SimulationXun
{
    None = 0,
    Shangxun = 1,
    Zhongxun = 2,
    Xiaxun = 3,
}

public static class SimulationCadencePresets
{
    public static IReadOnlyCollection<SimulationCadenceBand> MonthOnly { get; } =
    [
        SimulationCadenceBand.Month,
    ];

    public static IReadOnlyCollection<SimulationCadenceBand> XunAndMonth { get; } =
    [
        SimulationCadenceBand.Xun,
        SimulationCadenceBand.Month,
    ];

    public static IReadOnlyCollection<SimulationCadenceBand> MonthAndSeasonal { get; } =
    [
        SimulationCadenceBand.Month,
        SimulationCadenceBand.Seasonal,
    ];

    public static IReadOnlyCollection<SimulationCadenceBand> XunMonthAndSeasonal { get; } =
    [
        SimulationCadenceBand.Xun,
        SimulationCadenceBand.Month,
        SimulationCadenceBand.Seasonal,
    ];
}
