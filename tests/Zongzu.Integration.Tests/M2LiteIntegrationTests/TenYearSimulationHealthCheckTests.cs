using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Zongzu.Application;
using Zongzu.Contracts;
using Zongzu.Kernel;
using Zongzu.Modules.ConflictAndForce;
using Zongzu.Modules.FamilyCore;
using Zongzu.Modules.NarrativeProjection;
using Zongzu.Modules.OfficeAndCareer;
using Zongzu.Modules.OrderAndBanditry;
using Zongzu.Modules.PersonRegistry;
using Zongzu.Modules.PopulationAndHouseholds;
using Zongzu.Modules.SocialMemoryAndRelations;
using Zongzu.Modules.TradeAndIndustry;
using Zongzu.Modules.WarfareCampaign;
using Zongzu.Modules.WorldSettlements;
using Zongzu.Persistence;
using Zongzu.Scheduler;

namespace Zongzu.Integration.Tests;

[TestFixture]
public sealed class TenYearSimulationHealthCheckTests
{
    [Test]
    public void CampaignEnabledStressSandbox_TenYearHealthReport()
    {
        const int monthsToRun = 120;
        EventConsumptionProbe consumptionProbe = new();
        IReadOnlyList<IModuleRunner> diagnosticModules = WrapModules(
            SimulationBootstrapper.CreateP3CampaignSandboxModules(),
            consumptionProbe);
        GameSimulation simulation = GameSimulation.Load(CreateCampaignEnabledStressSave(), diagnosticModules);

        WorldSettlementsState initialWorld = simulation.GetModuleStateForTesting<WorldSettlementsState>(KnownModuleKeys.WorldSettlements);
        OrderAndBanditryState initialOrder = simulation.GetModuleStateForTesting<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        SettlementId[] watchedSettlements = ChooseWatchedSettlements(initialWorld, initialOrder);

        Dictionary<(string ModuleKey, string EventType), int> eventCounts = new();
        Dictionary<string, int> diffCounts = new(StringComparer.Ordinal);
        List<SettlementMonthSample> settlementSamples = new(monthsToRun * watchedSettlements.Length);

        for (int month = 1; month <= monthsToRun; month += 1)
        {
            SimulationMonthResult result = simulation.AdvanceOneMonth();
            foreach (IDomainEvent domainEvent in result.DomainEvents)
            {
                Increment(eventCounts, (domainEvent.ModuleKey, domainEvent.EventType));
            }

            foreach (WorldDiffEntry diffEntry in result.Diff.Entries)
            {
                Increment(diffCounts, diffEntry.ModuleKey);
            }

            CaptureSettlementSamples(simulation, watchedSettlements, month, result.NextDate, settlementSamples);
        }

        string report = BuildReport(
            simulation,
            diagnosticModules,
            consumptionProbe,
            watchedSettlements,
            settlementSamples,
            eventCounts,
            diffCounts);

        TestContext.Out.WriteLine(report);
        TestContext.Progress.WriteLine(report);

        Assert.That(settlementSamples, Has.Count.EqualTo(monthsToRun * watchedSettlements.Length));
        Assert.That(eventCounts.Values.Sum(), Is.GreaterThan(0));
        Assert.That(simulation.CurrentDate, Is.EqualTo(new GameDate(1210, 1)));
    }

    [Test]
    public void EventContractHealth_CurrentDiagnosticDebtHasExplicitClassification()
    {
        string[] knownDebt = KnownEmittedWithoutAuthorityConsumers
            .Concat(KnownDeclaredButNotEmitted)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        string[] missing = knownDebt
            .Where(static eventKey => !EventContractHealthClassifications.ContainsKey(eventKey))
            .ToArray();

        Assert.That(missing, Is.Empty, "Missing event contract classifications: " + string.Join(", ", missing));
        Assert.That(
            EventContractHealthClassifications.Values.Select(static classification => classification.Kind),
            Does.Contain(EventContractHealthKind.ProjectionOnlyReceipt));
        Assert.That(
            EventContractHealthClassifications.Values.Select(static classification => classification.Kind),
            Does.Contain(EventContractHealthKind.FutureContract));
        Assert.That(
            EventContractHealthClassifications.Values.Select(static classification => classification.Kind),
            Does.Contain(EventContractHealthKind.DormantSeededPath));
        Assert.That(
            EventContractHealthClassifications.Values.Select(static classification => classification.Kind),
            Does.Contain(EventContractHealthKind.AcceptanceTestGap));
    }

