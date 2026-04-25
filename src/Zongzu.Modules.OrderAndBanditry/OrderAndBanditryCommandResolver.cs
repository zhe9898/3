using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed class OrderAndBanditryCommandContext
{
    public OrderAndBanditryState State { get; init; } = new();

    public PlayerCommandRequest Command { get; init; } = new();

    public IOfficeAndCareerQueries? OfficeQueries { get; init; }

    public IFamilyCoreQueries? FamilyQueries { get; init; }

    public ISocialMemoryAndRelationsQueries? SocialMemoryQueries { get; init; }
}

public static class OrderAndBanditryCommandResolver
{
    public static PlayerCommandResult IssueIntent(OrderAndBanditryCommandContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        PlayerCommandRequest command = context.Command;
        OrderAdministrativeReachProfile administrativeReach = ResolveAdministrativeReach(context.OfficeQueries, command.SettlementId);
        PublicLifeResponseResidueFriction responseFriction = ResolvePublicLifeResponseResidueFriction(
            context.SocialMemoryQueries,
            context.FamilyQueries,
            command);
        OrderPublicLifeCommandResult resolution = new OrderAndBanditryModule().HandlePublicLifeCommand(
            context.State,
            new OrderPublicLifeCommand
            {
                SettlementId = command.SettlementId,
                CommandName = command.CommandName,
                CommandLabel = DeterminePublicLifeCommandLabel(command.CommandName),
                BenefitShift = administrativeReach.BenefitShift + responseFriction.OrderBenefitShift,
                ShieldingShift = administrativeReach.ShieldingShift,
                BacklashShift = administrativeReach.BacklashShift + responseFriction.OrderBacklashShift,
                LeakageShift = administrativeReach.LeakageShift,
                ReachSummaryTail = CombineSummaryTails(administrativeReach.SummaryTail, responseFriction.SummaryTail),
                ResponseRepairSupport = responseFriction.RepairSupport,
                ResponseHardeningDrag = responseFriction.HardeningDrag,
                ResponseResidueSummaryTail = responseFriction.SummaryTail,
            });

        return new PlayerCommandResult
        {
            Accepted = resolution.Accepted,
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
            SettlementId = command.SettlementId,
            ClanId = command.ClanId,
            CommandName = command.CommandName,
            Label = resolution.Label,
            Summary = resolution.Summary,
            TargetLabel = resolution.Accepted ? $"据点 {command.SettlementId.Value}" : string.Empty,
        };
    }

    public static string DeterminePublicLifeCommandLabel(string commandName)
    {
        return commandName switch
        {
            PlayerCommandNames.EscortRoadReport => "催护一路",
            PlayerCommandNames.FundLocalWatch => "添雇巡丁",
            PlayerCommandNames.SuppressBanditry => "严缉路匪",
            PlayerCommandNames.NegotiateWithOutlaws => "遣人议路",
            PlayerCommandNames.TolerateDisorder => "暂缓穷追",
            PlayerCommandNames.RepairLocalWatchGuarantee => "补保巡丁",
            PlayerCommandNames.CompensateRunnerMisread => "赔脚户误读",
            PlayerCommandNames.DeferHardPressure => "暂缓强压",
            _ => commandName,
        };
    }

    public static string DetermineAdministrativeReachExecutionSummary(JurisdictionAuthoritySnapshot? jurisdiction)
    {
        return EvaluateAdministrativeReach(jurisdiction).ExecutionSummary;
    }

