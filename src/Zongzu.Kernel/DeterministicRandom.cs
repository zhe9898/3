using System;

namespace Zongzu.Kernel;

public interface IDeterministicRandom
{
    int NextInt(int minInclusive, int maxExclusive);
}

public sealed class DeterministicRandom : IDeterministicRandom
{
    private readonly KernelState _kernelState;

    public DeterministicRandom(KernelState kernelState)
    {
        _kernelState = kernelState ?? throw new ArgumentNullException(nameof(kernelState));
    }

    public int NextInt(int minInclusive, int maxExclusive)
    {
        if (minInclusive >= maxExclusive)
        {
            throw new ArgumentOutOfRangeException(nameof(maxExclusive), "maxExclusive must be greater than minInclusive.");
        }

        ulong sample = NextUInt64();
        uint range = (uint)(maxExclusive - minInclusive);
        uint value = (uint)(sample % range);
        return minInclusive + (int)value;
    }

    private ulong NextUInt64()
    {
        ulong state = _kernelState.RandomState + 0x9E3779B97F4A7C15UL;
        _kernelState.RandomState = state;

        ulong z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
}