    [Test]
    public void EventContractHealth_DiagnosticEventKeysAvoidDuplicateModulePrefixes()
    {
        Assert.That(
            FormatEventContractKey(KnownModuleKeys.OfficeAndCareer, "OfficeAndCareer.AmnestyApplied"),
            Is.EqualTo("OfficeAndCareer.AmnestyApplied"));
        Assert.That(
            FormatEventContractKey(KnownModuleKeys.WorldSettlements, "WorldSettlements.CanalWindowChanged"),
            Is.EqualTo("WorldSettlements.CanalWindowChanged"));
        Assert.That(
            FormatEventContractKey(KnownModuleKeys.FamilyCore, "ClanPrestigeAdjusted"),
            Is.EqualTo("FamilyCore.ClanPrestigeAdjusted"));
    }

    private static SaveRoot CreateCampaignEnabledStressSave()
    {
        GameSimulation simulation = SimulationBootstrapper.CreateP3CampaignSandboxBootstrap(20260421);
        MethodInfo? method = typeof(SimulationBootstrapper).GetMethod(
            "SeedM3StressWorld",
            BindingFlags.NonPublic | BindingFlags.Static);

        if (method is null)
        {
            throw new InvalidOperationException("Could not locate stress seeder for the diagnostic sandbox.");
        }

        method.Invoke(null, new object[] { simulation, true });
        return simulation.ExportSave();
    }

    private static IReadOnlyList<IModuleRunner> WrapModules(IReadOnlyList<IModuleRunner> modules, EventConsumptionProbe probe)
    {
        return modules.Select(module => new DiagnosticModuleRunner(module, probe)).ToArray();
    }

    private static SettlementId[] ChooseWatchedSettlements(WorldSettlementsState world, OrderAndBanditryState order)
    {
        Dictionary<SettlementId, SettlementDisorderState> disorderBySettlement = order.Settlements
            .ToDictionary(static settlement => settlement.SettlementId);

        SettlementStateData stable = world.Settlements
            .OrderByDescending(settlement => settlement.Security + settlement.Prosperity)
            .ThenBy(static settlement => settlement.Id.Value)
            .First();

        SettlementStateData pressured = world.Settlements
            .OrderByDescending(settlement =>
            {
                disorderBySettlement.TryGetValue(settlement.Id, out SettlementDisorderState? disorder);
                int banditPressure = disorder?.BanditThreat ?? 0;
                int routePressure = disorder?.RoutePressure ?? 0;
                return (100 - settlement.Security) + banditPressure + routePressure;
            })
            .ThenBy(static settlement => settlement.Id.Value)
            .First();

        if (stable.Id == pressured.Id)
        {
            pressured = world.Settlements
                .Where(settlement => settlement.Id != stable.Id)
                .OrderBy(settlement => settlement.Security + settlement.Prosperity)
                .ThenBy(static settlement => settlement.Id.Value)
                .First();
        }

        return [stable.Id, pressured.Id];
    }

    private static void CaptureSettlementSamples(
        GameSimulation simulation,
        IReadOnlyList<SettlementId> watchedSettlements,
        int month,
        GameDate date,
        List<SettlementMonthSample> samples)
    {
        WorldSettlementsState world = simulation.GetModuleStateForTesting<WorldSettlementsState>(KnownModuleKeys.WorldSettlements);
        PopulationAndHouseholdsState population = simulation.GetModuleStateForTesting<PopulationAndHouseholdsState>(KnownModuleKeys.PopulationAndHouseholds);
        OrderAndBanditryState order = simulation.GetModuleStateForTesting<OrderAndBanditryState>(KnownModuleKeys.OrderAndBanditry);
        ConflictAndForceState conflict = simulation.GetModuleStateForTesting<ConflictAndForceState>(KnownModuleKeys.ConflictAndForce);
        WarfareCampaignState warfare = simulation.GetModuleStateForTesting<WarfareCampaignState>(KnownModuleKeys.WarfareCampaign);

        foreach (SettlementId settlementId in watchedSettlements)
        {
            SettlementStateData settlement = world.Settlements.Single(entry => entry.Id == settlementId);
            PopulationSettlementState? populationSettlement = population.Settlements.SingleOrDefault(entry => entry.SettlementId == settlementId);
            SettlementDisorderState? disorder = order.Settlements.SingleOrDefault(entry => entry.SettlementId == settlementId);
            SettlementForceState? force = conflict.Settlements.SingleOrDefault(entry => entry.SettlementId == settlementId);
            CampaignFrontState? campaign = warfare.Campaigns.SingleOrDefault(entry => entry.AnchorSettlementId == settlementId);

            samples.Add(new SettlementMonthSample(
                month,
                date,
                settlement.Id,
                settlement.Name,
                settlement.Security,
                settlement.Prosperity,
                populationSettlement?.CommonerDistress,
                populationSettlement?.MigrationPressure,
                disorder?.BanditThreat,
                disorder?.RoutePressure,
                disorder?.DisorderPressure,
                disorder?.SuppressionDemand,
                force?.Readiness,
                force?.CommandCapacity,
                campaign?.SupplyState,
                campaign?.FrontPressure,
                campaign?.MoraleState,
                campaign?.IsActive));
        }
    }

