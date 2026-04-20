using System;
using System.Collections.Generic;
using System.Linq;
using Zongzu.Contracts;
using Zongzu.Kernel;

namespace Zongzu.Modules.FamilyCore;

public sealed class FamilyCoreModule : ModuleRunner<FamilyCoreState>
{
    private const int AdultAgeMonths = 16 * 12;
    private const int SecureHeirAgeMonths = 20 * 12;
    private const int ElderAgeMonths = 55 * 12;
    private const int DeathAgeMonths = 72 * 12;
    private const int InfantAgeMonths = 2 * 12;

    private static readonly string[] CommandNames =
    [
        PlayerCommandNames.ArrangeMarriage,
        PlayerCommandNames.DesignateHeirPolicy,
        PlayerCommandNames.SupportSeniorBranch,
        PlayerCommandNames.OrderFormalApology,
        PlayerCommandNames.PermitBranchSeparation,
        PlayerCommandNames.SuspendClanRelief,
        PlayerCommandNames.InviteClanEldersMediation,
        PlayerCommandNames.InviteClanEldersPubliclyBroker,
    ];

    private static readonly string[] EventNames =
    [
        FamilyCoreEventNames.ClanPrestigeAdjusted,
        FamilyCoreEventNames.FamilyMembersAged,
        FamilyCoreEventNames.LineageDisputeHardened,
        FamilyCoreEventNames.LineageMediationOpened,
        FamilyCoreEventNames.BranchSeparationApproved,
        FamilyCoreEventNames.MarriageAllianceArranged,
        FamilyCoreEventNames.BirthRegistered,
        FamilyCoreEventNames.ClanMemberDied,
        FamilyCoreEventNames.HeirSecurityWeakened,
    ];

    private static readonly string[] ConsumedEventNames =
    [
        WarfareCampaignEventNames.CampaignMobilized,
        WarfareCampaignEventNames.CampaignPressureRaised,
        WarfareCampaignEventNames.CampaignSupplyStrained,
        WarfareCampaignEventNames.CampaignAftermathRegistered,
    ];

    public override string ModuleKey => KnownModuleKeys.FamilyCore;

    public override int ModuleSchemaVersion => 4;

    public override SimulationPhase Phase => SimulationPhase.FamilyStructure;

    public override int ExecutionOrder => 300;

    public override IReadOnlyCollection<SimulationCadenceBand> CadenceBands => SimulationCadencePresets.XunAndMonth;

    public override IReadOnlyCollection<string> AcceptedCommands => CommandNames;

    public override IReadOnlyCollection<string> PublishedEvents => EventNames;

    public override IReadOnlyCollection<string> ConsumedEvents => ConsumedEventNames;

    public override FamilyCoreState CreateInitialState()
    {
        return new FamilyCoreState();
    }

    public override void RegisterQueries(FamilyCoreState state, QueryRegistry queries)
    {
        queries.Register<IFamilyCoreQueries>(new FamilyCoreQueries(state, queries));
    }

