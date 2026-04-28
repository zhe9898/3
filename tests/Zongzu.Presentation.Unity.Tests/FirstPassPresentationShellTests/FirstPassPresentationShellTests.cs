using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Presentation.Unity;

namespace Zongzu.Presentation.Unity.Tests;

[TestFixture]
public sealed partial class FirstPassPresentationShellTests
{
    [Test]
    public void Compose_ProjectsMonthlyPublicLifeCadenceIntoGreatHallAndDeskSandbox()
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(CreateBundle());

        Assert.That(shell.GreatHall.PublicLifeSummary, Does.Contain("春社集日"));
        Assert.That(shell.GreatHall.PublicLifeSummary, Does.Contain("客商"));
        Assert.That(shell.GreatHall.PublicLifeSummary, Does.Contain("说法相左").Or.Contain("榜文").Or.Contain("街谈"));
        Assert.That(shell.DeskSandbox.Settlements, Has.Count.EqualTo(1));
        Assert.That(shell.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("春社集日"));
        Assert.That(shell.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("街口茶肆"));
        Assert.That(shell.DeskSandbox.Settlements[0].PublicLifeSummary, Does.Contain("榜文").Or.Contain("街谈").Or.Contain("路报"));
    }

    [Test]
    public void Compose_CopiesMobilityAndFidelityReadbacksIntoGreatHallDeskAndLineage()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.PlayerCommands = bundle.PlayerCommands with
        {
            Affordances = bundle.PlayerCommands.Affordances
                .Concat(new[]
                {
                    new PlayerCommandAffordanceSnapshot
                    {
                        ModuleKey = KnownModuleKeys.PopulationAndHouseholds,
                        SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                        SettlementId = new SettlementId(1),
                        CommandName = PlayerCommandNames.RestrictNightTravel,
                        PersonnelFlowReadinessSummary = "人员流动预备读回：近处细读，远处汇总。",
                        IsEnabled = true,
                    },
                })
                .ToArray(),
            PersonnelFlowReadinessSummary = "人员流动命令预备汇总：只汇总已投影的人员流动预备读回；不是直接调人、迁人、召人命令。",
            PersonnelFlowOwnerLaneGateSummary = "人员流动归口门槛：当前可读归口为PopulationAndHouseholds本户回应；FamilyCore亲族调处、OfficeAndCareer文书役使、WarfareCampaign军务人力仍需另开owner-lane计划。",
            PersonnelFlowFutureOwnerLanePreflightSummary = "人员流动未来归口预检：FamilyCore/OfficeAndCareer/WarfareCampaign仍需另开owner-lane计划；不是直接调人、迁人、召人、派役、点兵命令。",
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.MobilitySummary, Does.Contain("Near detail"));
        Assert.That(shell.GreatHall.MobilitySummary, Does.Contain("PopulationAndHouseholds owns movement"));
        Assert.That(shell.GreatHall.MobilitySummary, Does.Contain("人员流动命令预备汇总"));
        Assert.That(shell.GreatHall.MobilitySummary, Does.Contain("不是直接调人、迁人、召人命令"));
        Assert.That(shell.GreatHall.MobilitySummary, Does.Contain("人员流动归口门槛"));
        Assert.That(shell.GreatHall.MobilitySummary, Does.Contain("另开owner-lane计划"));
        Assert.That(shell.GreatHall.MobilitySummary, Does.Contain("人员流动未来归口预检"));
        Assert.That(shell.GreatHall.MobilitySummary, Does.Contain("不是直接调人、迁人、召人、派役、点兵命令"));
        Assert.That(shell.DeskSandbox.Settlements, Has.Count.EqualTo(1));
        Assert.That(shell.DeskSandbox.Settlements[0].MobilitySummary, Does.Contain("Pool readback"));
        Assert.That(shell.DeskSandbox.Settlements[0].MobilitySummary, Does.Contain("not every regional traveler"));
        Assert.That(shell.DeskSandbox.Settlements[0].MobilitySummary, Does.Contain("人员流动归口门槛"));
        Assert.That(shell.DeskSandbox.Settlements[0].MobilitySummary, Does.Contain("当前可读归口为PopulationAndHouseholds本户回应"));
        Assert.That(shell.DeskSandbox.Settlements[0].MobilitySummary, Does.Not.Contain("人员流动未来归口预检"));
        Assert.That(shell.Lineage.PersonDossiers[0].MovementReadbackSummary, Does.Contain("PopulationAndHouseholds"));
        Assert.That(shell.Lineage.PersonDossiers[0].FidelityRingReadbackSummary, Does.Contain("PersonRegistry"));
    }

    [Test]
    public void Compose_DoesNotEchoPersonnelFlowGateToDeskSettlementWithoutLocalReadiness()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        SettlementId activeSettlementId = new(1);
        SettlementId quietSettlementId = new(2);
        bundle.Settlements = bundle.Settlements
            .Concat(new[]
            {
                new SettlementSnapshot
                {
                    Id = quietSettlementId,
                    Name = "Remote County",
                    Tier = SettlementTier.CountySeat,
                    Security = 51,
                    Prosperity = 43,
                },
            })
            .ToArray();
        bundle.SettlementMobilities = bundle.SettlementMobilities
            .Concat(new[]
            {
                new SettlementMobilitySnapshot
                {
                    SettlementId = quietSettlementId,
                    SettlementName = "Remote County",
                    AvailableLabor = 41,
                    LaborDemand = 52,
                    SeasonalSurplus = -11,
                    WageLevel = 55,
                    OutflowPressure = 31,
                    InflowPressure = 12,
                    FloatingPopulation = 8,
                    PoolThicknessSummary = "Remote pool readback stays pooled.",
                    FocusReadbackSummary = "Remote focus remains summarized.",
                    ScaleBudgetReadbackSummary = "Remote scale stays far-summary.",
                    SourceModuleKeys = [KnownModuleKeys.PopulationAndHouseholds],
                },
            })
            .ToArray();
        bundle.PlayerCommands = bundle.PlayerCommands with
        {
            Affordances = bundle.PlayerCommands.Affordances
                .Concat(new[]
                {
                    new PlayerCommandAffordanceSnapshot
                    {
                        ModuleKey = KnownModuleKeys.PopulationAndHouseholds,
                        SurfaceKey = PlayerCommandSurfaceKeys.PublicLife,
                        SettlementId = activeSettlementId,
                        CommandName = PlayerCommandNames.RestrictNightTravel,
                        PersonnelFlowReadinessSummary = "local readiness exists only here",
                        IsEnabled = true,
                    },
                })
                .ToArray(),
            PersonnelFlowOwnerLaneGateSummary = "personnel gate local marker: owner lanes remain separate.",
        };

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        SettlementNodeViewModel activeSettlement = shell.DeskSandbox.Settlements
            .Single(settlement => settlement.MobilitySummary.Contains("Pool readback"));
        SettlementNodeViewModel quietSettlement = shell.DeskSandbox.Settlements.Single(settlement => settlement.SettlementName == "Remote County");
        Assert.That(activeSettlement.MobilitySummary, Does.Contain("personnel gate local marker"));
        Assert.That(quietSettlement.MobilitySummary, Does.Contain("Remote pool readback stays pooled."));
        Assert.That(quietSettlement.MobilitySummary, Does.Not.Contain("personnel gate local marker"));
    }

    [Test]
    public void Compose_ProjectsGreatHallCountsDateHashAndCoreSummaries()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Notifications =
        [
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(1),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Urgent,
                Surface = NarrativeSurface.GreatHall,
                Title = "急报一",
                Summary = "急事一件。",
            },
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(2),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Consequential,
                Surface = NarrativeSurface.GreatHall,
                Title = "缓报二",
                Summary = "缓事一件。",
            },
            new NarrativeNotificationSnapshot
            {
                Id = new NotificationId(3),
                CreatedAt = new GameDate(1200, 2),
                Tier = NotificationTier.Background,
                Surface = NarrativeSurface.DeskSandbox,
                Title = "杂讯三",
                Summary = "杂讯一件。",
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.GreatHall.CurrentDateLabel, Is.EqualTo("1200-02"));
        Assert.That(shell.GreatHall.ReplayHash, Is.EqualTo("cadence-hash"));
        Assert.That(shell.GreatHall.UrgentCount, Is.EqualTo(1));
        Assert.That(shell.GreatHall.ConsequentialCount, Is.EqualTo(1));
        Assert.That(shell.GreatHall.BackgroundCount, Is.EqualTo(1));
        Assert.That(shell.GreatHall.EducationSummary, Does.Contain("塾馆在读0人"));
        Assert.That(shell.GreatHall.TradeSummary, Does.Contain("市账1册"));
        Assert.That(shell.GreatHall.TradeSummary, Does.Contain("得利1支"));
        Assert.That(shell.GreatHall.LeadNoticeTitle, Is.EqualTo("急报一"));
    }

    [Test]
    public void Compose_ProjectsWarfareAftermathFallbacksWhenNoCampaignAftermathExists()
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(CreateBundle());

        Assert.That(shell.GreatHall.AftermathDocketSummary, Does.Contain("尚无战后案牍"));
        Assert.That(shell.DeskSandbox.Settlements, Has.Count.EqualTo(1));
        Assert.That(shell.DeskSandbox.Settlements[0].AftermathSummary, Is.EqualTo("战后案牍未起。"));
        Assert.That(shell.Warfare.Summary, Does.Contain("暂无军务"));
    }

    [Test]
    public void Compose_ProjectsFamilyCouncilCommandsAndReceipts()
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(CreateBundle());

        Assert.That(shell.FamilyCouncil.Clans, Has.Count.EqualTo(1));
        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("承祧").Or.Contain("婚议").Or.Contain("举哀"));
        Assert.That(shell.GreatHall.FamilySummary, Does.Contain("宜先议定承祧"));
        Assert.That(shell.FamilyCouncil.Clans[0].LifecycleSummary, Does.Contain("议亲定婚").And.Contain("承祧"));
        Assert.That(shell.FamilyCouncil.Clans[0].LifecycleSummary, Does.Contain("眼下宜先议定承祧"));
        Assert.That(shell.FamilyCouncil.Clans[0].ClanName, Is.EqualTo("清河张氏"));
        Assert.That(shell.FamilyCouncil.CommandAffordances, Has.Count.EqualTo(2));
        Assert.That(shell.FamilyCouncil.CommandAffordances.Any(static command => command.CommandName == PlayerCommandNames.InviteClanEldersMediation), Is.True);
        Assert.That(shell.FamilyCouncil.CommandAffordances.Any(static command => command.CommandName == PlayerCommandNames.DesignateHeirPolicy), Is.True);
        Assert.That(shell.FamilyCouncil.RecentReceipts, Has.Count.EqualTo(1));
        Assert.That(shell.FamilyCouncil.RecentReceipts[0].OutcomeSummary, Does.Contain("族老"));
        Assert.That(shell.FamilyCouncil.Summary, Does.Contain("婚事").And.Contain("承祧"));
        Assert.That(shell.FamilyCouncil.Summary, Does.Contain("眼下最宜先命清河张氏议定承祧。"));
    }

    [Test]
    public void Compose_ProjectsLineageTilesForHeirAndNonHeirClans()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.Clans =
        [
            bundle.Clans[0],
            new ClanSnapshot
            {
                Id = new ClanId(2),
                ClanName = "B房李氏",
                HomeSettlementId = new SettlementId(1),
                Prestige = 41,
                SupportReserve = 27,
                BranchTension = 18,
                InheritancePressure = 22,
                SeparationPressure = 14,
                MediationMomentum = 11,
                MarriageAlliancePressure = 19,
                MarriageAllianceValue = 12,
                HeirSecurity = 9,
                ReproductivePressure = 23,
                MourningLoad = 0,
                LastLifecycleCommandLabel = "缓议婚帖",
                LastLifecycleOutcome = "暂缓一月再议。",
                LastConflictCommandLabel = string.Empty,
                LastConflictOutcome = string.Empty,
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(bundle);

        Assert.That(shell.Lineage.Clans, Has.Count.EqualTo(2));
        ClanTileViewModel withHeir = shell.Lineage.Clans.Single(clan => clan.ClanName == "清河张氏");
        ClanTileViewModel withoutHeir = shell.Lineage.Clans.Single(clan => clan.ClanName == "B房李氏");
        Assert.That(withHeir.StatusText, Is.EqualTo("承祧之人已入谱。"));
        Assert.That(withoutHeir.StatusText, Is.EqualTo("宗房暂未举出承祧人。"));
        Assert.That(withoutHeir.SupportReserve, Is.EqualTo(27));
    }

    [Test]
    public void Compose_ProjectsPersonDossiersIntoLineageSurface()
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(CreateBundle());

        Assert.That(shell.Lineage.PersonDossiers, Has.Count.EqualTo(1));
        PersonDossierViewModel dossier = shell.Lineage.PersonDossiers[0];
        Assert.That(dossier.PersonId, Is.EqualTo(1));
        Assert.That(dossier.DisplayName, Is.EqualTo("Zhang Yuan"));
        Assert.That(dossier.BranchPositionLabel, Is.EqualTo("Main-line heir"));
        Assert.That(dossier.MemoryPressureSummary, Does.Contain("pressure 38"));
        Assert.That(dossier.LivelihoodSummary, Does.Contain("PettyTrader"));
        Assert.That(dossier.MovementReadbackSummary, Does.Contain("PopulationAndHouseholds"));
        Assert.That(dossier.FidelityRingReadbackSummary, Does.Contain("PersonRegistry"));
        Assert.That(dossier.EducationSummary, Does.Contain("local exam passed"));
        Assert.That(dossier.OfficeSummary, Does.Contain("appointed"));
        Assert.That(dossier.SocialPositionLabel, Does.Contain("local-exam passer"));
        Assert.That(dossier.SocialPositionReadbackSummary, Does.Contain("社会位置读回"));
        Assert.That(dossier.SocialPositionReadbackSummary, Does.Contain("PersonRegistry只保身份/FidelityRing"));
        Assert.That(dossier.SocialPositionReadbackSummary, Does.Contain("不是升降阶级或zhuhu/kehu转换"));
        Assert.That(dossier.CurrentStatusSummary, Does.Contain("Living Adult"));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.PersonRegistry));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.FamilyCore));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.SocialMemoryAndRelations));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.PopulationAndHouseholds));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.EducationAndExams));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.TradeAndIndustry));
        Assert.That(dossier.SourceModuleKeys, Does.Contain(KnownModuleKeys.OfficeAndCareer));
        Assert.That(shell.Lineage.FocusedPerson, Is.Not.Null);
        Assert.That(shell.Lineage.FocusedPerson!.ObjectAnchorLabel, Is.EqualTo("画像卷轴"));
        Assert.That(shell.Lineage.FocusedPerson.TabletLabel, Does.Contain("Zhang Yuan"));
        Assert.That(shell.Lineage.FocusedPerson.PortraitScrollLine, Does.Contain("local-exam passer"));
        Assert.That(shell.Lineage.FocusedPerson.KinshipThreadLine, Is.EqualTo(dossier.KinshipSummary));
        Assert.That(shell.Lineage.FocusedPerson.LivelihoodThreadLine, Is.EqualTo(dossier.LivelihoodSummary));
        Assert.That(shell.Lineage.FocusedPerson.EducationThreadLine, Is.EqualTo(dossier.EducationSummary));
        Assert.That(shell.Lineage.FocusedPerson.OfficeThreadLine, Is.EqualTo(dossier.OfficeSummary));
        Assert.That(shell.Lineage.FocusedPerson.MemoryThreadLine, Is.EqualTo(dossier.MemoryPressureSummary));
        Assert.That(shell.Lineage.FocusedPerson.StatusLedgerLine, Does.Contain(dossier.SocialPositionReadbackSummary));
        Assert.That(shell.Lineage.FocusedPerson.Dossier.PersonId, Is.EqualTo(dossier.PersonId));
    }

    [Test]
    public void Compose_SelectionFocusesRequestedPersonDossier()
    {
        PresentationReadModelBundle bundle = CreateBundle();
        bundle.PersonDossiers =
        [
            bundle.PersonDossiers[0],
            new PersonDossierSnapshot
            {
                PersonId = new PersonId(2),
                DisplayName = "Li Wen",
                LifeStage = LifeStage.Adult,
                Gender = PersonGender.Male,
                IsAlive = true,
                FidelityRing = FidelityRing.Local,
                ClanId = new ClanId(1),
                ClanName = "娓呮渤寮犳皬",
                BranchPositionLabel = "Branch member",
                KinshipSummary = "children 0",
                TemperamentSummary = "ambition 42, prudence 61, loyalty 48, sociability 55",
                LivelihoodSummary = "No household livelihood projection.",
                EducationSummary = "No education projection.",
                OfficeSummary = "No office projection.",
                MemoryPressureSummary = "pressure 12; trust 8, hope 6",
                DormantMemorySummary = "No dormant social-memory stub.",
                SocialPositionLabel = "Branch member",
                SocialPositionReadbackSummary = "社会位置读回：FamilyCore亲族位置；PersonRegistry只保身份/FidelityRing；不是升降阶级或zhuhu/kehu转换。",
                CurrentStatusSummary = "Living Adult; Local ring; clan Qinghe Zhang; Branch member; pressure 12.",
                SourceModuleKeys = [KnownModuleKeys.PersonRegistry, KnownModuleKeys.FamilyCore],
            },
        ];

        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(
            bundle,
            new PresentationShellSelectionViewModel { FocusedPersonId = 2 });

        Assert.That(shell.Lineage.PersonDossiers, Has.Count.EqualTo(2));
        Assert.That(shell.Lineage.FocusedPerson, Is.Not.Null);
        Assert.That(shell.Lineage.FocusedPerson!.Dossier.PersonId, Is.EqualTo(2));
        Assert.That(shell.Lineage.FocusedPerson.TabletLabel, Does.Contain("Li Wen"));
        Assert.That(shell.Lineage.FocusedPerson.PortraitScrollLine, Does.Contain("Branch member"));
    }

    [Test]
    public void Compose_SelectionFallsBackWhenRequestedPersonIsMissing()
    {
        PresentationShellViewModel shell = FirstPassPresentationShell.Compose(
            CreateBundle(),
            new PresentationShellSelectionViewModel { FocusedPersonId = 999 });

        Assert.That(shell.Lineage.FocusedPerson, Is.Not.Null);
        Assert.That(shell.Lineage.FocusedPerson!.Dossier.PersonId, Is.EqualTo(1));
        Assert.That(shell.Lineage.FocusedPerson.Dossier.DisplayName, Is.EqualTo("Zhang Yuan"));
    }

}
