using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.ConflictAndForce;

/// <summary>
/// Phase 9 武力骨骼 — <c>LIVING_WORLD_DESIGN §2.9</c>。
/// 将 <see cref="SettlementForceState"/> 的 Guard/Retainer/Militia/Escort 人头数投影为带族类的 <see cref="ForceGroupState"/>；
/// 将 <c>HasActiveConflict + LastConflictTrace</c> 投影为结构化 <see cref="ConflictIncidentState"/>。
/// 薄链：仅重建可投影群体，不做独立事件池。
/// </summary>
public static class ConflictAndForceStateProjection
{
    public static ConflictAndForceState UpgradeFromSchemaV3ToV4(ConflictAndForceState state)
    {
        state.ForceGroups ??= new List<ForceGroupState>();
        state.Incidents ??= new List<ConflictIncidentState>();
        BuildForceGroupsAndIncidents(state);
        return state;
    }

    public static void BuildForceGroupsAndIncidents(ConflictAndForceState state)
    {
        state.Settlements ??= new List<SettlementForceState>();
        state.ForceGroups ??= new List<ForceGroupState>();
        state.Incidents ??= new List<ConflictIncidentState>();

        Dictionary<string, ForceGroupState> priorGroups = state.ForceGroups
            .Where(static group => !string.IsNullOrWhiteSpace(group.ForceId))
            .GroupBy(static group => group.ForceId, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);

        Dictionary<string, ConflictIncidentState> priorIncidents = state.Incidents
            .Where(static incident => !string.IsNullOrWhiteSpace(incident.IncidentId))
            .GroupBy(static incident => incident.IncidentId, StringComparer.Ordinal)
            .ToDictionary(static incident => incident.Key, static incident => incident.First(), StringComparer.Ordinal);

        List<ForceGroupState> nextGroups = new();
        List<ConflictIncidentState> nextIncidents = new();

        foreach (SettlementForceState settlement in state.Settlements
            .OrderBy(static settlement => settlement.SettlementId.Value))
        {
            TryBuildGroup(settlement, ForceFamily.HouseholdRetainer, settlement.RetainerCount, priorGroups, nextGroups);
            TryBuildGroup(settlement, ForceFamily.EscortBand, settlement.EscortCount, priorGroups, nextGroups);
            TryBuildGroup(settlement, ForceFamily.Militia, settlement.MilitiaCount, priorGroups, nextGroups);
            TryBuildGroup(settlement, ForceFamily.YamenForce, settlement.GuardCount, priorGroups, nextGroups);

            if (settlement.HasActiveConflict)
            {
                string incidentId = BuildIncidentId(settlement.SettlementId);
                priorIncidents.TryGetValue(incidentId, out ConflictIncidentState? prior);

                IncidentScale scale = ClassifyScale(settlement);
                IncidentOutcome outcome = ClassifyOutcome(settlement, prior);

                nextIncidents.Add(new ConflictIncidentState
                {
                    IncidentId = incidentId,
                    Scale = scale,
                    Location = settlement.SettlementId,
                    RouteId = prior?.RouteId ?? string.Empty,
                    Attackers = prior?.Attackers is { Count: > 0 } attackers
                        ? new List<string>(attackers)
                        : new List<string>(),
                    Defenders = prior?.Defenders is { Count: > 0 } defenders
                        ? new List<string>(defenders)
                        : new List<string>(),
                    Outcome = outcome,
                    CauseKey = string.IsNullOrWhiteSpace(prior?.CauseKey)
                        ? InferCauseKey(settlement)
                        : prior!.CauseKey,
                    OccurredYear = prior?.OccurredYear ?? 0,
                    OccurredMonth = prior?.OccurredMonth ?? 0,
                });
            }
        }

        state.ForceGroups = nextGroups
            .OrderBy(static group => group.Location.Value)
            .ThenBy(static group => (int)group.Family)
            .ThenBy(static group => group.ForceId, StringComparer.Ordinal)
            .ToList();

        state.Incidents = nextIncidents
            .OrderBy(static incident => incident.Location.Value)
            .ThenBy(static incident => incident.IncidentId, StringComparer.Ordinal)
            .ToList();
    }

