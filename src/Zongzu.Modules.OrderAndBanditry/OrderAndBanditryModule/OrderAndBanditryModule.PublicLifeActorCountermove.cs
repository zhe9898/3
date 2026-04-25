using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.OrderAndBanditry;

public sealed partial class OrderAndBanditryModule
{
    private static void ApplyPublicLifeOrderActorCountermove(
        ModuleExecutionScope<OrderAndBanditryState> scope,
        SettlementDisorderState disorder,
        IReadOnlyList<ClanSnapshot> localClans,
        ISocialMemoryAndRelationsQueries socialQueries)
    {
        PublicLifeResponseResidueWeights weights = PublicLifeResponseResidueWeights.FromSocialMemory(
            socialQueries,
            localClans.Select(static clan => clan.Id),
            scope.Context.CurrentDate);
        if (weights.IsNeutral)
        {
            return;
        }

        if (weights.HardeningDrag >= Math.Max(24, weights.SoftSupport + 10)
            && (disorder.RoutePressure >= 35 || disorder.RetaliationRisk >= 35 || disorder.CoercionRisk >= 30)
            && !string.Equals(
                disorder.LastRefusalResponseTraceCode,
                PublicLifeOrderResponseTraceCodes.OrderActorRunnerMisreadHardened,
                StringComparison.Ordinal))
        {
            disorder.RoutePressure = Math.Clamp(disorder.RoutePressure + 2, 0, 100);
            disorder.ImplementationDrag = Math.Clamp(disorder.ImplementationDrag + 1, 0, 100);
            disorder.RetaliationRisk = Math.Clamp(disorder.RetaliationRisk + 4, 0, 100);
            disorder.CoercionRisk = Math.Clamp(disorder.CoercionRisk + 3, 0, 100);
            disorder.LastRefusalResponseCommandCode = PlayerCommandNames.CompensateRunnerMisread;
            disorder.LastRefusalResponseCommandLabel = "脚户误读反噬";
            disorder.LastRefusalResponseOutcomeCode = PublicLifeOrderResponseOutcomeCodes.Escalated;
            disorder.LastRefusalResponseTraceCode = PublicLifeOrderResponseTraceCodes.OrderActorRunnerMisreadHardened;
            disorder.LastRefusalResponseSummary =
                $"旧案恶化余重{weights.HardeningDrag}仍压在路口，脚户与巡丁把前案误读成强催，路压{disorder.RoutePressure}、怨尾{disorder.RetaliationRisk}再起。";
            disorder.ResponseCarryoverMonths = 1;
            scope.RecordDiff(
                $"路上后账被脚户误读，{disorder.SettlementId.Value}路压{disorder.RoutePressure}，怨尾{disorder.RetaliationRisk}。",
                disorder.SettlementId.Value.ToString());
            return;
        }

        if (weights.SoftSupport >= 18
            && (disorder.RoutePressure >= 25 || disorder.ImplementationDrag >= 18 || disorder.RetaliationRisk >= 10)
            && !string.Equals(
                disorder.LastRefusalResponseTraceCode,
                PublicLifeOrderResponseTraceCodes.OrderActorLocalWatchSelfSettled,
                StringComparison.Ordinal))
        {
            disorder.RoutePressure = Math.Clamp(disorder.RoutePressure - 2, 0, 100);
            disorder.ImplementationDrag = Math.Clamp(disorder.ImplementationDrag - 2, 0, 100);
            disorder.RouteShielding = Math.Clamp(disorder.RouteShielding + 3, 0, 100);
            disorder.RetaliationRisk = Math.Clamp(disorder.RetaliationRisk - 3, 0, 100);
            disorder.LastRefusalResponseCommandCode = PlayerCommandNames.RepairLocalWatchGuarantee;
            disorder.LastRefusalResponseCommandLabel = "巡丁自补保";
            disorder.LastRefusalResponseOutcomeCode = weights.RepairedWeight >= weights.ContainedWeight
                ? PublicLifeOrderResponseOutcomeCodes.Repaired
                : PublicLifeOrderResponseOutcomeCodes.Contained;
            disorder.LastRefusalResponseTraceCode = PublicLifeOrderResponseTraceCodes.OrderActorLocalWatchSelfSettled;
            disorder.LastRefusalResponseSummary =
                $"修复余重{weights.RepairedWeight}、暂压余重{weights.ContainedWeight}让巡丁与脚户愿意自补担保，路压{disorder.RoutePressure}，护路{disorder.RouteShielding}。";
            disorder.ResponseCarryoverMonths = 1;
            scope.RecordDiff(
                $"巡丁脚户自补前案担保，{disorder.SettlementId.Value}路压{disorder.RoutePressure}，护路{disorder.RouteShielding}。",
                disorder.SettlementId.Value.ToString());
        }
    }

    private readonly record struct PublicLifeResponseResidueWeights(
        int RepairedWeight,
        int ContainedWeight,
        int EscalatedWeight,
        int IgnoredWeight)
    {
        public bool IsNeutral => RepairedWeight == 0 && ContainedWeight == 0 && EscalatedWeight == 0 && IgnoredWeight == 0;

        public int SoftSupport => RepairedWeight + (ContainedWeight / 2);

        public int HardeningDrag => EscalatedWeight + (IgnoredWeight / 2);

        public static PublicLifeResponseResidueWeights FromSocialMemory(
            ISocialMemoryAndRelationsQueries socialQueries,
            IEnumerable<ClanId> clanIds,
            GameDate currentDate)
        {
            HashSet<ClanId> localClanIds = clanIds.ToHashSet();
            if (localClanIds.Count == 0)
            {
                return default;
            }

            int repaired = 0;
            int contained = 0;
            int escalated = 0;
            int ignored = 0;

            foreach (SocialMemoryEntrySnapshot memory in socialQueries.GetMemories()
                         .Where(static memory => memory.State == MemoryLifecycleState.Active)
                         .Where(static memory => memory.CauseKey.StartsWith("order.public_life.response.", StringComparison.Ordinal))
                         .Where(memory => memory.SourceClanId.HasValue && localClanIds.Contains(memory.SourceClanId.Value))
                         .Where(memory => !IsCurrentMonth(memory.OriginDate, currentDate)))
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

            return new PublicLifeResponseResidueWeights(
                Math.Clamp(repaired, 0, 200),
                Math.Clamp(contained, 0, 200),
                Math.Clamp(escalated, 0, 200),
                Math.Clamp(ignored, 0, 200));
        }

        private static bool IsCurrentMonth(GameDate date, GameDate currentDate)
        {
            return date.Year == currentDate.Year && date.Month == currentDate.Month;
        }
    }
}
