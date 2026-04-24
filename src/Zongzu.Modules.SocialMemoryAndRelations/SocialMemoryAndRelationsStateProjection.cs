using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

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
        state.ClanEmotionalClimates ??= new List<ClanEmotionalClimateState>();
        state.PersonTemperings ??= new List<PersonPressureTemperingState>();
        NormalizeTemperingDates(state);

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

    public static void UpgradeFromSchemaV2ToV3(SocialMemoryAndRelationsState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        state.ClanEmotionalClimates ??= new List<ClanEmotionalClimateState>();
        state.PersonTemperings ??= new List<PersonPressureTemperingState>();
        NormalizeTemperingDates(state);

        foreach (ClanNarrativeState narrative in state.ClanNarratives)
        {
            if (state.ClanEmotionalClimates.Any(climate => climate.ClanId == narrative.ClanId))
            {
                continue;
            }

            int pressureScore = Math.Clamp(
                (narrative.FearPressure + narrative.ShamePressure + narrative.GrudgePressure) / 2,
                0,
                100);
            state.ClanEmotionalClimates.Add(new ClanEmotionalClimateState
            {
                ClanId = narrative.ClanId,
                Fear = Math.Clamp(narrative.FearPressure, 0, 100),
                Shame = Math.Clamp(narrative.ShamePressure, 0, 100),
                Anger = Math.Clamp(narrative.GrudgePressure / 2, 0, 100),
                Obligation = Math.Clamp(Math.Max(0, narrative.FavorBalance), 0, 100),
                Trust = Math.Clamp(Math.Max(0, narrative.FavorBalance), 0, 100),
                Bitterness = Math.Clamp(narrative.GrudgePressure / 3, 0, 100),
                LastPressureScore = pressureScore,
                LastPressureBand = ResolveBand(pressureScore),
                LastTemperingBand = 0,
                LastUpdated = new GameDate(1, 1),
                LastTrace = "schema2 narrative backfill",
            });
        }

        state.ClanEmotionalClimates = state.ClanEmotionalClimates
            .OrderBy(static climate => climate.ClanId.Value)
            .ToList();
        state.PersonTemperings = state.PersonTemperings
            .OrderBy(static entry => entry.PersonId.Value)
            .ToList();
    }

    private static void NormalizeTemperingDates(SocialMemoryAndRelationsState state)
    {
        foreach (ClanEmotionalClimateState climate in state.ClanEmotionalClimates)
        {
            if (climate.LastUpdated.Month == 0)
            {
                climate.LastUpdated = new GameDate(1, 1);
            }
        }

        foreach (PersonPressureTemperingState tempering in state.PersonTemperings)
        {
            if (tempering.LastUpdated.Month == 0)
            {
                tempering.LastUpdated = new GameDate(1, 1);
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

    private static int ResolveBand(int value)
    {
        return value switch
        {
            >= 80 => 3,
            >= 60 => 2,
            >= 40 => 1,
            _ => 0,
        };
    }
}
