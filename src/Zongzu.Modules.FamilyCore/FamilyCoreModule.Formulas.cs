using System;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed partial class FamilyCoreModule
{
    internal static void ApplyXunClanPulse(
        SimulationXun currentXun,
        ClanStateData clan,
        FamilyMonthSignals signals,
        SettlementSnapshot homeSettlement)
    {
        switch (currentXun)
        {
            case SimulationXun.Shangxun:
                if (clan.MourningLoad >= 18 || signals.InfantCount > 0)
                {
                    clan.SupportReserve = Math.Max(0, clan.SupportReserve - 1);
                }

                if (clan.HeirSecurity < 40
                    && signals.AdultCount <= 1
                    && clan.MarriageAllianceValue < 45)
                {
                    clan.MarriageAlliancePressure = Math.Clamp(clan.MarriageAlliancePressure + 1, 0, 100);
                }

                break;
            case SimulationXun.Zhongxun:
            {
                int branchDelta = 0;
                if (clan.BranchFavorPressure >= 45 || clan.ReliefSanctionPressure >= 40)
                {
                    branchDelta += 1;
                }

                if (homeSettlement.Security < 45)
                {
                    branchDelta += 1;
                }

                if (clan.MediationMomentum >= 45 && branchDelta > 0)
                {
                    branchDelta -= 1;
                }

                clan.BranchTension = Math.Clamp(clan.BranchTension + branchDelta, 0, 100);
                break;
            }
            case SimulationXun.Xiaxun:
                if (clan.HeirSecurity < 40)
                {
                    clan.InheritancePressure = Math.Clamp(clan.InheritancePressure + 1, 0, 100);
                }

                if (clan.BranchTension >= 55)
                {
                    clan.SeparationPressure = Math.Clamp(clan.SeparationPressure + 1, 0, 100);
                }

                if (clan.MarriageAllianceValue >= 45
                    && signals.ChildCount == 0
                    && clan.MourningLoad < 18)
                {
                    clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + 1, 0, 100);
                }

                break;
        }
    }

    internal static FamilyMonthSignals AnalyzeClan(
        FamilyCoreState state,
        ClanStateData clan,
        IPersonRegistryQueries registryQueries,
        GameDate currentDate)
    {
        FamilyPersonAge[] livingPeople = state.People
            .Where(person => person.ClanId == clan.Id && IsPersonAlive(person, registryQueries))
            .Select(person => new FamilyPersonAge(person, GetAgeMonths(person, registryQueries, currentDate)))
            .OrderByDescending(static entry => entry.AgeMonths)
            .ThenBy(static entry => entry.Person.Id.Value)
            .ToArray();

        FamilyPersonAge? livingHeir = null;
        if (clan.HeirPersonId is not null)
        {
            PersonId heirId = clan.HeirPersonId.Value;
            foreach (FamilyPersonAge entry in livingPeople)
            {
                if (entry.Person.Id == heirId)
                {
                    livingHeir = entry;
                    break;
                }
            }
        }

        int adultCount = livingPeople.Count(static entry => entry.AgeMonths >= AdultAgeMonths && entry.AgeMonths < ElderAgeMonths);
        int childCount = livingPeople.Count(static entry => entry.AgeMonths < AdultAgeMonths);
        int elderCount = livingPeople.Count(static entry => entry.AgeMonths >= ElderAgeMonths);
        int infantCount = livingPeople.Count(static entry => entry.AgeMonths <= InfantAgeMonths);

        return new FamilyMonthSignals(livingPeople, livingHeir, adultCount, childCount, elderCount, infantCount);
    }

    internal static bool IsPersonAlive(FamilyPersonState person, IPersonRegistryQueries registryQueries)
    {
        if (registryQueries.TryGetPerson(person.Id, out PersonRecord record))
        {
            return record.IsAlive;
        }

        return person.IsAlive;
    }

    internal static int GetAgeMonths(FamilyPersonState person, IPersonRegistryQueries registryQueries, GameDate currentDate)
    {
        int registryAge = registryQueries.GetAgeMonths(person.Id, currentDate);
        return registryAge >= 0 ? registryAge : person.AgeMonths;
    }

    internal static bool ClanStateUnchanged(
        ClanStateData clan,
        int oldPrestige,
        int oldSupportReserve,
        int oldBranchTension,
        int oldInheritancePressure,
        int oldSeparationPressure,
        int oldMediationMomentum,
        int oldMarriageAlliancePressure,
        int oldMarriageAllianceValue,
        int oldHeirSecurity,
        int oldReproductivePressure,
        int oldMourningLoad)
    {
        return clan.Prestige == oldPrestige
               && clan.SupportReserve == oldSupportReserve
               && clan.BranchTension == oldBranchTension
               && clan.InheritancePressure == oldInheritancePressure
               && clan.SeparationPressure == oldSeparationPressure
               && clan.MediationMomentum == oldMediationMomentum
               && clan.MarriageAlliancePressure == oldMarriageAlliancePressure
               && clan.MarriageAllianceValue == oldMarriageAllianceValue
               && clan.HeirSecurity == oldHeirSecurity
               && clan.ReproductivePressure == oldReproductivePressure
               && clan.MourningLoad == oldMourningLoad;
    }

    internal static int ComputeMarriageAlliancePressureDelta(ClanStateData clan, FamilyMonthSignals signals)
    {
        int delta = 0;
        delta += signals.AdultCount <= 1 && clan.MarriageAllianceValue < 45 ? 2 : 0;
        delta += clan.HeirSecurity < 40 ? 1 : 0;
        delta += signals.ChildCount == 0 ? 1 : 0;
        delta -= clan.MarriageAllianceValue >= 45 ? 2 : 0;
        delta -= clan.MourningLoad >= 18 ? 1 : 0;
        return delta;
    }

    internal static int ComputeReproductivePressureDelta(ClanStateData clan, FamilyMonthSignals signals)
    {
        int delta = 0;
        delta += clan.MarriageAllianceValue >= 45 && signals.ChildCount == 0 ? 2 : 0;
        delta += clan.HeirSecurity < 45 ? 1 : 0;
        delta -= signals.InfantCount > 0 ? 3 : 0;
        delta -= signals.ChildCount > 0 ? 1 : 0;
        delta -= clan.MourningLoad >= 18 ? 1 : 0;
        return delta;
    }

    internal static int ComputeHeirSecurity(ClanStateData clan, FamilyMonthSignals signals)
    {
        int score;
        if (signals.LivingHeir is null)
        {
            score = 18;
        }
        else if (signals.LivingHeir.Value.AgeMonths >= SecureHeirAgeMonths)
        {
            score = 72;
        }
        else if (signals.LivingHeir.Value.AgeMonths >= AdultAgeMonths)
        {
            score = 54;
        }
        else
        {
            score = 32;
        }

        score += clan.MarriageAllianceValue / 6;
        score += signals.ChildCount > 0 ? 4 : 0;
        score -= clan.InheritancePressure / 4;
        score -= clan.MourningLoad / 5;
        return Math.Clamp(score, 0, 100);
    }

    internal static int ComputeBranchTensionDelta(ClanStateData clan, SettlementSnapshot settlement)
    {
        int delta = 0;
        delta += clan.BranchFavorPressure >= 45 ? 2 : clan.BranchFavorPressure >= 20 ? 1 : 0;
        delta += clan.ReliefSanctionPressure >= 40 ? 2 : clan.ReliefSanctionPressure >= 18 ? 1 : 0;
        delta += clan.InheritancePressure >= 55 ? 1 : 0;
        delta += clan.SeparationPressure >= 60 ? 1 : 0;
        delta += clan.MourningLoad >= 22 ? 1 : 0;
        delta += settlement.Security < 45 ? 1 : 0;
        delta -= clan.MediationMomentum >= 55 ? 2 : clan.MediationMomentum >= 28 ? 1 : 0;
        return delta;
    }

    internal static int ComputeInheritancePressureDelta(ClanStateData clan, FamilyMonthSignals signals)
    {
        int delta = 0;
        delta += signals.LivingHeir is null ? 2 : 1;
        delta += clan.HeirSecurity < 40 ? 1 : 0;
        delta += clan.Prestige >= 60 ? 1 : 0;
        delta -= clan.MediationMomentum >= 50 ? 1 : 0;
        return delta;
    }

    internal static int ComputeSeparationPressureDelta(ClanStateData clan)
    {
        int delta = 0;
        delta += clan.BranchTension >= 65 ? 2 : clan.BranchTension >= 45 ? 1 : 0;
        delta += clan.ReliefSanctionPressure >= 45 ? 1 : 0;
        delta += clan.MourningLoad >= 22 ? 1 : 0;
        delta -= clan.MediationMomentum >= 55 ? 1 : 0;
        return delta;
    }

    /// <summary>
    /// STEP2A / A7 — 成年男性拥挤 + 无分房调停 → 分房之议逐月累积。
    /// 触发条件：本 clan 在世成年男性（heir-eligible gender，未登记者默认男）
    /// ≥ 3 人 且 <c>MediationMomentum &lt; 55</c>（族老尚未开分房议）。每月 +1。
    /// skill <c>lineage-inheritance</c> / <c>household-separation</c>：分房是
    /// 北宋多子家族真实压力，此 step 只累积信号，不执行 <c>PermitBranchSeparation</c>，
    /// 那一步留给 Step 2-B 的玩家命令通道。
    /// </summary>
    internal static int ComputeAdultMaleCrowdingSeparationDelta(
        FamilyCoreState state,
        ClanStateData clan,
        IPersonRegistryQueries registryQueries,
        GameDate currentDate)
    {
        if (clan.MediationMomentum >= 55) return 0;

        int adultMales = 0;
        foreach (FamilyPersonState person in state.People)
        {
            if (person.ClanId != clan.Id) continue;
            if (!IsPersonAlive(person, registryQueries)) continue;
            int age = GetAgeMonths(person, registryQueries, currentDate);
            if (age < AdultAgeMonths) continue;
            if (!IsHeirEligibleGender(person.Id, registryQueries)) continue;
            adultMales += 1;
            if (adultMales >= 3) return 1;
        }
        return 0;
    }

    internal static int ComputeCampaignPrestigeDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = 0;
        if (campaign.MoraleState >= 62 && campaign.SupplyState >= 50 && !bundle.CampaignSupplyStrained)
        {
            delta += 1;
        }

        delta -= bundle.CampaignPressureRaised ? 1 : 0;
        delta -= bundle.CampaignSupplyStrained ? 2 : 0;
        delta -= bundle.CampaignAftermathRegistered ? 1 : 0;
        return Math.Clamp(delta, -3, 1);
    }

    internal static int ComputeCampaignSupportDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = 0;
        delta -= bundle.CampaignMobilized ? 2 : 0;
        delta -= bundle.CampaignPressureRaised ? 1 : 0;
        delta -= bundle.CampaignSupplyStrained ? 2 : 0;
        delta -= bundle.CampaignAftermathRegistered ? 1 : 0;
        delta -= Math.Max(0, campaign.MobilizedForceCount - 24) / 24;
        return Math.Clamp(delta, -8, 0);
    }
}
