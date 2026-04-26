using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed partial class FamilyCoreModule
{
    private static void DispatchHouseholdBurdenEvents(ModuleEventHandlingScope<FamilyCoreState> scope)
    {
        IDomainEvent[] householdBurdenEvents = scope.Events
            .Where(IsHouseholdBurdenEvent)
            .ToArray();
        if (householdBurdenEvents.Length == 0)
        {
            return;
        }

        IPopulationAndHouseholdsQueries populationQueries = scope.GetRequiredQuery<IPopulationAndHouseholdsQueries>();
        foreach (IDomainEvent domainEvent in householdBurdenEvents)
        {
            ApplyHouseholdBurdenToSponsorClan(scope, populationQueries, domainEvent);
        }
    }

    private static bool IsHouseholdBurdenEvent(IDomainEvent domainEvent)
    {
        return domainEvent.EventType is PopulationEventNames.HouseholdDebtSpiked
            or PopulationEventNames.HouseholdSubsistencePressureChanged
            or PopulationEventNames.HouseholdBurdenIncreased;
    }

    private static void ApplyHouseholdBurdenToSponsorClan(
        ModuleEventHandlingScope<FamilyCoreState> scope,
        IPopulationAndHouseholdsQueries populationQueries,
        IDomainEvent domainEvent)
    {
        if (!TryReadHouseholdId(domainEvent, out HouseholdId householdId))
        {
            return;
        }

        HouseholdPressureSnapshot household;
        try
        {
            household = populationQueries.GetRequiredHousehold(householdId);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        if (!household.SponsorClanId.HasValue)
        {
            return;
        }

        ClanStateData? clan = scope.State.Clans
            .FirstOrDefault(c => c.Id == household.SponsorClanId.Value);
        if (clan is null)
        {
            return;
        }

        HouseholdFamilyBurdenProfile profile = ComputeHouseholdFamilyBurdenProfile(domainEvent, household, clan);
        int oldBranchTension = clan.BranchTension;

        clan.CharityObligation = Math.Clamp(clan.CharityObligation + profile.CharityObligationDelta, 0, 100);
        clan.SupportReserve = Math.Clamp(clan.SupportReserve - profile.SupportReserveDrawdown, 0, 100);
        clan.BranchTension = Math.Clamp(clan.BranchTension + profile.BranchTensionDelta, 0, 100);
        clan.ReliefSanctionPressure = Math.Clamp(
            clan.ReliefSanctionPressure + profile.ReliefSanctionDelta,
            0,
            100);
        clan.LastLifecycleTrace =
            $"{household.HouseholdName}{RenderHouseholdBurdenCause(domainEvent)}压宗房。";
        clan.LastLifecycleOutcome =
            $"义务{clan.CharityObligation}，余力{clan.SupportReserve}，房争{clan.BranchTension}。";

        scope.RecordDiff(
            $"{clan.ClanName}担保户压入宗房：义务{profile.CharityObligationDelta:+#;-#;0}，余力{profile.SupportReserveDrawdown:-#;+0;0}，房争{profile.BranchTensionDelta:+#;-#;0}。",
            clan.Id.Value.ToString());

        if (oldBranchTension < 55 && clan.BranchTension >= 55)
        {
            scope.Emit(
                FamilyCoreEventNames.LineageDisputeHardened,
                $"{clan.ClanName}担保后议转硬。",
                clan.Id.Value.ToString(),
                BuildHouseholdFamilyBurdenMetadata(domainEvent, household, profile));
        }

    }

    private static Dictionary<string, string> BuildHouseholdFamilyBurdenMetadata(
        IDomainEvent domainEvent,
        HouseholdPressureSnapshot household,
        HouseholdFamilyBurdenProfile profile)
    {
        Dictionary<string, string> metadata = new(StringComparer.Ordinal)
        {
            [DomainEventMetadataKeys.Cause] = ResolveFamilyBurdenCause(domainEvent),
            [DomainEventMetadataKeys.SourceEventType] = domainEvent.EventType,
            [DomainEventMetadataKeys.HouseholdId] = household.Id.Value.ToString(),
            [DomainEventMetadataKeys.SettlementId] = household.SettlementId.Value.ToString(),
            [DomainEventMetadataKeys.ClanId] = household.SponsorClanId!.Value.Value.ToString(),
            [DomainEventMetadataKeys.HouseholdDistressSignal] = household.Distress.ToString(),
            [DomainEventMetadataKeys.HouseholdDebtSignal] = household.DebtPressure.ToString(),
            [DomainEventMetadataKeys.FamilyCharityObligationDelta] = profile.CharityObligationDelta.ToString(),
            [DomainEventMetadataKeys.FamilySupportReserveDrawdown] = profile.SupportReserveDrawdown.ToString(),
            [DomainEventMetadataKeys.FamilyBranchTensionDelta] = profile.BranchTensionDelta.ToString(),
            [DomainEventMetadataKeys.FamilyReliefSanctionDelta] = profile.ReliefSanctionDelta.ToString(),
        };

        foreach (string key in new[]
        {
            DomainEventMetadataKeys.DebtAfter,
            DomainEventMetadataKeys.DistressAfter,
            DomainEventMetadataKeys.TaxDebtDelta,
            DomainEventMetadataKeys.SubsistenceDistressDelta,
            DomainEventMetadataKeys.OfficialSupplyDistressDelta,
        })
        {
            if (domainEvent.Metadata.TryGetValue(key, out string? value))
            {
                metadata[key] = value;
            }
        }

        return metadata;
    }

    private static HouseholdFamilyBurdenProfile ComputeHouseholdFamilyBurdenProfile(
        IDomainEvent domainEvent,
        HouseholdPressureSnapshot household,
        ClanStateData clan)
    {
        int eventPressure = domainEvent.EventType switch
        {
            PopulationEventNames.HouseholdBurdenIncreased => 6,
            PopulationEventNames.HouseholdSubsistencePressureChanged => 5,
            PopulationEventNames.HouseholdDebtSpiked => 4,
            _ => 0,
        };
        int householdDistressSignal = ScoreBand(household.Distress, 55, 70, 85);
        int householdDebtSignal = ScoreBand(household.DebtPressure, 55, 70, 85);
        int laborDrag = household.LaborCapacity < 25
            ? 2
            : household.LaborCapacity < 40 ? 1 : 0;
        int dependentDrag = household.DependentCount > household.LaborerCount + 1 ? 1 : 0;
        int migrationDrag = household.IsMigrating || household.MigrationRisk >= 70 ? 1 : 0;
        int metadataPressure = ReadHouseholdBurdenMetadataPressure(domainEvent);
        int reserveBuffer = clan.SupportReserve >= 70
            ? 2
            : clan.SupportReserve >= 45 ? 1 : 0;
        int reserveShortfall = clan.SupportReserve < 20
            ? 2
            : clan.SupportReserve < 35 ? 1 : 0;

        int charityDelta = Math.Clamp(
            eventPressure
            + householdDistressSignal
            + householdDebtSignal
            + laborDrag
            + dependentDrag
            + migrationDrag
            + metadataPressure
            - reserveBuffer,
            2,
            14);
        int supportDrawdown = Math.Clamp(
            Math.Max(0, charityDelta / 3) + (clan.SupportReserve > 0 ? 1 : 0),
            0,
            6);
        int branchDelta = Math.Clamp(
            (charityDelta / 4) + reserveShortfall + migrationDrag,
            1,
            8);
        int reliefDelta = Math.Clamp((charityDelta / 5) + reserveShortfall, 0, 5);

        return new HouseholdFamilyBurdenProfile(charityDelta, supportDrawdown, branchDelta, reliefDelta);
    }

    private static int ReadHouseholdBurdenMetadataPressure(IDomainEvent domainEvent)
    {
        if (domainEvent.EventType == PopulationEventNames.HouseholdDebtSpiked)
        {
            return ScoreMetadata(domainEvent, DomainEventMetadataKeys.TaxDebtDelta, 10, 18, 24);
        }

        if (domainEvent.EventType == PopulationEventNames.HouseholdSubsistencePressureChanged)
        {
            return ScoreMetadata(domainEvent, DomainEventMetadataKeys.SubsistenceDistressDelta, 2, 5, 8);
        }

        if (domainEvent.EventType == PopulationEventNames.HouseholdBurdenIncreased)
        {
            return ScoreMetadata(domainEvent, DomainEventMetadataKeys.OfficialSupplyDistressDelta, 2, 5, 8);
        }

        return 0;
    }

    private static int ScoreMetadata(IDomainEvent domainEvent, string key, int low, int medium, int high)
    {
        return domainEvent.Metadata.TryGetValue(key, out string? value)
            && int.TryParse(value, out int parsed)
            ? ScoreBand(parsed, low, medium, high)
            : 0;
    }

    private static int ScoreBand(int value, int low, int medium, int high)
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

    private static bool TryReadHouseholdId(IDomainEvent domainEvent, out HouseholdId householdId)
    {
        if (int.TryParse(domainEvent.EntityKey, out int entityValue))
        {
            householdId = new HouseholdId(entityValue);
            return true;
        }

        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.HouseholdId, out string? metadataValue)
            && int.TryParse(metadataValue, out int parsedMetadataValue))
        {
            householdId = new HouseholdId(parsedMetadataValue);
            return true;
        }

        householdId = default;
        return false;
    }

    private static string ResolveFamilyBurdenCause(IDomainEvent domainEvent)
    {
        if (domainEvent.Metadata.TryGetValue(DomainEventMetadataKeys.Cause, out string? cause)
            && !string.IsNullOrWhiteSpace(cause))
        {
            return cause;
        }

        return domainEvent.EventType switch
        {
            PopulationEventNames.HouseholdDebtSpiked => DomainEventMetadataValues.CauseTaxSeason,
            PopulationEventNames.HouseholdSubsistencePressureChanged => DomainEventMetadataValues.CauseGrainPriceSpike,
            PopulationEventNames.HouseholdBurdenIncreased => DomainEventMetadataValues.CauseOfficialSupply,
            _ => string.Empty,
        };
    }

    private static string RenderHouseholdBurdenCause(IDomainEvent domainEvent)
    {
        return ResolveFamilyBurdenCause(domainEvent) switch
        {
            DomainEventMetadataValues.CauseTaxSeason => "税役债压",
            DomainEventMetadataValues.CauseGrainPriceSpike => "粮价生计",
            DomainEventMetadataValues.CauseOfficialSupply => "军需催逼",
            _ => "家户承压",
        };
    }

    private readonly record struct HouseholdFamilyBurdenProfile(
        int CharityObligationDelta,
        int SupportReserveDrawdown,
        int BranchTensionDelta,
        int ReliefSanctionDelta);
}