    private static OrderAdministrativeReachProfile ResolveAdministrativeReach(
        IOfficeAndCareerQueries? officeQueries,
        SettlementId settlementId)
    {
        if (officeQueries is null)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        try
        {
            return EvaluateAdministrativeReach(officeQueries.GetRequiredJurisdiction(settlementId));
        }
        catch (InvalidOperationException)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }
    }

    private static OrderAdministrativeReachProfile EvaluateAdministrativeReach(JurisdictionAuthoritySnapshot? jurisdiction)
    {
        if (jurisdiction is null)
        {
            return OrderAdministrativeReachProfile.Neutral;
        }

        int supportSignal =
            jurisdiction.JurisdictionLeverage
            + jurisdiction.ClerkDependence
            + (jurisdiction.AuthorityTier * 10);
        int frictionSignal =
            jurisdiction.AdministrativeTaskLoad
            + jurisdiction.PetitionPressure
            + (jurisdiction.PetitionBacklog / 2);
        int netSignal = supportSignal - frictionSignal;

        if (netSignal >= 40)
        {
            return new OrderAdministrativeReachProfile(
                3,
                5,
                -4,
                -3,
                "县署肯出手，文移与差役都跟得上。",
                "县署肯出手，此令多半能照行到底。");
        }

        if (netSignal >= 15)
        {
            return new OrderAdministrativeReachProfile(
                1,
                2,
                -2,
                -1,
                "县署还能接得住，文移差役尚能随令。",
                "县署还能接得住，此令大体跟得上。");
        }

        if (netSignal <= -40)
        {
            return new OrderAdministrativeReachProfile(
                -3,
                -5,
                4,
                3,
                "县署拥案未解，文移不畅，路上只得勉强敷衍。",
                "县署拥案未解，此令多半只落在文面，地面跟得慢。");
        }

        if (netSignal <= -15)
        {
            return new OrderAdministrativeReachProfile(
                -1,
                -2,
                2,
                1,
                "县署案前偏重，差役跟得慢，只能先做半套。",
                "县署案前偏重，此令常要拖成半套。");
        }

        return OrderAdministrativeReachProfile.Neutral;
    }

    private static PublicLifeResponseResidueFriction ResolvePublicLifeResponseResidueFriction(
        ISocialMemoryAndRelationsQueries? socialQueries,
        IFamilyCoreQueries? familyQueries,
        PlayerCommandRequest command)
    {
        if (socialQueries is null)
        {
            return PublicLifeResponseResidueFriction.Neutral;
        }

        HashSet<ClanId> localClanIds = ResolveLocalClanIds(familyQueries, command);
        if (localClanIds.Count == 0)
        {
            return PublicLifeResponseResidueFriction.Neutral;
        }

        int repaired = 0;
        int contained = 0;
        int escalated = 0;
        int ignored = 0;

        foreach (SocialMemoryEntrySnapshot memory in socialQueries.GetMemories()
                     .Where(static memory => memory.State == MemoryLifecycleState.Active)
                     .Where(static memory => memory.CauseKey.StartsWith("order.public_life.response.", StringComparison.Ordinal))
                     .Where(memory => memory.SourceClanId.HasValue && localClanIds.Contains(memory.SourceClanId.Value)))
        {
            if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Repaired}.", StringComparison.Ordinal))
            {
                repaired += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Contained}.", StringComparison.Ordinal))
            {
                contained += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Escalated}.", StringComparison.Ordinal))
            {
                escalated += memory.Weight;
            }
            else if (memory.CauseKey.Contains($".{PublicLifeOrderResponseOutcomeCodes.Ignored}.", StringComparison.Ordinal))
            {
                ignored += memory.Weight;
            }
        }

        return PublicLifeResponseResidueFriction.FromWeights(repaired, contained, escalated, ignored);
    }

    private static HashSet<ClanId> ResolveLocalClanIds(
        IFamilyCoreQueries? familyQueries,
        PlayerCommandRequest command)
    {
        if (command.ClanId.HasValue)
        {
            return [command.ClanId.Value];
        }

        return familyQueries is null
            ? []
            : familyQueries.GetClans()
                .Where(clan => clan.HomeSettlementId == command.SettlementId)
                .Select(static clan => clan.Id)
                .ToHashSet();
    }

    private static string CombineSummaryTails(string first, string second)
    {
        if (string.IsNullOrWhiteSpace(first))
        {
            return second;
        }

        if (string.IsNullOrWhiteSpace(second))
        {
            return first;
        }

        return $"{first}{second}";
    }

    private readonly record struct PublicLifeResponseResidueFriction(
        int RepairedWeight,
        int ContainedWeight,
        int EscalatedWeight,
        int IgnoredWeight)
    {
        public int RepairSupport => Math.Clamp((RepairedWeight / 18) + (ContainedWeight / 30), 0, 4);

        public int HardeningDrag => Math.Clamp((EscalatedWeight / 14) + (IgnoredWeight / 18), 0, 6);

        public int OrderBenefitShift => Math.Clamp(RepairSupport - (HardeningDrag / 2), -3, 4);

        public int OrderBacklashShift => HardeningDrag;

        public string SummaryTail => RepairSupport == 0 && HardeningDrag == 0
            ? string.Empty
            : $" 社会记忆回读：修复余重{RepairedWeight}、暂压余重{ContainedWeight}、恶化余重{EscalatedWeight}、放置余重{IgnoredWeight}。";

        public static PublicLifeResponseResidueFriction Neutral => new(0, 0, 0, 0);

        public static PublicLifeResponseResidueFriction FromWeights(
            int repairedWeight,
            int containedWeight,
            int escalatedWeight,
            int ignoredWeight)
        {
            return new(
                Math.Clamp(repairedWeight, 0, 200),
                Math.Clamp(containedWeight, 0, 200),
                Math.Clamp(escalatedWeight, 0, 200),
                Math.Clamp(ignoredWeight, 0, 200));
        }
    }

    private readonly record struct OrderAdministrativeReachProfile(
        int BenefitShift,
        int ShieldingShift,
        int BacklashShift,
        int LeakageShift,
        string SummaryTail,
        string ExecutionSummary)
    {
        public static OrderAdministrativeReachProfile Neutral => new(
            0,
            0,
            0,
            0,
            string.Empty,
            "此地眼下多凭本地人手与地面情势，官面帮衬未显。");
    }
}
