namespace Zongzu.Kernel;

public readonly record struct PersonId(int Value);

public readonly record struct HouseholdId(int Value);

public readonly record struct ClanId(int Value);

public readonly record struct SettlementId(int Value);

/// <summary>
/// SPATIAL_SKELETON_SPEC §2 — a social-function route between settlements.
/// Same physical corridor can host multiple RouteId entries (grain + market
/// + exam-travel sharing one road), each its own RouteState.
/// </summary>
public readonly record struct RouteId(int Value);

public readonly record struct InstitutionId(int Value);

public readonly record struct MemoryId(int Value);

public readonly record struct RelationshipEdgeId(int Value);

public readonly record struct ForceGroupId(int Value);

public readonly record struct CampaignId(int Value);

public readonly record struct NotificationId(int Value);

public static class KernelIdAllocator
{
    public static PersonId NextPerson(KernelState kernelState)
    {
        return new PersonId(kernelState.NextPersonId++);
    }

    public static HouseholdId NextHousehold(KernelState kernelState)
    {
        return new HouseholdId(kernelState.NextHouseholdId++);
    }

    public static ClanId NextClan(KernelState kernelState)
    {
        return new ClanId(kernelState.NextClanId++);
    }

    public static SettlementId NextSettlement(KernelState kernelState)
    {
        return new SettlementId(kernelState.NextSettlementId++);
    }

    public static RouteId NextRoute(KernelState kernelState)
    {
        return new RouteId(kernelState.NextRouteId++);
    }

    public static InstitutionId NextInstitution(KernelState kernelState)
    {
        return new InstitutionId(kernelState.NextInstitutionId++);
    }

    public static MemoryId NextMemory(KernelState kernelState)
    {
        return new MemoryId(kernelState.NextMemoryId++);
    }

    public static RelationshipEdgeId NextRelationshipEdge(KernelState kernelState)
    {
        return new RelationshipEdgeId(kernelState.NextRelationshipEdgeId++);
    }

    public static NotificationId NextNotification(KernelState kernelState)
    {
        return new NotificationId(kernelState.NextNotificationId++);
    }
}
