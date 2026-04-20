using System;
using System.Collections.Generic;
using Zongzu.Contracts;

namespace Zongzu.Modules.SocialMemoryAndRelations;

/// <summary>
/// Phase 4 记忆骨骼 schema migration（<c>LIVING_WORLD_DESIGN §2.4</c>）。
/// v1 → v2：把旧 string <c>Kind</c> 记忆条目提升为结构化
/// <see cref="MemoryType"/>/<see cref="MemorySubtype"/> + 生命周期 + 原因键，
/// 并初始化 <see cref="SocialMemoryAndRelationsState.DormantStubs"/> 容器。
/// </summary>
public static class SocialMemoryAndRelationsStateProjection
{
    public static void UpgradeFromSchemaV1ToV2(SocialMemoryAndRelationsState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.DormantStubs ??= new List<DormantStubState>();

        foreach (MemoryRecordState memory in state.Memories)
        {
            (MemoryType type, MemorySubtype subtype, int decay) = ClassifyLegacyKind(memory.Kind);

            if (memory.Type == MemoryType.Unknown || memory.Type == MemoryType.Grudge && !string.IsNullOrEmpty(memory.Kind) && memory.Subtype == MemorySubtype.Unknown)
            {
                memory.Type = type;
            }

            if (memory.Subtype == MemorySubtype.Unknown)
            {
                memory.Subtype = subtype;
            }

            if (string.IsNullOrEmpty(memory.CauseKey))
            {
                memory.CauseKey = string.IsNullOrEmpty(memory.Kind) ? "legacy.unknown" : $"legacy.{memory.Kind}";
            }

            if (memory.SourceKind == MemorySubjectKind.Unknown)
            {
                memory.SourceKind = MemorySubjectKind.Clan;
                memory.SourceClanId = memory.SubjectClanId;
            }

            if (memory.TargetKind == MemorySubjectKind.Unknown)
            {
                memory.TargetKind = MemorySubjectKind.Clan;
                memory.TargetClanId = memory.SubjectClanId;
            }

            if (memory.OriginDate.Year == 0)
            {
                memory.OriginDate = memory.CreatedAt;
            }

            if (memory.Weight == 0)
            {
                memory.Weight = Math.Clamp(memory.Intensity, 0, 100);
            }

            if (memory.MonthlyDecay == 0)
            {
                memory.MonthlyDecay = decay;
            }

            if (memory.LifecycleState == MemoryLifecycleState.Unknown)
            {
                memory.LifecycleState = MemoryLifecycleState.Active;
            }
        }
    }

    private static (MemoryType Type, MemorySubtype Subtype, int MonthlyDecay) ClassifyLegacyKind(string kind)
    {
        return kind switch
        {
            "hardship" => (MemoryType.Grudge, MemorySubtype.WealthGrudge, 2),
            "conciliation" => (MemoryType.Favor, MemorySubtype.ReliefFavor, 3),
            "campaign-aftermath" => (MemoryType.Fear, MemorySubtype.WarDread, 1),
            "campaign-pressure" => (MemoryType.Fear, MemorySubtype.WarDread, 2),
            _ => (MemoryType.Grudge, MemorySubtype.Unknown, 2),
        };
    }
}
