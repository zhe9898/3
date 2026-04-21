using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.EducationAndExams;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.PublicLifeAndRumor;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Persistence;

namespace Zongzu.Application;

public static partial class SimulationBootstrapper
{
    private static string DetermineMigratedEscalationBandLabel(int blackRoutePressure, int coercionRisk)
    {
        int combined = blackRoutePressure + coercionRisk;
        return combined switch
        {
            >= 130 => "私路成势",
            >= 100 => "暗运成线",
            >= 70 => "夹带渐多",
            >= 40 => "私贩试探",
            _ => "尚未成势",
        };
    }

    private static string DetermineMigratedDiversionBandLabel(int diversionShare, int seizureRisk, int illicitMargin)
    {
        int combined = diversionShare + seizureRisk + Math.Max(0, illicitMargin);
        return combined switch
        {
            >= 120 => "私货成路",
            >= 85 => "正私并行",
            >= 55 => "夹带渐增",
            >= 25 => "零星夹带",
            _ => "尚未分流",
        };
    }

    private static string DetermineMigratedRouteConstraintLabel(int blockedShipmentCount, int seizureRisk, int routeRisk)
    {
        int combined = (blockedShipmentCount * 12) + seizureRisk + (routeRisk / 2);
        return combined switch
        {
            >= 120 => "盘查封路",
            >= 85 => "卡口渐密",
            >= 50 => "时有阻滞",
            >= 20 => "尚可通行",
            _ => "行路平稳",
        };
    }