    public override void RunXun(ModuleExecutionScope<FamilyCoreState> scope)
    {
        IWorldSettlementsQueries settlementsQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPersonRegistryQueries registryQueries = scope.GetRequiredQuery<IPersonRegistryQueries>();
        GameDate currentDate = scope.Context.CurrentDate;

        foreach (ClanStateData clan in scope.State.Clans.OrderBy(static clan => clan.Id.Value))
        {
            SettlementSnapshot homeSettlement = settlementsQueries.GetRequiredSettlement(clan.HomeSettlementId);
            FamilyMonthSignals signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);
            ApplyXunClanPulse(scope.Context.CurrentXun, clan, signals, homeSettlement);
        }
    }

    public override void RunMonth(ModuleExecutionScope<FamilyCoreState> scope)
    {
        IWorldSettlementsQueries settlementsQueries = scope.GetRequiredQuery<IWorldSettlementsQueries>();
        IPersonRegistryQueries registryQueries = scope.GetRequiredQuery<IPersonRegistryQueries>();
        GameDate currentDate = scope.Context.CurrentDate;

        // Age progression for persons not yet registered in PersonRegistry
        // (clan-internal mirror). Registered persons are aged by PersonRegistry
        // in Phase 0 and FamilyCore reads via IPersonRegistryQueries.GetAgeMonths.
        // See PERSON_OWNERSHIP_RULES.md — authority flows from PersonRegistry.
        int peopleAged = 0;
        foreach (FamilyPersonState person in scope.State.People.OrderBy(static person => person.Id.Value))
        {
            if (!IsPersonAlive(person, registryQueries))
            {
                continue;
            }

            if (registryQueries.TryGetPerson(person.Id, out _))
            {
                // Registry is authoritative for this person; local mirror is unused for age.
                peopleAged += 1;
                continue;
            }

            person.AgeMonths += 1;
            peopleAged += 1;
        }

        if (peopleAged > 0)
        {
            scope.RecordDiff($"宗房册中在世之人本月俱各长一月，共{peopleAged}人。");
            scope.Emit(FamilyCoreEventNames.FamilyMembersAged, $"宗房人口本月俱各长一月，共{peopleAged}人。");
        }

        foreach (ClanStateData clan in scope.State.Clans.OrderBy(static clan => clan.Id.Value))
        {
            SettlementSnapshot homeSettlement = settlementsQueries.GetRequiredSettlement(clan.HomeSettlementId);
            FamilyMonthSignals signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);

            if (TryArrangeAutonomousMarriage(scope, clan, signals))
            {
                signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);
            }

            bool hadDeathThisMonth = TryResolveClanDeath(scope, clan, signals, registryQueries);
            signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);

            bool hadBirthThisMonth = TryResolveClanBirth(scope, clan, signals);
            if (hadBirthThisMonth)
            {
                signals = AnalyzeClan(scope.State, clan, registryQueries, currentDate);
            }

            int oldPrestige = clan.Prestige;
            int oldSupportReserve = clan.SupportReserve;
            int oldBranchTension = clan.BranchTension;
            int oldInheritancePressure = clan.InheritancePressure;
            int oldSeparationPressure = clan.SeparationPressure;
            int oldMediationMomentum = clan.MediationMomentum;
            int oldMarriageAlliancePressure = clan.MarriageAlliancePressure;
            int oldMarriageAllianceValue = clan.MarriageAllianceValue;
            int oldHeirSecurity = clan.HeirSecurity;
            int oldReproductivePressure = clan.ReproductivePressure;
            int oldMourningLoad = clan.MourningLoad;

            int pressureScore = homeSettlement.Security + homeSettlement.Prosperity;
            if (pressureScore >= 130)
            {
                clan.Prestige = Math.Min(100, clan.Prestige + 1);
            }
            else if (pressureScore < 95)
            {
                clan.Prestige = Math.Max(0, clan.Prestige - 1);
            }

            clan.SupportReserve = Math.Clamp(clan.SupportReserve + ((homeSettlement.Prosperity - 50) / 10), 0, 100);

            clan.MarriageAlliancePressure = Math.Clamp(
                clan.MarriageAlliancePressure + ComputeMarriageAlliancePressureDelta(clan, signals),
                0,
                100);
            clan.ReproductivePressure = Math.Clamp(
                clan.ReproductivePressure + ComputeReproductivePressureDelta(clan, signals),
                0,
                100);
            clan.HeirSecurity = ComputeHeirSecurity(clan, signals);
            clan.MourningLoad = Math.Max(0, clan.MourningLoad - (hadDeathThisMonth ? 0 : 1));

            clan.BranchTension = Math.Clamp(
                clan.BranchTension + ComputeBranchTensionDelta(clan, homeSettlement),
                0,
                100);
            clan.InheritancePressure = Math.Clamp(
                clan.InheritancePressure + ComputeInheritancePressureDelta(clan, signals),
                0,
                100);
            clan.SeparationPressure = Math.Clamp(
                clan.SeparationPressure + ComputeSeparationPressureDelta(clan),
                0,
                100);
            clan.MediationMomentum = Math.Max(0, clan.MediationMomentum - 1);
            clan.BranchFavorPressure = Math.Max(0, clan.BranchFavorPressure - 1);
            clan.ReliefSanctionPressure = Math.Max(0, clan.ReliefSanctionPressure - 1);

            if (ClanStateUnchanged(
                    clan,
                    oldPrestige,
                    oldSupportReserve,
                    oldBranchTension,
                    oldInheritancePressure,
                    oldSeparationPressure,
                    oldMediationMomentum,
                    oldMarriageAlliancePressure,
                    oldMarriageAllianceValue,
                    oldHeirSecurity,
                    oldReproductivePressure,
                    oldMourningLoad))
            {
                continue;
            }

            scope.RecordDiff(
                $"{clan.ClanName}门望{clan.Prestige}，可支宗力{clan.SupportReserve}，婚议之压{clan.MarriageAlliancePressure}，承祧稳度{clan.HeirSecurity}，添丁之望{clan.ReproductivePressure}，丧服之重{clan.MourningLoad}，房支争力{clan.BranchTension}，分房之议{clan.SeparationPressure}。",
                clan.Id.Value.ToString());
            scope.Emit(FamilyCoreEventNames.ClanPrestigeAdjusted, $"{clan.ClanName}因门内盛衰与乡里形势而声势消长。", clan.Id.Value.ToString());

            if (oldHeirSecurity >= 40 && clan.HeirSecurity < 40)
            {
                scope.Emit(FamilyCoreEventNames.HeirSecurityWeakened, $"{clan.ClanName}承祧未稳，祠堂已起后议。", clan.Id.Value.ToString());
            }

            if (oldBranchTension < 55 && clan.BranchTension >= 55)
            {
                scope.Emit(FamilyCoreEventNames.LineageDisputeHardened, $"{clan.ClanName}祠堂争端渐炽。", clan.Id.Value.ToString());
            }

            if (oldMediationMomentum < 45 && clan.MediationMomentum >= 45)
            {
                scope.Emit(FamilyCoreEventNames.LineageMediationOpened, $"{clan.ClanName}已请族老开议调停。", clan.Id.Value.ToString());
            }

            if (oldSeparationPressure >= 55
                && clan.SeparationPressure <= 35
                && string.Equals(clan.LastConflictCommandCode, PlayerCommandNames.PermitBranchSeparation, StringComparison.Ordinal))
            {
                scope.Emit(FamilyCoreEventNames.BranchSeparationApproved, $"{clan.ClanName}分房之议已定。", clan.Id.Value.ToString());
            }
        }
    }

    public override void HandleEvents(ModuleEventHandlingScope<FamilyCoreState> scope)
    {
        IReadOnlyList<WarfareCampaignEventBundle> warfareEvents = WarfareCampaignEventBundler.Build(scope.Events);
        if (warfareEvents.Count == 0)
        {
            return;
        }

        Dictionary<SettlementId, CampaignFrontSnapshot> campaignsBySettlement = scope.GetRequiredQuery<IWarfareCampaignQueries>()
            .GetCampaigns()
            .ToDictionary(static campaign => campaign.AnchorSettlementId, static campaign => campaign);

        foreach (WarfareCampaignEventBundle bundle in warfareEvents)
        {
            if (!campaignsBySettlement.TryGetValue(bundle.SettlementId, out CampaignFrontSnapshot? campaign))
            {
                continue;
            }

            ClanStateData[] clans = scope.State.Clans
                .Where(clan => clan.HomeSettlementId == bundle.SettlementId)
                .OrderBy(static clan => clan.Id.Value)
                .ToArray();
            if (clans.Length == 0)
            {
                continue;
            }

            int prestigeDelta = ComputeCampaignPrestigeDelta(bundle, campaign);
            int supportDelta = ComputeCampaignSupportDelta(bundle, campaign);

            foreach (ClanStateData clan in clans)
            {
                clan.Prestige = Math.Clamp(clan.Prestige + prestigeDelta, 0, 100);
                clan.SupportReserve = Math.Clamp(clan.SupportReserve + supportDelta, 0, 100);
            }

            scope.RecordDiff(
                $"{campaign.AnchorSettlementName}战后余波牵动宗房声势：门望{prestigeDelta:+#;-#;0}，宗力{supportDelta:+#;-#;0}。{campaign.LastAftermathSummary}",
                bundle.SettlementId.Value.ToString());
            scope.Emit(FamilyCoreEventNames.ClanPrestigeAdjusted, $"{campaign.AnchorSettlementName}战后余波改动了宗房声势。", bundle.SettlementId.Value.ToString());
        }
    }

    private static void ApplyXunClanPulse(
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

    private static FamilyMonthSignals AnalyzeClan(
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

    private static bool IsPersonAlive(FamilyPersonState person, IPersonRegistryQueries registryQueries)
    {
        if (registryQueries.TryGetPerson(person.Id, out PersonRecord record))
        {
            return record.IsAlive;
        }

        return person.IsAlive;
    }

    private static int GetAgeMonths(FamilyPersonState person, IPersonRegistryQueries registryQueries, GameDate currentDate)
    {
        int registryAge = registryQueries.GetAgeMonths(person.Id, currentDate);
        return registryAge >= 0 ? registryAge : person.AgeMonths;
    }

    private static bool TryArrangeAutonomousMarriage(ModuleExecutionScope<FamilyCoreState> scope, ClanStateData clan, FamilyMonthSignals signals)
    {
        if (signals.AdultCount == 0
            || clan.MarriageAllianceValue >= 45
            || clan.MarriageAlliancePressure < 72
            || clan.MourningLoad >= 18
            || clan.SupportReserve < 40)
        {
            return false;
        }

        clan.MarriageAllianceValue = 48;
        clan.MarriageAlliancePressure = Math.Max(0, clan.MarriageAlliancePressure - 18);
        clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + 10, 0, 100);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - 3);
        clan.LastLifecycleOutcome = "门内先自议亲，暂把承祧与分房的后议压住。";
        clan.LastLifecycleTrace = $"{clan.ClanName}门内自行议定婚事，先借姻亲稳一稳香火与房支人情。";

        scope.RecordDiff($"{clan.ClanName}门内先自议亲，婚事已定，祠堂与家内都盼其稳住香火。", clan.Id.Value.ToString());
        scope.Emit(FamilyCoreEventNames.MarriageAllianceArranged, $"{clan.ClanName}门内议亲已定。", clan.Id.Value.ToString());
        return true;
    }

    private static bool TryResolveClanBirth(ModuleExecutionScope<FamilyCoreState> scope, ClanStateData clan, FamilyMonthSignals signals)
    {
        if (signals.AdultCount == 0
            || clan.MarriageAllianceValue < 55
            || clan.ReproductivePressure < 52
            || clan.MourningLoad >= 18
            || signals.InfantCount > 0)
        {
            return false;
        }

        PersonId newbornId = KernelIdAllocator.NextPerson(scope.Context.KernelState);
        string newbornName = $"{clan.ClanName}新丁{newbornId.Value}";
        // Register identity first (PersonRegistry is authoritative for age and
        // IsAlive since Phase 2b). FamilyCore still owns clan-scoped kinship
        // and personality.
        IPersonRegistryCommands registryCommands = scope.GetRequiredQuery<IPersonRegistryCommands>();
        registryCommands.Register(
            scope.Context,
            newbornId,
            newbornName,
            scope.Context.CurrentDate,
            PersonGender.Unspecified,
            FidelityRing.Local);

        scope.State.People.Add(new FamilyPersonState
        {
            Id = newbornId,
            ClanId = clan.Id,
            GivenName = newbornName,
            AgeMonths = 0,
            IsAlive = true,
            BranchPosition = BranchPosition.DependentKin,
            Ambition = 50,
            Prudence = 50,
            Loyalty = 50,
            Sociability = 50,
        });

        clan.ReproductivePressure = Math.Max(0, clan.ReproductivePressure - 26);
        clan.SupportReserve = Math.Max(0, clan.SupportReserve - 4);
        clan.HeirSecurity = Math.Clamp(Math.Max(clan.HeirSecurity, 32) + 4, 0, 100);
        clan.LastLifecycleOutcome = "门内添丁，香火暂得续望，但家中口粮与抚养之费也随之加重。";
        clan.LastLifecycleTrace = $"{clan.ClanName}门内新添襁褓之儿，堂上暂缓继嗣焦心，家内却更添抚养之累。";

        scope.RecordDiff($"{clan.ClanName}门内添丁，襁褓初定，家中口粮、抚养与香火之望一并上肩。", clan.Id.Value.ToString());
        scope.Emit(FamilyCoreEventNames.BirthRegistered, $"{clan.ClanName}门内添丁。", clan.Id.Value.ToString());
        return true;
    }

    private static bool TryResolveClanDeath(
        ModuleExecutionScope<FamilyCoreState> scope,
        ClanStateData clan,
        FamilyMonthSignals signals,
        IPersonRegistryQueries registryQueries)
    {
        FamilyPersonAge? deathEntry = null;
        foreach (FamilyPersonAge entry in signals.LivingPeople
            .Where(entry => entry.AgeMonths >= DeathAgeMonths)
            .OrderByDescending(static entry => entry.AgeMonths)
            .ThenBy(static entry => entry.Person.Id.Value))
        {
            deathEntry = entry;
            break;
        }

        if (deathEntry is null)
        {
            return false;
        }

        FamilyPersonState deathTarget = deathEntry.Value.Person;
        int deathAgeMonths = deathEntry.Value.AgeMonths;
        // PersonRegistry is the authoritative death write since Phase 2c/2d.
        // FamilyCore no longer writes the local IsAlive mirror; all readers
        // (simulation loop, snapshots, projections) consult registryQueries
        // first and fall back to the local flag only for unregistered persons.
        // See PERSON_OWNERSHIP_RULES.md.
        IPersonRegistryCommands registryCommands = scope.GetRequiredQuery<IPersonRegistryCommands>();
        registryCommands.MarkDeceased(scope.Context, deathTarget.Id);
        bool wasHeir = clan.HeirPersonId == deathTarget.Id;
        if (wasHeir)
        {
            clan.HeirPersonId = null;
        }

        clan.MourningLoad = Math.Clamp(clan.MourningLoad + 24, 0, 100);
        clan.InheritancePressure = Math.Clamp(clan.InheritancePressure + (wasHeir ? 18 : 8), 0, 100);
        clan.SeparationPressure = Math.Clamp(clan.SeparationPressure + (wasHeir ? 8 : 2), 0, 100);
        if (deathAgeMonths < AdultAgeMonths)
        {
            clan.ReproductivePressure = Math.Clamp(clan.ReproductivePressure + 10, 0, 100);
        }

        clan.LastLifecycleOutcome = wasHeir
            ? "承祧之人忽逝，举哀之外，又添后议与房支觊觎。"
            : "门内举哀，堂上先忙丧服与祭次，旁事都得让后。";
        clan.LastLifecycleTrace = wasHeir
            ? $"{clan.ClanName}失了承祧之人，香火、后议与房支人心一时俱紧。"
            : $"{clan.ClanName}门内有长者亡故，家中先忙丧服、发引与祭次。";

        scope.RecordDiff(
            wasHeir
                ? $"{clan.ClanName}承祧之人身故，门内举哀，继嗣之议与房支争执随即翻起。"
                : $"{clan.ClanName}门内举哀，丧服与祭次先压住家中诸事。",
            clan.Id.Value.ToString());
        // Entity key is PersonId so PersonRegistry can consolidate this into
        // the canonical PersonDeceased. See PERSON_OWNERSHIP_RULES.md.
        scope.Emit(FamilyCoreEventNames.ClanMemberDied, $"{clan.ClanName}门内举哀。", deathTarget.Id.Value.ToString());
        return true;
    }

    private static bool ClanStateUnchanged(
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

    private static int ComputeMarriageAlliancePressureDelta(ClanStateData clan, FamilyMonthSignals signals)
    {
        int delta = 0;
        delta += signals.AdultCount <= 1 && clan.MarriageAllianceValue < 45 ? 2 : 0;
        delta += clan.HeirSecurity < 40 ? 1 : 0;
        delta += signals.ChildCount == 0 ? 1 : 0;
        delta -= clan.MarriageAllianceValue >= 45 ? 2 : 0;
        delta -= clan.MourningLoad >= 18 ? 1 : 0;
        return delta;
    }

    private static int ComputeReproductivePressureDelta(ClanStateData clan, FamilyMonthSignals signals)
    {
        int delta = 0;
        delta += clan.MarriageAllianceValue >= 45 && signals.ChildCount == 0 ? 2 : 0;
        delta += clan.HeirSecurity < 45 ? 1 : 0;
        delta -= signals.InfantCount > 0 ? 3 : 0;
        delta -= signals.ChildCount > 0 ? 1 : 0;
        delta -= clan.MourningLoad >= 18 ? 1 : 0;
        return delta;
    }

    private static int ComputeHeirSecurity(ClanStateData clan, FamilyMonthSignals signals)
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

    private static int ComputeBranchTensionDelta(ClanStateData clan, SettlementSnapshot settlement)
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

    private static int ComputeInheritancePressureDelta(ClanStateData clan, FamilyMonthSignals signals)
    {
        int delta = 0;
        delta += signals.LivingHeir is null ? 2 : 1;
        delta += clan.HeirSecurity < 40 ? 1 : 0;
        delta += clan.Prestige >= 60 ? 1 : 0;
        delta -= clan.MediationMomentum >= 50 ? 1 : 0;
        return delta;
    }

    private static int ComputeSeparationPressureDelta(ClanStateData clan)
    {
        int delta = 0;
        delta += clan.BranchTension >= 65 ? 2 : clan.BranchTension >= 45 ? 1 : 0;
        delta += clan.ReliefSanctionPressure >= 45 ? 1 : 0;
        delta += clan.MourningLoad >= 22 ? 1 : 0;
        delta -= clan.MediationMomentum >= 55 ? 1 : 0;
        return delta;
    }

    private static int ComputeCampaignPrestigeDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
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

    private static int ComputeCampaignSupportDelta(WarfareCampaignEventBundle bundle, CampaignFrontSnapshot campaign)
    {
        int delta = 0;
        delta -= bundle.CampaignMobilized ? 2 : 0;
        delta -= bundle.CampaignPressureRaised ? 1 : 0;
        delta -= bundle.CampaignSupplyStrained ? 2 : 0;
        delta -= bundle.CampaignAftermathRegistered ? 1 : 0;
        delta -= Math.Max(0, campaign.MobilizedForceCount - 24) / 24;
        return Math.Clamp(delta, -8, 0);
    }

    private sealed class FamilyCoreQueries : IFamilyCoreQueries
    {
        private readonly FamilyCoreState _state;
        private readonly QueryRegistry _queries;

        public FamilyCoreQueries(FamilyCoreState state, QueryRegistry queries)
        {
            _state = state;
            _queries = queries;
        }

        private bool IsPersonAlive(FamilyPersonState person)
        {
            IPersonRegistryQueries registry = _queries.GetRequired<IPersonRegistryQueries>();
            if (registry.TryGetPerson(person.Id, out PersonRecord record))
            {
                return record.IsAlive;
            }

            return person.IsAlive;
        }

        public ClanSnapshot GetRequiredClan(ClanId clanId)
        {
            ClanStateData clan = _state.Clans.Single(clan => clan.Id == clanId);
            return Clone(_state, clan);
        }

        public IReadOnlyList<ClanSnapshot> GetClans()
        {
            return _state.Clans
                .OrderBy(static clan => clan.Id.Value)
                .Select(clan => Clone(_state, clan))
                .ToArray();
        }

        public FamilyPersonSnapshot? FindPerson(PersonId personId)
        {
            FamilyPersonState? person = _state.People.SingleOrDefault(person => person.Id == personId);
            return person is null ? null : ClonePerson(person);
        }

        public IReadOnlyList<FamilyPersonSnapshot> GetClanMembers(ClanId clanId)
        {
            return _state.People
                .Where(person => person.ClanId == clanId)
                .OrderBy(static person => person.Id.Value)
                .Select(ClonePerson)
                .ToArray();
        }

        private FamilyPersonSnapshot ClonePerson(FamilyPersonState person)
        {
            return new FamilyPersonSnapshot
            {
                Id = person.Id,
                ClanId = person.ClanId,
                GivenName = person.GivenName,
                AgeMonths = person.AgeMonths,
                IsAlive = IsPersonAlive(person),
                BranchPosition = person.BranchPosition,
                SpouseId = person.SpouseId,
                FatherId = person.FatherId,
                MotherId = person.MotherId,
                ChildrenIds = person.ChildrenIds.ToArray(),
                Ambition = person.Ambition,
                Prudence = person.Prudence,
                Loyalty = person.Loyalty,
                Sociability = person.Sociability,
            };
        }

        private ClanSnapshot Clone(FamilyCoreState state, ClanStateData clan)
        {
            int infantCount = state.People.Count(person =>
                person.ClanId == clan.Id
                && IsPersonAlive(person)
                && person.AgeMonths <= InfantAgeMonths);

            return new ClanSnapshot
            {
                Id = clan.Id,
                ClanName = clan.ClanName,
                HomeSettlementId = clan.HomeSettlementId,
                Prestige = clan.Prestige,
                SupportReserve = clan.SupportReserve,
                HeirPersonId = clan.HeirPersonId,
                BranchTension = clan.BranchTension,
                InheritancePressure = clan.InheritancePressure,
                SeparationPressure = clan.SeparationPressure,
                MediationMomentum = clan.MediationMomentum,
                BranchFavorPressure = clan.BranchFavorPressure,
                ReliefSanctionPressure = clan.ReliefSanctionPressure,
                MarriageAlliancePressure = clan.MarriageAlliancePressure,
                MarriageAllianceValue = clan.MarriageAllianceValue,
                HeirSecurity = clan.HeirSecurity,
                ReproductivePressure = clan.ReproductivePressure,
                MourningLoad = clan.MourningLoad,
                InfantCount = infantCount,
                LastConflictCommandCode = clan.LastConflictCommandCode,
                LastConflictCommandLabel = clan.LastConflictCommandLabel,
                LastConflictOutcome = clan.LastConflictOutcome,
                LastConflictTrace = clan.LastConflictTrace,
                LastLifecycleCommandCode = clan.LastLifecycleCommandCode,
                LastLifecycleCommandLabel = clan.LastLifecycleCommandLabel,
                LastLifecycleOutcome = clan.LastLifecycleOutcome,
                LastLifecycleTrace = clan.LastLifecycleTrace,
            };
        }
    }

    private readonly record struct FamilyMonthSignals(
        IReadOnlyList<FamilyPersonAge> LivingPeople,
        FamilyPersonAge? LivingHeir,
        int AdultCount,
        int ChildCount,
        int ElderCount,
        int InfantCount);

    private readonly record struct FamilyPersonAge(FamilyPersonState Person, int AgeMonths);
}
