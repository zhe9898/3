using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

/// <summary>
/// Phase 8 匪患骨骼 — <c>LIVING_WORLD_DESIGN §2.8</c>。
/// 将 <see cref="SettlementDisorderState"/> 的匪患压力投影为具名匪群 <see cref="OutlawBandState"/>。
/// 薄链：策源地、强度、聚合度随月脉搏随动，不做独立事件池。
/// </summary>
public static class OrderAndBanditryStateProjection
{
    private const int SeedThreshold = 50;

    public static OrderAndBanditryState UpgradeFromSchemaV6ToV7(OrderAndBanditryState state)
    {
        state.OutlawBands ??= new List<OutlawBandState>();
        BuildOrEvolveOutlawBands(state);
        return state;
    }

    public static void BuildOrEvolveOutlawBands(OrderAndBanditryState state)
    {
        state.Settlements ??= new List<SettlementDisorderState>();
        state.OutlawBands ??= new List<OutlawBandState>();

        Dictionary<string, OutlawBandState> existingBands = state.OutlawBands
            .Where(static band => !string.IsNullOrWhiteSpace(band.BandId))
            .GroupBy(static band => band.BandId, StringComparer.Ordinal)
            .ToDictionary(static group => group.Key, static group => group.First(), StringComparer.Ordinal);

        List<OutlawBandState> next = new();

        foreach (SettlementDisorderState settlement in state.Settlements
            .OrderBy(static settlement => settlement.SettlementId.Value))
        {
            if (settlement.BanditThreat < SeedThreshold)
            {
                continue;
            }

            string bandId = BuildBandId(settlement.SettlementId);
            existingBands.TryGetValue(bandId, out OutlawBandState? prior);

            int strength = Math.Clamp(
                (settlement.BanditThreat * 3 / 4) + (settlement.BlackRoutePressure / 5),
                0,
                100);
            int legitimacy = Math.Clamp(
                (settlement.CoercionRisk / 3) + (settlement.DisorderPressure / 4),
                0,
                100);
            int cohesion = Math.Clamp(
                40 + (settlement.BanditThreat / 4) - (settlement.ResponseActivationLevel / 5),
                0,
                100);
            int grainReserve = Math.Clamp(
                (settlement.BanditThreat / 3) + (settlement.BlackRoutePressure / 4),
                0,
                100);

            if (prior is not null)
            {
                strength = BlendTowards(prior.Strength, strength);
                legitimacy = BlendTowards(prior.Legitimacy, legitimacy);
                cohesion = BlendTowards(prior.Cohesion, cohesion);
                grainReserve = BlendTowards(prior.GrainReserve, grainReserve);
            }

            BandConcentration concentration = ClassifyConcentration(strength, legitimacy);

            OutlawBandState band = new()
            {
                BandId = bandId,
                BandName = prior?.BandName is { Length: > 0 } name ? name : BuildBandName(settlement),
                BaseSettlementId = settlement.SettlementId,
                Strength = strength,
                GrainReserve = grainReserve,
                Cohesion = cohesion,
                Legitimacy = legitimacy,
                Concentration = concentration,
                ControlledRoutes = prior?.ControlledRoutes is { Count: > 0 } routes
                    ? new List<string>(routes)
                    : new List<string>(),
            };

            next.Add(band);
        }

        state.OutlawBands = next
            .OrderBy(static band => band.BaseSettlementId.Value)
            .ThenBy(static band => band.BandId, StringComparer.Ordinal)
            .ToList();
    }

    private static BandConcentration ClassifyConcentration(int strength, int legitimacy)
    {
        if (strength >= 80 && legitimacy >= 55)
        {
            return BandConcentration.RebelGovernance;
        }

        if (strength >= 70)
        {
            return BandConcentration.TerritoryHolding;
        }

        if (strength >= 55)
        {
            return BandConcentration.RouteHolding;
        }

        if (strength >= 35)
        {
            return BandConcentration.Roaming;
        }

        return BandConcentration.Scattered;
    }

    private static int BlendTowards(int prior, int target)
    {
        return Math.Clamp((prior + target + target) / 3, 0, 100);
    }

    private static string BuildBandId(SettlementId settlementId)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"outlaw-band-{settlementId.Value}");
    }

    private static string BuildBandName(SettlementDisorderState settlement)
    {
        return string.Create(
            CultureInfo.InvariantCulture,
            $"乡野群盗 · {settlement.SettlementId.Value}");
    }
}