    private static SaveMigrationPipeline CreateDefaultMigrationPipeline()
    {
        SaveMigrationPipeline pipeline = new();
        pipeline.RegisterModuleMigration(KnownModuleKeys.WorldSettlements, 1, 2, MigrateWorldSettlementsStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WorldSettlements, 2, 3, MigrateWorldSettlementsStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WorldSettlements, 3, 4, MigrateWorldSettlementsStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WorldSettlements, 4, 5, MigrateWorldSettlementsStateV4ToV5);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WorldSettlements, 5, 6, MigrateWorldSettlementsStateV5ToV6);
        pipeline.RegisterModuleMigration(KnownModuleKeys.FamilyCore, 1, 2, MigrateFamilyCoreStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.FamilyCore, 2, 3, MigrateFamilyCoreStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.FamilyCore, 3, 4, MigrateFamilyCoreStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.FamilyCore, 4, 5, MigrateFamilyCoreStateV4ToV5);
        pipeline.RegisterModuleMigration(KnownModuleKeys.FamilyCore, 5, 6, MigrateFamilyCoreStateV5ToV6);
        pipeline.RegisterModuleMigration(KnownModuleKeys.FamilyCore, 6, 7, MigrateFamilyCoreStateV6ToV7);
        pipeline.RegisterModuleMigration(KnownModuleKeys.PublicLifeAndRumor, 1, 2, MigratePublicLifeAndRumorStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.PublicLifeAndRumor, 2, 3, MigratePublicLifeAndRumorStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.PublicLifeAndRumor, 3, 4, MigratePublicLifeAndRumorStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OfficeAndCareer, 1, 2, MigrateOfficeAndCareerStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OfficeAndCareer, 2, 3, MigrateOfficeAndCareerStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OfficeAndCareer, 3, 4, MigrateOfficeAndCareerStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.TradeAndIndustry, 1, 2, MigrateTradeAndIndustryStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.TradeAndIndustry, 2, 3, MigrateTradeAndIndustryStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.TradeAndIndustry, 3, 4, MigrateTradeAndIndustryStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 1, 2, MigrateOrderAndBanditryStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 2, 3, MigrateOrderAndBanditryStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 3, 4, MigrateOrderAndBanditryStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 4, 5, MigrateOrderAndBanditryStateV4ToV5);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 5, 6, MigrateOrderAndBanditryStateV5ToV6);
        pipeline.RegisterModuleMigration(KnownModuleKeys.OrderAndBanditry, 6, 7, MigrateOrderAndBanditryStateV6ToV7);
        pipeline.RegisterModuleMigration(KnownModuleKeys.ConflictAndForce, 1, 2, MigrateConflictAndForceStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.ConflictAndForce, 2, 3, MigrateConflictAndForceStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.ConflictAndForce, 3, 4, MigrateConflictAndForceStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 1, 2, MigrateWarfareCampaignStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 2, 3, MigrateWarfareCampaignStateV2ToV3);
        pipeline.RegisterModuleMigration(KnownModuleKeys.WarfareCampaign, 3, 4, MigrateWarfareCampaignStateV3ToV4);
        pipeline.RegisterModuleMigration(KnownModuleKeys.PopulationAndHouseholds, 1, 2, MigratePopulationAndHouseholdsStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.SocialMemoryAndRelations, 1, 2, MigrateSocialMemoryAndRelationsStateV1ToV2);
        pipeline.RegisterModuleMigration(KnownModuleKeys.EducationAndExams, 1, 2, MigrateEducationAndExamsStateV1ToV2);
        return pipeline;
    }

    private static ModuleStateEnvelope MigrateEducationAndExamsStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        EducationAndExamsState migratedState = (EducationAndExamsState)serializer.Deserialize(typeof(EducationAndExamsState), envelope.Payload);
        EducationAndExamsStateProjection.UpgradeFromSchemaV1ToV2(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.EducationAndExams,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(EducationAndExamsState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateSocialMemoryAndRelationsStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        SocialMemoryAndRelationsState migratedState = (SocialMemoryAndRelationsState)serializer.Deserialize(typeof(SocialMemoryAndRelationsState), envelope.Payload);
        SocialMemoryAndRelationsStateProjection.UpgradeFromSchemaV1ToV2(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.SocialMemoryAndRelations,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(SocialMemoryAndRelationsState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigratePopulationAndHouseholdsStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        PopulationAndHouseholdsState migratedState = (PopulationAndHouseholdsState)serializer.Deserialize(typeof(PopulationAndHouseholdsState), envelope.Payload);
        PopulationAndHouseholdsStateProjection.UpgradeFromSchemaV1ToV2(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.PopulationAndHouseholds,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(PopulationAndHouseholdsState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateWorldSettlementsStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WorldSettlementsState migratedState = (WorldSettlementsState)serializer.Deserialize(typeof(WorldSettlementsState), envelope.Payload);

        foreach (SettlementStateData settlement in migratedState.Settlements)
        {
            if (settlement.Tier == SettlementTier.Unknown)
            {
                settlement.Tier = SettlementTier.CountySeat;
            }
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WorldSettlements,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(WorldSettlementsState), migratedState),
        };
    }

    /// <summary>
    /// SPATIAL_SKELETON_SPEC §13 — Phase 1c v2→v3 upgrade:
    /// <list type="bullet">
    ///   <item>Seed <see cref="SettlementStateData.NodeKind"/> by inference from
    ///         <see cref="SettlementStateData.Tier"/> (SPEC §13.2 table).</item>
    ///   <item>Seed <see cref="SettlementStateData.Visibility"/> to
    ///         <see cref="NodeVisibility.StateVisible"/> (safe default: all
    ///         pre-1c nodes were officially registered).</item>
    ///   <item>Seed <see cref="SettlementStateData.EcoZone"/> to
    ///         <see cref="SettlementEcoZone.JiangnanWaterNetwork"/> (Lanxi
    ///         seed — the only live world in Phase 1c).</item>
    ///   <item>Leave <see cref="WorldSettlementsState.Routes"/> empty (SPEC
    ///         §13.3: old saves carry no routes; seed bootstrap rebuilds them).</item>
    ///   <item>Leave <see cref="WorldSettlementsState.CurrentSeason"/> at
    ///         constructor defaults (neutral Slack / Limited / Quiet).</item>
    /// </list>
    /// </summary>
    private static ModuleStateEnvelope MigrateWorldSettlementsStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WorldSettlementsState migratedState = (WorldSettlementsState)serializer.Deserialize(typeof(WorldSettlementsState), envelope.Payload);

        foreach (SettlementStateData settlement in migratedState.Settlements)
        {
            if (settlement.NodeKind == SettlementNodeKind.Unknown)
            {
                settlement.NodeKind = settlement.Tier switch
                {
                    SettlementTier.PrefectureSeat => SettlementNodeKind.PrefectureSeat,
                    SettlementTier.CountySeat => SettlementNodeKind.CountySeat,
                    SettlementTier.MarketTown => SettlementNodeKind.MarketTown,
                    _ => SettlementNodeKind.Village,
                };
            }

            if (settlement.Visibility == NodeVisibility.Unknown)
            {
                settlement.Visibility = NodeVisibility.StateVisible;
            }

            if (settlement.EcoZone == SettlementEcoZone.Unknown)
            {
                settlement.EcoZone = SettlementEcoZone.JiangnanWaterNetwork;
            }

            // NeighborIds and ParentAdministrativeId default to empty/null;
            // seed bootstrap (SPEC §12) rebuilds them when the Lanxi world
            // is re-seeded. Phase 1c does not force pre-existing saves to
            // adopt the seed graph.
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WorldSettlements,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(WorldSettlementsState), migratedState),
        };
    }

    /// <summary>
    /// STEP2A / A0a — v3 → v4：<see cref="SettlementStateData.HealerAccess"/>
    /// band 入场。旧档按 NodeKind 推断档位（skill simulation-calibration：band
    /// 语义各不相同，不是同质数字）。州府级名医、县镇坐堂医、渡口游方郎中、
    /// 村落祠堂私窝无医。不重写已经有值的档位。
    /// </summary>
    private static ModuleStateEnvelope MigrateWorldSettlementsStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WorldSettlementsState migratedState = (WorldSettlementsState)serializer.Deserialize(typeof(WorldSettlementsState), envelope.Payload);

        foreach (SettlementStateData settlement in migratedState.Settlements)
        {
            if (settlement.HealerAccess != HealerAccess.Unknown)
            {
                continue;
            }

            settlement.HealerAccess = settlement.NodeKind switch
            {
                SettlementNodeKind.PrefectureSeat => HealerAccess.Renowned,
                SettlementNodeKind.CountySeat => HealerAccess.Local,
                SettlementNodeKind.MarketTown => HealerAccess.Local,
                SettlementNodeKind.WalledTown => HealerAccess.Local,
                SettlementNodeKind.Ferry => HealerAccess.Itinerant,
                SettlementNodeKind.Wharf => HealerAccess.Itinerant,
                SettlementNodeKind.CanalJunction => HealerAccess.Itinerant,
                SettlementNodeKind.Temple => HealerAccess.Itinerant,
                SettlementNodeKind.ShrineCourt => HealerAccess.Itinerant,
                _ => HealerAccess.None,
            };
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WorldSettlements,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(WorldSettlementsState), migratedState),
        };
    }

    /// <summary>
    /// STEP2A / A0b — v4 → v5：<see cref="SettlementStateData.TempleHealingPresence"/>
    /// band 入场。寺观平行通道而非第二家医院（skill
    /// religion-temples-ritual-brokerage）。旧档按 NodeKind 推断档位；
    /// Temple 本身 Institutional，县镇 Lay 香火通道，村 / 渡口 Folk，
    /// 祠堂 / 仓 / 私窝 None。
    /// </summary>
    private static ModuleStateEnvelope MigrateWorldSettlementsStateV4ToV5(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WorldSettlementsState migratedState = (WorldSettlementsState)serializer.Deserialize(typeof(WorldSettlementsState), envelope.Payload);

        foreach (SettlementStateData settlement in migratedState.Settlements)
        {
            if (settlement.TempleHealingPresence != TempleHealingPresence.Unknown)
            {
                continue;
            }

            settlement.TempleHealingPresence = settlement.NodeKind switch
            {
                SettlementNodeKind.Temple => TempleHealingPresence.Institutional,
                SettlementNodeKind.ShrineCourt => TempleHealingPresence.Lay,
                SettlementNodeKind.HillShrine => TempleHealingPresence.Folk,
                SettlementNodeKind.PrefectureSeat => TempleHealingPresence.Lay,
                SettlementNodeKind.CountySeat => TempleHealingPresence.Lay,
                SettlementNodeKind.MarketTown => TempleHealingPresence.Lay,
                SettlementNodeKind.WalledTown => TempleHealingPresence.Lay,
                SettlementNodeKind.Village => TempleHealingPresence.Folk,
                SettlementNodeKind.EstateCluster => TempleHealingPresence.Folk,
                SettlementNodeKind.Ferry => TempleHealingPresence.Folk,
                SettlementNodeKind.Wharf => TempleHealingPresence.Folk,
                SettlementNodeKind.CanalJunction => TempleHealingPresence.Folk,
                _ => TempleHealingPresence.None,
            };
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WorldSettlements,
            ModuleSchemaVersion = 5,
            Payload = serializer.Serialize(typeof(WorldSettlementsState), migratedState),
        };
    }

    /// <summary>
    /// STEP2A / A0c — v5 → v6：<see cref="SettlementStateData.GranaryTrust"/>
    /// + <see cref="SettlementStateData.ReliefReach"/> 入场。赈济是政治
    /// （skill disaster-famine-relief-granaries）——旧档 Granary 给 55 信任 +
    /// Selective 实到，县治 45 + Stalled，市镇 40 + Stalled，其余 None。
    /// 不重写非零 GranaryTrust；ReliefReach 仅替换 Unknown。
    /// </summary>
    private static ModuleStateEnvelope MigrateWorldSettlementsStateV5ToV6(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WorldSettlementsState migratedState = (WorldSettlementsState)serializer.Deserialize(typeof(WorldSettlementsState), envelope.Payload);

        foreach (SettlementStateData settlement in migratedState.Settlements)
        {
            if (settlement.GranaryTrust <= 0)
            {
                settlement.GranaryTrust = settlement.NodeKind switch
                {
                    SettlementNodeKind.Granary => 55,
                    SettlementNodeKind.PrefectureSeat => 50,
                    SettlementNodeKind.CountySeat => 45,
                    SettlementNodeKind.MarketTown => 40,
                    SettlementNodeKind.WalledTown => 40,
                    SettlementNodeKind.Ferry => 35,
                    SettlementNodeKind.Wharf => 35,
                    SettlementNodeKind.CanalJunction => 38,
                    SettlementNodeKind.Temple => 30,
                    SettlementNodeKind.Village => 30,
                    SettlementNodeKind.EstateCluster => 28,
                    SettlementNodeKind.LineageHall => 25,
                    SettlementNodeKind.SmugglingCache => 10,
                    SettlementNodeKind.CovertMeetPoint => 10,
                    _ => 25,
                };
            }

            if (settlement.ReliefReach == ReliefReach.Unknown)
            {
                settlement.ReliefReach = settlement.NodeKind switch
                {
                    SettlementNodeKind.Granary => ReliefReach.Selective,
                    SettlementNodeKind.PrefectureSeat => ReliefReach.Selective,
                    SettlementNodeKind.CountySeat => ReliefReach.Stalled,
                    SettlementNodeKind.MarketTown => ReliefReach.Stalled,
                    SettlementNodeKind.WalledTown => ReliefReach.Stalled,
                    _ => ReliefReach.None,
                };
            }
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WorldSettlements,
            ModuleSchemaVersion = 6,
            Payload = serializer.Serialize(typeof(WorldSettlementsState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateFamilyCoreStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(typeof(FamilyCoreState), envelope.Payload);
        FamilyCoreStateProjection.UpgradeFromSchemaV1(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(FamilyCoreState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateFamilyCoreStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(typeof(FamilyCoreState), envelope.Payload);
        FamilyCoreStateProjection.UpgradeFromSchemaV2ToV3(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(FamilyCoreState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateFamilyCoreStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(typeof(FamilyCoreState), envelope.Payload);
        FamilyCoreStateProjection.UpgradeFromSchemaV3ToV4(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(FamilyCoreState), migratedState),
        };
    }

    /// <summary>
    /// STEP2A / A0a — v4 → v5：家内照料 + 郎中药铺链字段入场。旧档补默认值，
    /// 规则（A1 老死风险带）不在本步触发。
    /// </summary>
    private static ModuleStateEnvelope MigrateFamilyCoreStateV4ToV5(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(typeof(FamilyCoreState), envelope.Payload);
        FamilyCoreStateProjection.UpgradeFromSchemaV4ToV5(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 5,
            Payload = serializer.Serialize(typeof(FamilyCoreState), migratedState),
        };
    }

    /// <summary>
    /// STEP2A / A0d — v5 → v6：宗族救济挑选性字段入场。旧档补默认值，
    /// 规则（救一人 +Prestige / 弃一人 +BranchTension + ShameExclusion）
    /// 留给后续 step。
    /// </summary>
    private static ModuleStateEnvelope MigrateFamilyCoreStateV5ToV6(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(typeof(FamilyCoreState), envelope.Payload);
        FamilyCoreStateProjection.UpgradeFromSchemaV5ToV6(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 6,
            Payload = serializer.Serialize(typeof(FamilyCoreState), migratedState),
        };
    }

    /// <summary>
    /// STEP2A / A1 — v6 → v7：老死风险带 FragilityLedger 入场。旧档默认 0
    /// （健康重启），由后续月节拍累积。
    /// </summary>
    private static ModuleStateEnvelope MigrateFamilyCoreStateV6ToV7(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        FamilyCoreState migratedState = (FamilyCoreState)serializer.Deserialize(typeof(FamilyCoreState), envelope.Payload);
        FamilyCoreStateProjection.UpgradeFromSchemaV6ToV7(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.FamilyCore,
            ModuleSchemaVersion = 7,
            Payload = serializer.Serialize(typeof(FamilyCoreState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigratePublicLifeAndRumorStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        PublicLifeAndRumorState migratedState = (PublicLifeAndRumorState)serializer.Deserialize(typeof(PublicLifeAndRumorState), envelope.Payload);

        foreach (SettlementPublicLifeState settlement in migratedState.Settlements)
        {
            settlement.MonthlyCadenceCode ??= "legacy-cadence";
            settlement.MonthlyCadenceLabel ??= "旧档续脉";
            settlement.CrowdMixLabel ??= string.Empty;
            settlement.CadenceSummary ??= string.Empty;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.PublicLifeAndRumor,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(PublicLifeAndRumorState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigratePublicLifeAndRumorStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        PublicLifeAndRumorState migratedState = (PublicLifeAndRumorState)serializer.Deserialize(typeof(PublicLifeAndRumorState), envelope.Payload);

        foreach (SettlementPublicLifeState settlement in migratedState.Settlements)
        {
            settlement.DominantVenueCode ??= string.Empty;
            settlement.DocumentaryWeight = Math.Clamp(settlement.DocumentaryWeight, 0, 100);
            settlement.VerificationCost = Math.Clamp(settlement.VerificationCost, 0, 100);
            settlement.MarketRumorFlow = Math.Clamp(settlement.MarketRumorFlow, 0, 100);
            settlement.CourierRisk = Math.Clamp(settlement.CourierRisk, 0, 100);
            settlement.ChannelSummary ??= string.Empty;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.PublicLifeAndRumor,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(PublicLifeAndRumorState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigratePublicLifeAndRumorStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        PublicLifeAndRumorState migratedState = (PublicLifeAndRumorState)serializer.Deserialize(typeof(PublicLifeAndRumorState), envelope.Payload);

        foreach (SettlementPublicLifeState settlement in migratedState.Settlements)
        {
            settlement.OfficialNoticeLine ??= string.Empty;
            settlement.StreetTalkLine ??= string.Empty;
            settlement.RoadReportLine ??= string.Empty;
            settlement.PrefectureDispatchLine ??= string.Empty;
            settlement.ContentionSummary ??= string.Empty;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.PublicLifeAndRumor,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(PublicLifeAndRumorState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOfficeAndCareerStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OfficeAndCareerState migratedState = (OfficeAndCareerState)serializer.Deserialize(typeof(OfficeAndCareerState), envelope.Payload);
        OfficeAndCareerStateProjection.UpgradeFromSchemaV1(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(OfficeAndCareerState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOfficeAndCareerStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OfficeAndCareerState migratedState = (OfficeAndCareerState)serializer.Deserialize(typeof(OfficeAndCareerState), envelope.Payload);
        OfficeAndCareerStateProjection.UpgradeFromSchemaV2ToV3(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(OfficeAndCareerState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOfficeAndCareerStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OfficeAndCareerState migratedState = (OfficeAndCareerState)serializer.Deserialize(typeof(OfficeAndCareerState), envelope.Payload);
        OfficeAndCareerStateProjection.UpgradeFromSchemaV3ToV4(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OfficeAndCareer,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(OfficeAndCareerState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateTradeAndIndustryStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        TradeAndIndustryState migratedState = (TradeAndIndustryState)serializer.Deserialize(typeof(TradeAndIndustryState), envelope.Payload);

        migratedState.Clans ??= [];
        migratedState.Markets ??= [];
        migratedState.Routes ??= [];
        migratedState.BlackRouteLedgers ??= [];

        foreach (SettlementMarketState market in migratedState.Markets.OrderBy(static market => market.SettlementId.Value))
        {
            SettlementBlackRouteLedgerState ledger = migratedState.BlackRouteLedgers.SingleOrDefault(existing => existing.SettlementId == market.SettlementId)
                ?? new SettlementBlackRouteLedgerState
                {
                    SettlementId = market.SettlementId,
                    ShadowPriceIndex = 100,
                };

            if (!migratedState.BlackRouteLedgers.Contains(ledger))
            {
                migratedState.BlackRouteLedgers.Add(ledger);
            }

            int activeRouteCount = migratedState.Routes.Count(route => route.IsActive && route.SettlementId == market.SettlementId);
            int routeCapacity = migratedState.Routes
                .Where(route => route.IsActive && route.SettlementId == market.SettlementId)
                .Sum(static route => route.Capacity);

            ledger.ShadowPriceIndex = Math.Clamp(
                Math.Max(ledger.ShadowPriceIndex, 100)
                + ((market.PriceIndex - 100) / 2)
                + (market.LocalRisk / 5),
                70,
                180);
            ledger.DiversionShare = Math.Clamp(
                Math.Max(ledger.DiversionShare, (market.LocalRisk / 8) + (activeRouteCount * 4)),
                0,
                100);
            ledger.BlockedShipmentCount = Math.Clamp(
                Math.Max(ledger.BlockedShipmentCount, (market.LocalRisk >= 55 ? 1 : 0) + (activeRouteCount >= 2 ? 1 : 0)),
                0,
                12);
            ledger.SeizureRisk = Math.Clamp(
                Math.Max(ledger.SeizureRisk, (market.LocalRisk / 3) + (activeRouteCount * 2)),
                0,
                100);
            ledger.IllicitMargin = Math.Clamp(
                Math.Max(ledger.IllicitMargin, ((ledger.ShadowPriceIndex - 100) / 5) + (routeCapacity / 40) - ledger.BlockedShipmentCount),
                -10,
                30);
            ledger.DiversionBandLabel = string.IsNullOrWhiteSpace(ledger.DiversionBandLabel)
                ? DetermineMigratedDiversionBandLabel(ledger.DiversionShare, ledger.SeizureRisk, ledger.IllicitMargin)
                : ledger.DiversionBandLabel;
            ledger.LastLedgerTrace = string.IsNullOrWhiteSpace(ledger.LastLedgerTrace)
                ? $"{market.MarketName}的私下分流由旧档补出，先按市险与活路回填。"
                : ledger.LastLedgerTrace;
        }

        migratedState.BlackRouteLedgers = migratedState.BlackRouteLedgers
            .OrderBy(static ledger => ledger.SettlementId.Value)
            .ToList();

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.TradeAndIndustry,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(TradeAndIndustryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateTradeAndIndustryStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        TradeAndIndustryState migratedState = (TradeAndIndustryState)serializer.Deserialize(typeof(TradeAndIndustryState), envelope.Payload);

        foreach (RouteTradeState route in migratedState.Routes)
        {
            SettlementBlackRouteLedgerState? ledger = migratedState.BlackRouteLedgers
                .SingleOrDefault(existing => existing.SettlementId == route.SettlementId);
            int blockedShipmentCount = Math.Clamp(
                Math.Max(route.BlockedShipmentCount, ledger is null ? 0 : (ledger.BlockedShipmentCount > 0 ? 1 : 0) + (ledger.BlockedShipmentCount >= 3 ? 1 : 0)),
                0,
                6);
            int seizureRisk = Math.Clamp(
                Math.Max(route.SeizureRisk, (route.Risk / 5) + (ledger?.SeizureRisk ?? 0) / 2),
                0,
                100);

            route.BlockedShipmentCount = blockedShipmentCount;
            route.SeizureRisk = seizureRisk;
            route.RouteConstraintLabel = string.IsNullOrWhiteSpace(route.RouteConstraintLabel)
                ? DetermineMigratedRouteConstraintLabel(blockedShipmentCount, seizureRisk, route.Risk)
                : route.RouteConstraintLabel;
            route.LastRouteTrace = string.IsNullOrWhiteSpace(route.LastRouteTrace)
                ? $"{route.RouteName}旧档已按阻货与查缉势回填。"
                : route.LastRouteTrace;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.TradeAndIndustry,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(TradeAndIndustryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateTradeAndIndustryStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        TradeAndIndustryState migratedState = (TradeAndIndustryState)serializer.Deserialize(typeof(TradeAndIndustryState), envelope.Payload);
        TradeAndIndustryStateProjection.UpgradeFromSchemaV3ToV4(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.TradeAndIndustry,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(TradeAndIndustryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.BlackRoutePressure = Math.Clamp(
                Math.Max(settlement.BlackRoutePressure, (settlement.BanditThreat + settlement.RoutePressure + settlement.DisorderPressure) / 3),
                0,
                100);
            settlement.CoercionRisk = Math.Clamp(
                Math.Max(settlement.CoercionRisk, (settlement.BlackRoutePressure / 2) + (settlement.DisorderPressure / 3)),
                0,
                100);
            settlement.SuppressionRelief = Math.Clamp(settlement.SuppressionRelief, 0, 12);
            settlement.ResponseActivationLevel = Math.Clamp(settlement.ResponseActivationLevel, 0, 12);
            settlement.AdministrativeSuppressionWindow = Math.Clamp(settlement.AdministrativeSuppressionWindow, 0, 8);
            settlement.EscalationBandLabel = string.IsNullOrWhiteSpace(settlement.EscalationBandLabel)
                ? DetermineMigratedEscalationBandLabel(settlement.BlackRoutePressure, settlement.CoercionRisk)
                : settlement.EscalationBandLabel;
            settlement.LastPressureTrace = string.IsNullOrWhiteSpace(settlement.LastPressureTrace)
                ? (string.IsNullOrWhiteSpace(settlement.LastPressureReason)
                    ? "旧档私路压力已按地面不靖回填。"
                    : settlement.LastPressureReason)
                : settlement.LastPressureTrace;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.PaperCompliance = Math.Clamp(
                Math.Max(settlement.PaperCompliance, (settlement.SuppressionRelief * 18) + (settlement.AdministrativeSuppressionWindow * 12)),
                0,
                100);
            settlement.ImplementationDrag = Math.Clamp(
                Math.Max(settlement.ImplementationDrag, settlement.BlackRoutePressure - (settlement.SuppressionRelief * 6) - (settlement.AdministrativeSuppressionWindow * 8)),
                0,
                100);
            settlement.AdministrativeSuppressionWindow = Math.Clamp(settlement.AdministrativeSuppressionWindow, 0, 8);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.RouteShielding = Math.Clamp(
                Math.Max(
                    settlement.RouteShielding,
                    (settlement.ResponseActivationLevel * 8)
                    + (settlement.SuppressionRelief * 6)
                    - (settlement.RoutePressure / 4)),
                0,
                100);
            settlement.RetaliationRisk = Math.Clamp(
                Math.Max(
                    settlement.RetaliationRisk,
                    (settlement.CoercionRisk / 2)
                    + Math.Max(0, settlement.BlackRoutePressure - (settlement.RouteShielding / 2))
                    - (settlement.SuppressionRelief * 4)
                    - (settlement.AdministrativeSuppressionWindow * 5)),
                0,
                100);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV4ToV5(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.LastInterventionCommandCode ??= string.Empty;
            settlement.LastInterventionCommandLabel ??= string.Empty;
            settlement.LastInterventionSummary ??= string.Empty;
            settlement.LastInterventionOutcome ??= string.Empty;
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 5,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV5ToV6(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        migratedState.Settlements ??= [];
        foreach (SettlementDisorderState settlement in migratedState.Settlements)
        {
            settlement.InterventionCarryoverMonths = Math.Clamp(settlement.InterventionCarryoverMonths, 0, 1);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 6,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateOrderAndBanditryStateV6ToV7(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        OrderAndBanditryState migratedState = (OrderAndBanditryState)serializer.Deserialize(typeof(OrderAndBanditryState), envelope.Payload);

        OrderAndBanditryStateProjection.UpgradeFromSchemaV6ToV7(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.OrderAndBanditry,
            ModuleSchemaVersion = 7,
            Payload = serializer.Serialize(typeof(OrderAndBanditryState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateConflictAndForceStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState migratedState = (ConflictAndForceState)serializer.Deserialize(typeof(ConflictAndForceState), envelope.Payload);

        foreach (SettlementForceState migratedSettlement in migratedState.Settlements)
        {
            migratedSettlement.HasActiveConflict = ConflictAndForceResponseStateCalculator.InferLegacyHasActiveConflict(migratedSettlement.LastConflictTrace);
            ConflictAndForceResponseStateCalculator.Refresh(migratedSettlement);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.ConflictAndForce,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(ConflictAndForceState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateConflictAndForceStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState migratedState = (ConflictAndForceState)serializer.Deserialize(typeof(ConflictAndForceState), envelope.Payload);

        foreach (SettlementForceState migratedSettlement in migratedState.Settlements)
        {
            migratedSettlement.CampaignFatigue = Math.Clamp(migratedSettlement.CampaignFatigue, 0, 100);
            migratedSettlement.CampaignEscortStrain = Math.Clamp(migratedSettlement.CampaignEscortStrain, 0, 100);
            migratedSettlement.LastCampaignFalloutTrace ??= string.Empty;
            ConflictAndForceResponseStateCalculator.Refresh(migratedSettlement);
        }

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.ConflictAndForce,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(ConflictAndForceState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateConflictAndForceStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        ConflictAndForceState migratedState = (ConflictAndForceState)serializer.Deserialize(typeof(ConflictAndForceState), envelope.Payload);

        ConflictAndForceStateProjection.UpgradeFromSchemaV3ToV4(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.ConflictAndForce,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(ConflictAndForceState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateWarfareCampaignStateV1ToV2(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WarfareCampaignState migratedState = (WarfareCampaignState)serializer.Deserialize(typeof(WarfareCampaignState), envelope.Payload);
        WarfareCampaignStateProjection.UpgradeFromSchemaV1(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            ModuleSchemaVersion = 2,
            Payload = serializer.Serialize(typeof(WarfareCampaignState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateWarfareCampaignStateV2ToV3(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WarfareCampaignState migratedState = (WarfareCampaignState)serializer.Deserialize(typeof(WarfareCampaignState), envelope.Payload);
        WarfareCampaignStateProjection.UpgradeFromSchemaV2(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            ModuleSchemaVersion = 3,
            Payload = serializer.Serialize(typeof(WarfareCampaignState), migratedState),
        };
    }

    private static ModuleStateEnvelope MigrateWarfareCampaignStateV3ToV4(ModuleStateEnvelope envelope)
    {
        MessagePackModuleStateSerializer serializer = new();
        WarfareCampaignState migratedState = (WarfareCampaignState)serializer.Deserialize(typeof(WarfareCampaignState), envelope.Payload);
        WarfareCampaignStateProjection.UpgradeFromSchemaV3(migratedState);

        return new ModuleStateEnvelope
        {
            ModuleKey = KnownModuleKeys.WarfareCampaign,
            ModuleSchemaVersion = 4,
            Payload = serializer.Serialize(typeof(WarfareCampaignState), migratedState),
        };
    }
}
