using System.Linq;
using Zongzu.Kernel;

namespace Zongzu.Kernel.Tests;

[TestFixture]
public sealed class DeterministicRandomTests
{
    [Test]
    public void SameSeedProducesSameSequence()
    {
        KernelState firstState = KernelState.Create(12345);
        KernelState secondState = KernelState.Create(12345);
        DeterministicRandom firstRandom = new(firstState);
        DeterministicRandom secondRandom = new(secondState);

        int[] firstValues = Enumerable.Range(0, 16)
            .Select(_ => firstRandom.NextInt(0, 10_000))
            .ToArray();
        int[] secondValues = Enumerable.Range(0, 16)
            .Select(_ => secondRandom.NextInt(0, 10_000))
            .ToArray();

        Assert.That(firstValues, Is.EqualTo(secondValues));
    }

    [Test]
    public void GameDateNextMonthRollsYearBoundary()
    {
        GameDate date = new(1200, 12);

        GameDate nextDate = date.NextMonth();

        Assert.That(nextDate.Year, Is.EqualTo(1201));
        Assert.That(nextDate.Month, Is.EqualTo(1));
    }
}