    private static string BuildReport(
        GameSimulation simulation,
        IReadOnlyList<IModuleRunner> modules,
        EventConsumptionProbe consumptionProbe,
        IReadOnlyList<SettlementId> watchedSettlements,
        IReadOnlyList<SettlementMonthSample> settlementSamples,
        IReadOnlyDictionary<(string ModuleKey, string EventType), int> eventCounts,
        IReadOnlyDictionary<string, int> diffCounts)
    {
        StringBuilder builder = new();
        builder.AppendLine("《10年骨骼运行体检报告》原始诊断输出");
        builder.AppendLine($"Seed=20260421; Months=120; FinalDate={simulation.CurrentDate.Year}-{simulation.CurrentDate.Month:D2}; ReplayHash={simulation.ReplayHash}");
        builder.AppendLine();
        AppendModuleSurface(builder, modules, eventCounts, consumptionProbe);
        builder.AppendLine();
        AppendDiffSurface(builder, diffCounts);
        builder.AppendLine();
        AppendSettlementTrendSurface(builder, watchedSettlements, settlementSamples);
        builder.AppendLine();
        AppendPersonAndMemorySurface(builder, simulation);
        return builder.ToString();
    }

    private static void AppendModuleSurface(
        StringBuilder builder,
        IReadOnlyList<IModuleRunner> modules,
        IReadOnlyDictionary<(string ModuleKey, string EventType), int> eventCounts,
        EventConsumptionProbe consumptionProbe)
    {
        builder.AppendLine("一、跨模块事件流");
        builder.AppendLine("实际发布事件 Top:");
        foreach (var group in eventCounts
                     .OrderByDescending(static pair => pair.Value)
                     .ThenBy(static pair => pair.Key.ModuleKey, StringComparer.Ordinal)
                     .ThenBy(static pair => pair.Key.EventType, StringComparer.Ordinal)
                     .Take(32))
        {
            int handled = consumptionProbe.CountHandled(group.Key.ModuleKey, group.Key.EventType);
            string consumers = string.Join(
                ",",
                consumptionProbe.GetConsumers(group.Key.ModuleKey, group.Key.EventType).OrderBy(static value => value, StringComparer.Ordinal));
            string eventKey = FormatEventContractKey(group.Key.ModuleKey, group.Key.EventType);
            string contractNote = handled > 0
                ? "contract=AuthorityConsumed, note=declared consumer handled the event through the module seam"
                : FormatEventContractHealthNote(eventKey);
            builder.AppendLine(
                $"- {eventKey}: emitted={group.Value}, authorityConsumed={handled}, consumers=[{consumers}], {contractNote}");
        }

        HashSet<string> emittedEventKeys = eventCounts.Keys
            .Select(static key => FormatEventContractKey(key.ModuleKey, key.EventType))
            .ToHashSet(StringComparer.Ordinal);
        List<string> noAuthorityConsumer = eventCounts
            .Where(pair => consumptionProbe.CountHandled(pair.Key.ModuleKey, pair.Key.EventType) == 0)
            .OrderByDescending(static pair => pair.Value)
            .Select(static pair => FormatClassifiedEventDebt(
                FormatEventContractKey(pair.Key.ModuleKey, pair.Key.EventType),
                pair.Value))
            .Take(12)
            .ToList();
        List<string> declaredButNeverEmitted = modules
            .SelectMany(module => module.PublishedEvents.Select(eventType => FormatEventContractKey(module.ModuleKey, eventType)))
            .Where(eventKey => !emittedEventKeys.Contains(eventKey))
            .OrderBy(static value => value, StringComparer.Ordinal)
            .Select(static eventKey => FormatClassifiedEventDebt(eventKey, null))
            .Take(24)
            .ToList();

        builder.AppendLine("无人权威处理的已发布事件:");
        builder.AppendLine(noAuthorityConsumer.Count == 0 ? "- none" : "- " + string.Join("; ", noAuthorityConsumer));
        builder.AppendLine("声明可发布但120个月从未出现的事件:");
        builder.AppendLine(declaredButNeverEmitted.Count == 0 ? "- none" : "- " + string.Join("; ", declaredButNeverEmitted));
    }

