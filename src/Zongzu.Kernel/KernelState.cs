using System;

namespace Zongzu.Kernel;

public sealed class KernelState
{
    public long InitialSeed { get; set; }

    public ulong RandomState { get; set; }

    public int NextPersonId { get; set; }

    public int NextHouseholdId { get; set; }

    public int NextClanId { get; set; }

    public int NextSettlementId { get; set; }

    public int NextInstitutionId { get; set; }

    public int NextMemoryId { get; set; }

    public int NextRelationshipEdgeId { get; set; }

    public int NextNotificationId { get; set; }

    public string LastReplayHash { get; set; } = string.Empty;

    public static KernelState Create(long seed)
    {
        ulong state = unchecked((ulong)seed);
        if (state == 0UL)
        {
            state = 0x9E3779B97F4A7C15UL;
        }

        return new KernelState
        {
            InitialSeed = seed,
            RandomState = state,
            NextPersonId = 1,
            NextHouseholdId = 1,
            NextClanId = 1,
            NextSettlementId = 1,
            NextInstitutionId = 1,
            NextMemoryId = 1,
            NextRelationshipEdgeId = 1,
            NextNotificationId = 1,
        };
    }

    public KernelState Clone()
    {
        return new KernelState
        {
            InitialSeed = InitialSeed,
            RandomState = RandomState,
            NextPersonId = NextPersonId,
            NextHouseholdId = NextHouseholdId,
            NextClanId = NextClanId,
            NextSettlementId = NextSettlementId,
            NextInstitutionId = NextInstitutionId,
            NextMemoryId = NextMemoryId,
            NextRelationshipEdgeId = NextRelationshipEdgeId,
            NextNotificationId = NextNotificationId,
            LastReplayHash = LastReplayHash,
        };
    }

    public KernelState CloneWithoutReplayHash()
    {
        KernelState clone = Clone();
        clone.LastReplayHash = string.Empty;
        return clone;
    }
}