    private static void TryBuildGroup(
        SettlementForceState settlement,
        ForceFamily family,
        int headcount,
        IReadOnlyDictionary<string, ForceGroupState> priorGroups,
        List<ForceGroupState> nextGroups)
    {
        if (headcount <= 0)
        {
            return;
        }

        string forceId = BuildForceId(settlement.SettlementId, family);
        priorGroups.TryGetValue(forceId, out ForceGroupState? prior);

        int strength = Math.Clamp(headcount * StrengthPerHead(family), 0, 100);
        int readiness = Math.Clamp(settlement.Readiness, 0, 100);
        int morale = Math.Clamp(
            60 - (settlement.CampaignFatigue / 2) + (settlement.OrderSupportLevel / 4),
            0,
            100);
        int discipline = Math.Clamp(
            DefaultDiscipline(family) - (settlement.CampaignEscortStrain / 4),
            0,
            100);
        int fatigue = Math.Clamp(settlement.CampaignFatigue, 0, 100);

        if (prior is not null)
        {
            strength = BlendTowards(prior.Strength, strength);
            morale = BlendTowards(prior.Morale, morale);
            discipline = BlendTowards(prior.Discipline, discipline);
        }

        nextGroups.Add(new ForceGroupState
        {
            ForceId = forceId,
            Family = family,
            OwnerKey = prior?.OwnerKey ?? BuildDefaultOwnerKey(family, settlement.SettlementId),
            Location = settlement.SettlementId,
            Strength = strength,
            Readiness = readiness,
            Morale = morale,
            Discipline = discipline,
            Fatigue = fatigue,
        });
    }

    private static IncidentScale ClassifyScale(SettlementForceState settlement)
    {
        if (settlement.CampaignFatigue >= 55 || settlement.CampaignEscortStrain >= 55)
        {
            return IncidentScale.CampaignBoard;
        }

        if (settlement.ResponseActivationLevel >= 55)
        {
            return IncidentScale.TacticalLite;
        }

        if (settlement.OrderSupportLevel <= 30)
        {
            return IncidentScale.LocalVignette;
        }

        return IncidentScale.SocialPressure;
    }

    private static IncidentOutcome ClassifyOutcome(SettlementForceState settlement, ConflictIncidentState? prior)
    {
        if (!settlement.HasActiveConflict)
        {
            return IncidentOutcome.Suppressed;
        }

        if (settlement.CampaignFatigue >= 70)
        {
            return IncidentOutcome.Escalated;
        }

        return prior?.Outcome is IncidentOutcome.Pending or IncidentOutcome.Unknown or null
            ? IncidentOutcome.Pending
            : prior!.Outcome;
    }

    private static string InferCauseKey(SettlementForceState settlement)
    {
        if (settlement.CampaignEscortStrain >= 45)
        {
            return "campaign-strain";
        }

        if (settlement.OrderSupportLevel <= 25)
        {
            return "order-collapse";
        }

        return "local-friction";
    }

    private static int StrengthPerHead(ForceFamily family) => family switch
    {
        ForceFamily.HouseholdRetainer => 6,
        ForceFamily.EscortBand => 7,
        ForceFamily.Militia => 4,
        ForceFamily.YamenForce => 8,
        ForceFamily.OfficialDetachment => 10,
        ForceFamily.GarrisonForce => 9,
        ForceFamily.RebelBand => 5,
        _ => 4,
    };

    private static int DefaultDiscipline(ForceFamily family) => family switch
    {
        ForceFamily.HouseholdRetainer => 55,
        ForceFamily.EscortBand => 60,
        ForceFamily.Militia => 40,
        ForceFamily.YamenForce => 65,
        ForceFamily.OfficialDetachment => 75,
        ForceFamily.GarrisonForce => 70,
        ForceFamily.RebelBand => 35,
        _ => 50,
    };

    private static int BlendTowards(int prior, int target)
    {
        return Math.Clamp((prior + target + target) / 3, 0, 100);
    }

    private static string BuildForceId(SettlementId settlementId, ForceFamily family)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"force-{family}-{settlementId.Value}");
    }

    private static string BuildIncidentId(SettlementId settlementId)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"incident-{settlementId.Value}");
    }

    private static string BuildDefaultOwnerKey(ForceFamily family, SettlementId settlementId)
    {
        return family switch
        {
            ForceFamily.YamenForce => "yamen",
            ForceFamily.OfficialDetachment => "official",
            ForceFamily.GarrisonForce => "garrison",
            ForceFamily.RebelBand => "rebel",
            _ => string.Create(CultureInfo.InvariantCulture, $"settlement-{settlementId.Value}"),
        };
    }
}