    private static string FormatEventContractKey(string moduleKey, string eventType)
    {
        string modulePrefix = moduleKey + ".";
        return eventType.StartsWith(modulePrefix, StringComparison.Ordinal)
            ? eventType
            : modulePrefix + eventType;
    }

    private static string FormatClassifiedEventDebt(string eventKey, int? emittedCount)
    {
        string count = emittedCount.HasValue ? $" x{emittedCount.Value}" : string.Empty;
        EventContractHealthClassification classification = ClassifyEventContract(eventKey);
        return $"{eventKey}{count} [{classification.Kind}: {classification.Rationale}]";
    }

    private static string FormatEventContractHealthNote(string eventKey)
    {
        EventContractHealthClassification classification = ClassifyEventContract(eventKey);
        return $"contract={classification.Kind}, note={classification.Rationale}";
    }

    private static EventContractHealthClassification ClassifyEventContract(string eventKey)
    {
        return EventContractHealthClassifications.TryGetValue(eventKey, out EventContractHealthClassification? classification)
            ? classification
            : UnclassifiedEventContract;
    }

    private static void AppendDiffSurface(StringBuilder builder, IReadOnlyDictionary<string, int> diffCounts)
    {
        builder.AppendLine("二、结构化 diff 输出");
        foreach (KeyValuePair<string, int> diff in diffCounts.OrderByDescending(static pair => pair.Value))
        {
            builder.AppendLine($"- {diff.Key}: diffEntries={diff.Value}");
        }
    }

    private static void AppendSettlementTrendSurface(
        StringBuilder builder,
        IReadOnlyList<SettlementId> watchedSettlements,
        IReadOnlyList<SettlementMonthSample> samples)
    {
        builder.AppendLine("三、聚落十年曲线");
        foreach (SettlementId settlementId in watchedSettlements)
        {
            List<SettlementMonthSample> settlementSamples = samples
                .Where(sample => sample.SettlementId == settlementId)
                .OrderBy(static sample => sample.Month)
                .ToList();
            SettlementMonthSample first = settlementSamples[0];
            builder.AppendLine($"聚落 {first.SettlementName}({first.SettlementId.Value})");
            AppendTrend(builder, "Security", settlementSamples.Select(static sample => (int?)sample.Security));
            AppendTrend(builder, "Prosperity", settlementSamples.Select(static sample => (int?)sample.Prosperity));
            AppendTrend(builder, "CommonerDistress", settlementSamples.Select(static sample => sample.CommonerDistress));
            AppendTrend(builder, "MigrationPressure", settlementSamples.Select(static sample => sample.MigrationPressure));
            AppendTrend(builder, "BanditThreat", settlementSamples.Select(static sample => sample.BanditThreat));
            AppendTrend(builder, "RoutePressure", settlementSamples.Select(static sample => sample.RoutePressure));
            AppendTrend(builder, "DisorderPressure", settlementSamples.Select(static sample => sample.DisorderPressure));
            AppendTrend(builder, "SuppressionDemand", settlementSamples.Select(static sample => sample.SuppressionDemand));
            AppendTrend(builder, "ForceReadiness", settlementSamples.Select(static sample => sample.ForceReadiness));
            AppendTrend(builder, "CommandCapacity", settlementSamples.Select(static sample => sample.CommandCapacity));
            AppendTrend(builder, "CampaignSupplyState", settlementSamples.Select(static sample => sample.CampaignSupplyState));
            AppendTrend(builder, "CampaignFrontPressure", settlementSamples.Select(static sample => sample.CampaignFrontPressure));
            AppendTrend(builder, "CampaignMoraleState", settlementSamples.Select(static sample => sample.CampaignMoraleState));
            builder.AppendLine("  年度断面:");
            foreach (SettlementMonthSample sample in settlementSamples.Where(static sample => sample.Month % 12 == 0))
            {
                builder.AppendLine(
                    $"  - M{sample.Month:D3}: Sec={sample.Security}, Pros={sample.Prosperity}, Supply={Format(sample.CampaignSupplyState)}, Bandit={Format(sample.BanditThreat)}, Route={Format(sample.RoutePressure)}, Distress={Format(sample.CommonerDistress)}, ActiveCampaign={sample.CampaignIsActive}");
            }
        }
    }

    private static void AppendTrend(StringBuilder builder, string name, IEnumerable<int?> values)
    {
        int[] concrete = values.Where(static value => value.HasValue).Select(static value => value!.Value).ToArray();
        if (concrete.Length == 0)
        {
            builder.AppendLine($"  - {name}: no data");
            return;
        }

        int first = concrete[0];
        int last = concrete[^1];
        int min = concrete.Min();
        int max = concrete.Max();
        int unique = concrete.Distinct().Count();
        int signChanges = CountSignChanges(concrete);
        builder.AppendLine($"  - {name}: first={first}, last={last}, delta={last - first}, min={min}, max={max}, unique={unique}, signChanges={signChanges}");
    }

