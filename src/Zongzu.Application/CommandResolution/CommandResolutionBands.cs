using System;
using System.Linq;

namespace Zongzu.Application;

internal static class CommandResolutionBands
{
    public static int Score(int value, int low, int medium, int high)
    {
        if (value >= high)
        {
            return 3;
        }

        if (value >= medium)
        {
            return 2;
        }

        return value >= low ? 1 : 0;
    }
}

internal readonly record struct CommandResolutionFactor(string Label, int Band)
{
    public string Render()
    {
        return $"{Label}{Band}阶";
    }
}

internal static class CommandResolutionProfileText
{
    public static string RenderFactors(params CommandResolutionFactor[] factors)
    {
        return string.Join("、", factors.Select(static factor => factor.Render()));
    }
}

internal static class CommandResolutionMath
{
    public static int Clamp100(int value)
    {
        return Math.Clamp(value, 0, 100);
    }

    public static int AdjustReduction(int baseValue, int shift, int minimum = 1)
    {
        return Math.Max(minimum, baseValue + shift);
    }

    public static int AdjustIncrease(int baseValue, int shift)
    {
        return Math.Max(0, baseValue + shift);
    }
}