    private static int CountSignChanges(IReadOnlyList<int> values)
    {
        int previousSign = 0;
        int signChanges = 0;
        for (int index = 1; index < values.Count; index += 1)
        {
            int delta = values[index] - values[index - 1];
            int currentSign = Math.Sign(delta);
            if (currentSign == 0)
            {
                continue;
            }

            if (previousSign != 0 && previousSign != currentSign)
            {
                signChanges += 1;
            }

            previousSign = currentSign;
        }

        return signChanges;
    }

    private static void AppendPersonAndMemorySurface(StringBuilder builder, GameSimulation simulation)
    {
        PersonRegistryState personRegistry = simulation.GetModuleStateForTesting<PersonRegistryState>(KnownModuleKeys.PersonRegistry);
        SocialMemoryAndRelationsState social = simulation.GetModuleStateForTesting<SocialMemoryAndRelationsState>(KnownModuleKeys.SocialMemoryAndRelations);
        FamilyCoreState family = simulation.GetModuleStateForTesting<FamilyCoreState>(KnownModuleKeys.FamilyCore);

        builder.AppendLine("四、十年后 PersonRegistry / SocialMemoryAndRelations / FamilyCore");
        builder.AppendLine($"PersonRegistry: total={personRegistry.Persons.Count}, living={personRegistry.Persons.Count(static person => person.IsAlive)}, deceased={personRegistry.Persons.Count(static person => !person.IsAlive)}");
        foreach (var group in personRegistry.Persons.GroupBy(static person => person.LifeStage).OrderBy(static group => group.Key))
        {
            builder.AppendLine($"- LifeStage.{group.Key}: {group.Count()}");
        }

        foreach (PersonRecord person in personRegistry.Persons.OrderBy(static person => person.Id.Value))
        {
            builder.AppendLine($"  Person {person.Id.Value}: {person.DisplayName}, alive={person.IsAlive}, stage={person.LifeStage}, ring={person.FidelityRing}");
        }

        builder.AppendLine($"SocialMemory: clanNarratives={social.ClanNarratives.Count}, memories={social.Memories.Count}, dormantStubs={social.DormantStubs.Count}");
        foreach (ClanNarrativeState narrative in social.ClanNarratives.OrderBy(static narrative => narrative.ClanId.Value))
        {
            builder.AppendLine($"  Clan {narrative.ClanId.Value}: grudge={narrative.GrudgePressure}, fear={narrative.FearPressure}, shame={narrative.ShamePressure}, favor={narrative.FavorBalance}, text='{narrative.PublicNarrative}'");
        }

        foreach (MemoryRecordState memory in social.Memories.OrderBy(static memory => memory.Id.Value).Take(20))
        {
            builder.AppendLine($"  Memory {memory.Id.Value}: clan={memory.SubjectClanId.Value}, kind={memory.Kind}, type={memory.Type}/{memory.Subtype}, intensity={memory.Intensity}, weight={memory.Weight}, cause={memory.CauseKey}, summary='{memory.Summary}'");
        }

        builder.AppendLine($"FamilyCore: clans={family.Clans.Count}, people={family.People.Count}, deadFamilyPeople={family.People.Count(static person => !person.IsAlive)}");
        foreach (ClanStateData clan in family.Clans.OrderBy(static clan => clan.Id.Value))
        {
            builder.AppendLine(
                $"  Clan {clan.Id.Value}: prestige={clan.Prestige}, support={clan.SupportReserve}, branchTension={clan.BranchTension}, inheritance={clan.InheritancePressure}, marriage={clan.MarriageAlliancePressure}, heirSecurity={clan.HeirSecurity}, mourning={clan.MourningLoad}");
        }
    }

    private static string Format(int? value)
    {
        return value.HasValue ? value.Value.ToString() : "NA";
    }

    private static void Increment<TKey>(Dictionary<TKey, int> counts, TKey key)
        where TKey : notnull
    {
        counts.TryGetValue(key, out int current);
        counts[key] = current + 1;
    }

    private static readonly string[] KnownEmittedWithoutAuthorityConsumers =
    [
        "FamilyCore.ClanPrestigeAdjusted",
        "SocialMemoryAndRelations.ClanNarrativeUpdated",
        "ConflictAndForce.ConflictResolved",
        "ConflictAndForce.CommanderWounded",
        "FamilyCore.FamilyMembersAged",
        "WorldSettlements.SeasonalFestivalArrived",
        "WorldSettlements.CanalWindowChanged",
        "ConflictAndForce.ForceReadinessChanged",
        "PersonRegistry.PersonDeceased",
        "WorldSettlements.SettlementPressureChanged",
        "PopulationAndHouseholds.MigrationStarted",
        "SocialMemoryAndRelations.EmotionalPressureShifted",
        "SocialMemoryAndRelations.PressureTempered",
        "OfficeAndCareer.OfficeGranted",
        "OfficeAndCareer.OfficeLost",
        "FamilyCore.BirthRegistered",
        "PersonRegistry.PersonCreated",
        "PopulationAndHouseholds.LivelihoodCollapsed"
    ];

    private static readonly string[] KnownDeclaredButNotEmitted =
    [
        "ConflictAndForce.DeathByViolence",
        "ConflictAndForce.MilitiaMobilized",
        "EducationAndExams.StudyAbandoned",
        "EducationAndExams.TutorSecured",
        "FamilyCore.BranchSeparationApproved",
        "FamilyCore.HeirAppointed",
        "FamilyCore.HeirSuccessionOccurred",
        "FamilyCore.LineageMediationOpened",
        "FamilyCore.MarriageAllianceArranged",
        "OfficeAndCareer.AmnestyApplied",
        "OfficeAndCareer.OfficeDefected",
        "OfficeAndCareer.PolicyWindowOpened",
        "OfficeAndCareer.YamenOverloaded",
        "OrderAndBanditry.DisorderSpike",
        "OrderAndBanditry.SuppressionSucceeded",
        "PersonRegistry.FidelityRingChanged",
        "PopulationAndHouseholds.DeathByIllness",
        "PopulationAndHouseholds.HouseholdBurdenIncreased",
        "PopulationAndHouseholds.HouseholdSubsistencePressureChanged",
        "PublicLifeAndRumor.StreetTalkSurged",
        "SocialMemoryAndRelations.FavorIncurred",
        "SocialMemoryAndRelations.GrudgeSoftened",
        "TradeAndIndustry.GrainPriceSpike",
        "WarfareCampaign.CampaignSupplyStrained",
        "WorldSettlements.ComplianceModeShifted"
    ];

    private static readonly EventContractHealthClassification UnclassifiedEventContract = new(
        EventContractHealthKind.Unclassified,
        "new diagnostic debt; classify before using it as evidence");

    private static readonly IReadOnlyDictionary<string, EventContractHealthClassification> EventContractHealthClassifications =
        new Dictionary<string, EventContractHealthClassification>(StringComparer.Ordinal)
        {
            ["ConflictAndForce.CommanderWounded"] = new(
                EventContractHealthKind.FutureContract,
                "force injury fact for later family/office/memory ownership; no authority consumer in this slice"),
            ["ConflictAndForce.ConflictResolved"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "conflict outcome is already owned by ConflictAndForce and read by projections/diagnostics"),
            ["ConflictAndForce.DeathByViolence"] = new(
                EventContractHealthKind.DormantSeededPath,
                "violence death contract is declared but this stress seed does not force that path"),
            ["ConflictAndForce.ForceReadinessChanged"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "readiness change is a local force receipt unless a later owner module subscribes"),
            ["ConflictAndForce.MilitiaMobilized"] = new(
                EventContractHealthKind.FutureContract,
                "mobilization handoff is reserved for fuller militia/campaign integration"),
            ["EducationAndExams.StudyAbandoned"] = new(
                EventContractHealthKind.DormantSeededPath,
                "study-abandon branch depends on education stress not guaranteed by the ten-year seed"),
            ["EducationAndExams.TutorSecured"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "covered by focused education behavior, not guaranteed in the stress health fixture"),
            ["FamilyCore.BranchSeparationApproved"] = new(
                EventContractHealthKind.DormantSeededPath,
                "branch split needs a family dispute threshold not forced by this seed"),
            ["FamilyCore.BirthRegistered"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "birth registration is family-owned lifecycle evidence after state mutation"),
            ["FamilyCore.ClanPrestigeAdjusted"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "family prestige is already mutated by FamilyCore and read outward as a receipt"),
            ["FamilyCore.FamilyMembersAged"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "monthly aging is a FamilyCore lifecycle receipt, not an external command trigger"),
            ["FamilyCore.HeirAppointed"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "heir appointment is covered by focused lifecycle tests but may not occur in the stress seed"),
            ["FamilyCore.HeirSuccessionOccurred"] = new(
                EventContractHealthKind.DormantSeededPath,
                "succession needs a death/succession combination not guaranteed by this seed"),
            ["FamilyCore.LineageMediationOpened"] = new(
                EventContractHealthKind.DormantSeededPath,
                "lineage mediation depends on a hardened dispute threshold"),
            ["FamilyCore.MarriageAllianceArranged"] = new(
                EventContractHealthKind.DormantSeededPath,
                "marriage alliance depends on eligible lifecycle/social conditions"),
            ["OfficeAndCareer.AmnestyApplied"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "amnesty chain is proven by focused pressure-chain tests rather than this stress seed"),
            ["OfficeAndCareer.OfficeDefected"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "regime defection chain is proven by focused pressure-chain tests rather than this stress seed"),
            ["OfficeAndCareer.OfficeGranted"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "office grant is office-owned appointment evidence unless a later owner subscribes"),
            ["OfficeAndCareer.OfficeLost"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "office loss is office-owned appointment evidence unless a later owner subscribes"),
            ["OfficeAndCareer.PolicyWindowOpened"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "policy-window chain is proven by focused pressure-chain tests rather than this stress seed"),
            ["OfficeAndCareer.YamenOverloaded"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "tax/yamen/public-life chain is proven by focused pressure-chain tests rather than this stress seed"),
            ["OrderAndBanditry.DisorderSpike"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "disorder spike is proven by focused order/public-life pressure-chain tests rather than this stress seed"),
            ["OrderAndBanditry.SuppressionSucceeded"] = new(
                EventContractHealthKind.DormantSeededPath,
                "suppression success requires response conditions not forced by this stress seed"),
            ["PersonRegistry.FidelityRingChanged"] = new(
                EventContractHealthKind.FutureContract,
                "identity topology receipt is reserved for later cross-person relationship presentation"),
            ["PersonRegistry.PersonDeceased"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "registry death fact is terminal identity bookkeeping after cause owners resolve"),
            ["PersonRegistry.PersonCreated"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "registry birth/person creation fact is terminal identity bookkeeping after family resolution"),
            ["PopulationAndHouseholds.DeathByIllness"] = new(
                EventContractHealthKind.DormantSeededPath,
                "population illness death contract depends on mortality pressure not forced by this seed"),
            ["PopulationAndHouseholds.HouseholdBurdenIncreased"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "frontier supply burden chain is proven by focused pressure-chain tests rather than this stress seed"),
            ["PopulationAndHouseholds.HouseholdSubsistencePressureChanged"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "harvest/grain subsistence chain is proven by focused pressure-chain tests rather than this stress seed"),
            ["PopulationAndHouseholds.LivelihoodCollapsed"] = new(
                EventContractHealthKind.FutureContract,
                "livelihood collapse is population-owned evidence for later office/memory/trade follow-on contracts"),
            ["PopulationAndHouseholds.MigrationStarted"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "migration trace is population-owned and player-facing unless a later owner subscribes"),
            ["PublicLifeAndRumor.StreetTalkSurged"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "public-life surge is covered by focused pressure-chain tests rather than required in this seed"),
            ["SocialMemoryAndRelations.ClanNarrativeUpdated"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "memory narrative update is the durable residue receipt, not a command trigger"),
            ["SocialMemoryAndRelations.EmotionalPressureShifted"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "emotional pressure shift is SocialMemory-owned residue after structured causes"),
            ["SocialMemoryAndRelations.FavorIncurred"] = new(
                EventContractHealthKind.DormantSeededPath,
                "favor threshold depends on later social-pressure accumulation in the seed"),
            ["SocialMemoryAndRelations.GrudgeSoftened"] = new(
                EventContractHealthKind.DormantSeededPath,
                "grudge-softening threshold depends on later social-pressure relief"),
            ["SocialMemoryAndRelations.PressureTempered"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "pressure tempering is SocialMemory-owned residue after structured causes"),
            ["TradeAndIndustry.GrainPriceSpike"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "harvest/grain market chain is proven by focused pressure-chain tests rather than this stress seed"),
            ["WarfareCampaign.CampaignSupplyStrained"] = new(
                EventContractHealthKind.AcceptanceTestGap,
                "campaign supply strain is covered by focused warfare/campaign slices, not required in this seed"),
            ["WorldSettlements.CanalWindowChanged"] = new(
                EventContractHealthKind.FutureContract,
                "canal window is a route/economy signal reserved for fuller trade/order use"),
            ["WorldSettlements.ComplianceModeShifted"] = new(
                EventContractHealthKind.FutureContract,
                "compliance mode is reserved for fuller regime/local compliance integration"),
            ["WorldSettlements.SeasonalFestivalArrived"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "festival arrival is currently a calendar/public-life receipt"),
            ["WorldSettlements.SettlementPressureChanged"] = new(
                EventContractHealthKind.ProjectionOnlyReceipt,
                "settlement pressure changed is world-owned surface evidence after local mutation")
        };

    private enum EventContractHealthKind
    {
        ProjectionOnlyReceipt,
        FutureContract,
        DormantSeededPath,
        AcceptanceTestGap,
        AlignmentBug,
        Unclassified
    }

    private sealed record EventContractHealthClassification(
        EventContractHealthKind Kind,
        string Rationale);

    private sealed record SettlementMonthSample(
        int Month,
        GameDate Date,
        SettlementId SettlementId,
        string SettlementName,
        int Security,
        int Prosperity,
        int? CommonerDistress,
        int? MigrationPressure,
        int? BanditThreat,
        int? RoutePressure,
        int? DisorderPressure,
        int? SuppressionDemand,
        int? ForceReadiness,
        int? CommandCapacity,
        int? CampaignSupplyState,
        int? CampaignFrontPressure,
        int? CampaignMoraleState,
        bool? CampaignIsActive);

    private sealed class EventConsumptionProbe
    {
        private readonly Dictionary<(string Publisher, string EventType, string Consumer), int> _handled = new();

        public void Record(string consumer, IReadOnlyCollection<string> consumedEvents, IReadOnlyList<IDomainEvent> events)
        {
            HashSet<string> consumed = consumedEvents.ToHashSet(StringComparer.Ordinal);
            foreach (IDomainEvent domainEvent in events)
            {
                if (!consumed.Contains(domainEvent.EventType) ||
                    string.Equals(domainEvent.ModuleKey, consumer, StringComparison.Ordinal))
                {
                    continue;
                }

                Increment(_handled, (domainEvent.ModuleKey, domainEvent.EventType, consumer));
            }
        }

        public int CountHandled(string publisher, string eventType)
        {
            return _handled
                .Where(pair => string.Equals(pair.Key.Publisher, publisher, StringComparison.Ordinal)
                    && string.Equals(pair.Key.EventType, eventType, StringComparison.Ordinal))
                .Sum(static pair => pair.Value);
        }

        public IEnumerable<string> GetConsumers(string publisher, string eventType)
        {
            return _handled
                .Where(pair => string.Equals(pair.Key.Publisher, publisher, StringComparison.Ordinal)
                    && string.Equals(pair.Key.EventType, eventType, StringComparison.Ordinal))
                .Select(static pair => pair.Key.Consumer)
                .Distinct(StringComparer.Ordinal);
        }
    }

    private sealed class DiagnosticModuleRunner : IModuleRunner
    {
        private readonly IModuleRunner _inner;
        private readonly EventConsumptionProbe _probe;

        public DiagnosticModuleRunner(IModuleRunner inner, EventConsumptionProbe probe)
        {
            _inner = inner;
            _probe = probe;
        }

        public string ModuleKey => _inner.ModuleKey;

        public string StateNamespace => _inner.StateNamespace;

        public int ModuleSchemaVersion => _inner.ModuleSchemaVersion;

        public SimulationPhase Phase => _inner.Phase;

        public int ExecutionOrder => _inner.ExecutionOrder;

        public IReadOnlyCollection<SimulationCadenceBand> CadenceBands => _inner.CadenceBands;

        public FeatureMode DefaultMode => _inner.DefaultMode;

        public Type StateType => _inner.StateType;

        public IReadOnlyCollection<string> AcceptedCommands => _inner.AcceptedCommands;

        public IReadOnlyCollection<string> PublishedEvents => _inner.PublishedEvents;

        public IReadOnlyCollection<string> ConsumedEvents => _inner.ConsumedEvents;

        public object CreateInitialState()
        {
            return _inner.CreateInitialState();
        }

        public void RegisterQueries(object state, QueryRegistry queries)
        {
            _inner.RegisterQueries(state, queries);
        }

        public void RunXun(ModuleExecutionContext context, object state)
        {
            _inner.RunXun(context, state);
        }

        public void RunMonth(ModuleExecutionContext context, object state)
        {
            _inner.RunMonth(context, state);
        }

        public void HandleEvents(ModuleExecutionContext context, object state, IReadOnlyList<IDomainEvent> events)
        {
            if (ConsumedEvents.Count > 0)
            {
                _probe.Record(ModuleKey, ConsumedEvents, events);
            }

            _inner.HandleEvents(context, state, events);
        }

        public PlayerCommandResult HandleCommand(ModuleExecutionContext context, object state, PlayerCommandRequest command)
        {
            return _inner.HandleCommand(context, state, command);
        }
    }
}
